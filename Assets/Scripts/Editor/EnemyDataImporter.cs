using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using StopDefence.GameData;
using UnityEditor;
using UnityEngine;

namespace StopDefence.Editor
{
    public static class EnemyDataImporter
    {
        private const string SourceRelativePath = "GameData/GameData_Enemy.xlsx";
        private const string OutputAssetPath = "Assets/GameData/EnemyDatabase.asset";
        private const int HeaderRow = 1;
        private const int FirstDataRow = 2;

        private const string EnemyInfoSheetName = "EnemyInfo";
        private const string SpawnSheetName = "Spawn";

        private static readonly string[] EnemyInfoColumns =
        {
            "EnemyId",
            "DisplayName",
            "DataSheet",
            "EnemyType",
            "PrefabPath",
            "MaxLevel"
        };

        private static readonly string[] StatColumns =
        {
            "LV",
            "HP",
            "ATK",
            "ATK Speed",
            "ATK Range",
            "move speed",
            "drop coin"
        };

        private static readonly string[] SpawnColumns =
        {
            "Time",
            "EnemyId",
            "Level",
            "SpawnPoint",
            "Count",
            "Interval"
        };

        [MenuItem("Tools/Game Data/Import Enemy Data")]
        public static void Import()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root could not be resolved.");
            string sourcePath = Path.Combine(projectRoot, SourceRelativePath);

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Enemy data workbook was not found.", sourcePath);
            }

            Dictionary<string, XlsxSheet> sheets = XlsxWorkbook.ReadWorksheets(sourcePath)
                .ToDictionary(sheet => sheet.Name, StringComparer.OrdinalIgnoreCase);

            if (!sheets.TryGetValue(EnemyInfoSheetName, out XlsxSheet enemyInfoSheet))
            {
                throw new InvalidDataException($"Worksheet '{EnemyInfoSheetName}' was not found.");
            }

            if (!sheets.TryGetValue(SpawnSheetName, out XlsxSheet spawnSheet))
            {
                throw new InvalidDataException($"Worksheet '{SpawnSheetName}' was not found.");
            }

            var enemies = new List<EnemyData>();
            var enemyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> infoColumns = BuildColumnMap(enemyInfoSheet.GetRow(HeaderRow));
            ValidateRequiredColumns(EnemyInfoSheetName, infoColumns, EnemyInfoColumns);

            foreach (int rowNumber in enemyInfoSheet.RowNumbers.Where(row => row >= FirstDataRow))
            {
                IReadOnlyDictionary<int, string> infoRow = enemyInfoSheet.GetRow(rowNumber);
                string enemyId = GetValue(infoRow, infoColumns, "EnemyId").Trim();
                if (string.IsNullOrEmpty(enemyId))
                {
                    continue;
                }

                if (!enemyIds.Add(enemyId))
                {
                    throw new InvalidDataException(
                        $"Duplicate enemy ID '{enemyId}' in worksheet '{EnemyInfoSheetName}', row {rowNumber}.");
                }

                string displayName = GetRequiredValue(
                    infoRow, infoColumns, "DisplayName", EnemyInfoSheetName, rowNumber);
                string dataSheetName = GetRequiredValue(
                    infoRow, infoColumns, "DataSheet", EnemyInfoSheetName, rowNumber);
                string enemyTypeText = GetRequiredValue(
                    infoRow, infoColumns, "EnemyType", EnemyInfoSheetName, rowNumber);
                string prefabPath = GetRequiredValue(
                    infoRow, infoColumns, "PrefabPath", EnemyInfoSheetName, rowNumber);
                int maxLevel = ParseRequiredInt(
                    infoRow, infoColumns, "MaxLevel", EnemyInfoSheetName, rowNumber);

                if (!Enum.TryParse(enemyTypeText, true, out EnemyType enemyType))
                {
                    throw new InvalidDataException(
                        $"Unknown EnemyType '{enemyTypeText}' in worksheet '{EnemyInfoSheetName}', row {rowNumber}.");
                }

                if (!sheets.TryGetValue(dataSheetName, out XlsxSheet statSheet))
                {
                    throw new InvalidDataException(
                        $"Stat worksheet '{dataSheetName}' for enemy '{enemyId}' was not found.");
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    throw new InvalidDataException(
                        $"Enemy prefab '{prefabPath}' for enemy '{enemyId}' was not found.");
                }

                List<EnemyLevelData> levels = ParseLevels(statSheet);
                if (maxLevel != levels.Count || levels[levels.Count - 1].Level != maxLevel)
                {
                    throw new InvalidDataException(
                        $"Enemy '{enemyId}' declares MaxLevel {maxLevel}, but worksheet '{dataSheetName}' contains levels 1-{levels[levels.Count - 1].Level} ({levels.Count} rows).");
                }

                enemies.Add(new EnemyData(enemyId, displayName, enemyType, prefab, levels));
            }

            if (enemies.Count == 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{EnemyInfoSheetName}' does not contain any enemies.");
            }

            Dictionary<string, int> spawnColumns = BuildColumnMap(spawnSheet.GetRow(HeaderRow));
            ValidateRequiredColumns(SpawnSheetName, spawnColumns, SpawnColumns);
            var spawnSchedule = new List<EnemySpawnData>();

            foreach (int rowNumber in spawnSheet.RowNumbers.Where(row => row >= FirstDataRow))
            {
                IReadOnlyDictionary<int, string> row = spawnSheet.GetRow(rowNumber);
                string timeText = GetValue(row, spawnColumns, "Time").Trim();
                if (string.IsNullOrEmpty(timeText))
                {
                    continue;
                }

                float time = ParseRequiredFloat(row, spawnColumns, "Time", SpawnSheetName, rowNumber);
                string enemyId = GetRequiredValue(
                    row, spawnColumns, "EnemyId", SpawnSheetName, rowNumber);
                int level = ParseRequiredInt(row, spawnColumns, "Level", SpawnSheetName, rowNumber);
                int spawnPoint = ParseRequiredInt(
                    row, spawnColumns, "SpawnPoint", SpawnSheetName, rowNumber);
                int count = ParseRequiredInt(row, spawnColumns, "Count", SpawnSheetName, rowNumber);
                float interval = ParseRequiredFloat(
                    row, spawnColumns, "Interval", SpawnSheetName, rowNumber);

                EnemyData enemy = enemies.Find(value =>
                    string.Equals(value.Id, enemyId, StringComparison.OrdinalIgnoreCase));
                if (enemy == null)
                {
                    throw new InvalidDataException(
                        $"Unknown EnemyId '{enemyId}' in worksheet '{SpawnSheetName}', row {rowNumber}.");
                }

                if (time < 0f || spawnPoint < 1 || count < 1 || interval < 0f)
                {
                    throw new InvalidDataException(
                        $"Spawn values must satisfy Time >= 0, SpawnPoint >= 1, Count >= 1 and Interval >= 0 in row {rowNumber}.");
                }

                if (enemy.GetLevel(level) == null)
                {
                    throw new InvalidDataException(
                        $"Enemy '{enemyId}' has no level {level} in worksheet '{SpawnSheetName}', row {rowNumber}.");
                }

                spawnSchedule.Add(new EnemySpawnData(
                    time, enemyId, level, spawnPoint, count, interval));
            }

            if (spawnSchedule.Count == 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{SpawnSheetName}' does not contain any spawn rows.");
            }

            spawnSchedule = spawnSchedule.OrderBy(value => value.Time).ToList();

            EnemyDatabase database = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(OutputAssetPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<EnemyDatabase>();
                AssetDatabase.CreateAsset(database, OutputAssetPath);
            }

            database.ReplaceData(enemies, spawnSchedule);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = database;

            Debug.Log(
                $"[EnemyDataImporter] Imported {enemies.Count} enemies and " +
                $"{spawnSchedule.Count} spawn groups from '{SourceRelativePath}' into " +
                $"'{OutputAssetPath}'.");
        }

        private static List<EnemyLevelData> ParseLevels(XlsxSheet sheet)
        {
            Dictionary<string, int> columns = BuildColumnMap(sheet.GetRow(HeaderRow));
            ValidateRequiredColumns(sheet.Name, columns, StatColumns);
            var levels = new List<EnemyLevelData>();
            var levelNumbers = new HashSet<int>();

            foreach (int rowNumber in sheet.RowNumbers.Where(row => row >= FirstDataRow))
            {
                IReadOnlyDictionary<int, string> row = sheet.GetRow(rowNumber);
                string levelText = GetValue(row, columns, "LV").Trim();
                if (string.IsNullOrEmpty(levelText))
                {
                    continue;
                }

                int level = ParseRequiredInt(row, columns, "LV", sheet.Name, rowNumber);
                if (level < 1 || !levelNumbers.Add(level))
                {
                    throw new InvalidDataException(
                        $"Level {level} must be unique and at least 1 in worksheet '{sheet.Name}', row {rowNumber}.");
                }

                levels.Add(new EnemyLevelData(
                    level,
                    ParseRequiredFloat(row, columns, "HP", sheet.Name, rowNumber),
                    ParseRequiredFloat(row, columns, "ATK", sheet.Name, rowNumber),
                    ParseRequiredFloat(row, columns, "ATK Speed", sheet.Name, rowNumber),
                    ParseRequiredFloat(row, columns, "ATK Range", sheet.Name, rowNumber),
                    ParseRequiredFloat(row, columns, "move speed", sheet.Name, rowNumber),
                    ParseRequiredInt(row, columns, "drop coin", sheet.Name, rowNumber)));
            }

            if (levels.Count == 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{sheet.Name}' does not contain any level rows.");
            }

            levels = levels.OrderBy(value => value.Level).ToList();
            for (int index = 0; index < levels.Count; index++)
            {
                if (levels[index].Level != index + 1)
                {
                    throw new InvalidDataException(
                        $"Worksheet '{sheet.Name}' levels must be consecutive starting at 1.");
                }
            }

            return levels;
        }

        private static Dictionary<string, int> BuildColumnMap(
            IReadOnlyDictionary<int, string> headerRow)
        {
            return headerRow
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .ToDictionary(
                    pair => pair.Value.Trim(),
                    pair => pair.Key,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static void ValidateRequiredColumns(
            string sheetName,
            IReadOnlyDictionary<string, int> columns,
            IEnumerable<string> requiredColumns)
        {
            string[] missingColumns = requiredColumns
                .Where(column => !columns.ContainsKey(column))
                .ToArray();
            if (missingColumns.Length > 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{sheetName}' is missing columns: " +
                    $"{string.Join(", ", missingColumns)}.");
            }
        }

        private static string GetValue(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column)
        {
            return row.TryGetValue(columns[column], out string value) ? value : string.Empty;
        }

        private static string GetRequiredValue(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column,
            string sheetName,
            int rowNumber)
        {
            string value = GetValue(row, columns, column).Trim();
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidDataException(
                    $"'{column}' is empty in worksheet '{sheetName}', row {rowNumber}.");
            }

            return value;
        }

        private static float ParseRequiredFloat(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column,
            string sheetName,
            int rowNumber)
        {
            string value = GetRequiredValue(row, columns, column, sheetName, rowNumber);
            if (!float.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float result))
            {
                throw new InvalidDataException(
                    $"'{column}' must be a number in worksheet '{sheetName}', " +
                    $"row {rowNumber}, but was '{value}'.");
            }

            return result;
        }

        private static int ParseRequiredInt(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column,
            string sheetName,
            int rowNumber)
        {
            string value = GetRequiredValue(row, columns, column, sheetName, rowNumber);
            if (!int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int result))
            {
                throw new InvalidDataException(
                    $"'{column}' must be an integer in worksheet '{sheetName}', " +
                    $"row {rowNumber}, but was '{value}'.");
            }

            return result;
        }

        private sealed class XlsxSheet
        {
            private readonly Dictionary<int, IReadOnlyDictionary<int, string>> rows;

            public XlsxSheet(
                string name,
                Dictionary<int, IReadOnlyDictionary<int, string>> rows)
            {
                Name = name;
                this.rows = rows;
            }

            public string Name { get; }
            public IEnumerable<int> RowNumbers => rows.Keys.OrderBy(row => row);

            public IReadOnlyDictionary<int, string> GetRow(int rowNumber)
            {
                return rows.TryGetValue(rowNumber, out IReadOnlyDictionary<int, string> row)
                    ? row
                    : new Dictionary<int, string>();
            }

            public string GetValue(int rowNumber, int columnIndex)
            {
                IReadOnlyDictionary<int, string> row = GetRow(rowNumber);
                return row.TryGetValue(columnIndex, out string value) ? value : string.Empty;
            }
        }

        private static class XlsxWorkbook
        {
            private static readonly XNamespace SpreadsheetNamespace =
                "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            private static readonly XNamespace DocumentRelationshipNamespace =
                "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            private static readonly XNamespace PackageRelationshipNamespace =
                "http://schemas.openxmlformats.org/package/2006/relationships";

            public static IReadOnlyList<XlsxSheet> ReadWorksheets(string path)
            {
                using var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

                List<string> sharedStrings = ReadSharedStrings(archive);
                XDocument workbook = LoadXml(archive, "xl/workbook.xml");
                XDocument relationships = LoadXml(archive, "xl/_rels/workbook.xml.rels");

                Dictionary<string, string> targets = relationships
                    .Descendants(PackageRelationshipNamespace + "Relationship")
                    .ToDictionary(
                        relationship => relationship.Attribute("Id")?.Value ?? string.Empty,
                        relationship => relationship.Attribute("Target")?.Value ?? string.Empty);

                var sheets = new List<XlsxSheet>();
                foreach (XElement sheetElement in workbook
                             .Descendants(SpreadsheetNamespace + "sheet"))
                {
                    string name = sheetElement.Attribute("name")?.Value ?? string.Empty;
                    string relationshipId =
                        sheetElement.Attribute(DocumentRelationshipNamespace + "id")?.Value
                        ?? string.Empty;

                    if (!targets.TryGetValue(relationshipId, out string target))
                    {
                        throw new InvalidDataException(
                            $"Worksheet '{name}' has no workbook relationship.");
                    }

                    string worksheetPath = NormalizeWorksheetPath(target);
                    sheets.Add(ReadWorksheet(archive, worksheetPath, name, sharedStrings));
                }

                return sheets;
            }

            private static XlsxSheet ReadWorksheet(
                ZipArchive archive,
                string worksheetPath,
                string name,
                IReadOnlyList<string> sharedStrings)
            {
                XDocument document = LoadXml(archive, worksheetPath);
                var parsedRows = new Dictionary<int, IReadOnlyDictionary<int, string>>();

                foreach (XElement rowElement in document
                             .Descendants(SpreadsheetNamespace + "row"))
                {
                    if (!int.TryParse(
                            rowElement.Attribute("r")?.Value,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out int rowNumber))
                    {
                        continue;
                    }

                    var cells = new Dictionary<int, string>();
                    foreach (XElement cell in rowElement.Elements(SpreadsheetNamespace + "c"))
                    {
                        string reference = cell.Attribute("r")?.Value ?? string.Empty;
                        int columnIndex = ParseColumnIndex(reference);
                        cells[columnIndex] = ReadCellValue(cell, sharedStrings);
                    }

                    parsedRows[rowNumber] = cells;
                }

                return new XlsxSheet(name, parsedRows);
            }

            private static List<string> ReadSharedStrings(ZipArchive archive)
            {
                ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
                if (entry == null)
                {
                    return new List<string>();
                }

                XDocument document;
                using (Stream sharedStringsStream = entry.Open())
                {
                    document = XDocument.Load(sharedStringsStream);
                }

                return document
                    .Descendants(SpreadsheetNamespace + "si")
                    .Select(item => string.Concat(
                        item.Descendants(SpreadsheetNamespace + "t")
                            .Select(text => text.Value)))
                    .ToList();
            }

            private static string ReadCellValue(
                XElement cell,
                IReadOnlyList<string> sharedStrings)
            {
                string type = cell.Attribute("t")?.Value;
                if (type == "inlineStr")
                {
                    return string.Concat(
                        cell.Descendants(SpreadsheetNamespace + "t")
                            .Select(text => text.Value));
                }

                string value = cell.Element(SpreadsheetNamespace + "v")?.Value ?? string.Empty;
                if (type == "s" &&
                    int.TryParse(
                        value,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int index) &&
                    index >= 0 &&
                    index < sharedStrings.Count)
                {
                    return sharedStrings[index];
                }

                return value;
            }

            private static XDocument LoadXml(ZipArchive archive, string path)
            {
                ZipArchiveEntry entry = archive.GetEntry(path)
                    ?? throw new InvalidDataException(
                        $"The workbook entry '{path}' was not found.");
                using Stream entryStream = entry.Open();
                return XDocument.Load(entryStream);
            }

            private static string NormalizeWorksheetPath(string target)
            {
                var workbookUri = new Uri("http://workbook/xl/workbook.xml");
                var worksheetUri = new Uri(workbookUri, target.Replace('\\', '/'));
                return Uri.UnescapeDataString(worksheetUri.AbsolutePath.TrimStart('/'));
            }

            private static int ParseColumnIndex(string cellReference)
            {
                int index = 0;
                foreach (char character in cellReference)
                {
                    if (!char.IsLetter(character))
                    {
                        break;
                    }

                    index = index * 26 + char.ToUpperInvariant(character) - 'A' + 1;
                }

                if (index == 0)
                {
                    throw new InvalidDataException(
                        $"Invalid worksheet cell reference '{cellReference}'.");
                }

                return index - 1;
            }
        }
    }
}
