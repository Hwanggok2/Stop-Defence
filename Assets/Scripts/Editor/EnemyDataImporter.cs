using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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

        private static readonly string[] RequiredColumns =
        {
            "LV",
            "HP",
            "ATK",
            "ATK Speed",
            "ATK Range",
            "move speed",
            "drop coin"
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

            IReadOnlyList<XlsxSheet> sheets = XlsxWorkbook.ReadWorksheets(sourcePath);
            var enemies = new List<EnemyData>();
            var enemyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (XlsxSheet sheet in sheets)
            {
                Dictionary<string, int> columns = BuildColumnMap(sheet.GetRow(HeaderRow));
                ValidateRequiredColumns(sheet.Name, columns);

                string displayName = sheet.GetValue(HeaderRow, 0).Trim();
                if (string.IsNullOrEmpty(displayName))
                {
                    throw new InvalidDataException(
                        $"Worksheet '{sheet.Name}' must contain the enemy name in cell A1.");
                }

                string enemyId = NormalizeId(sheet.Name);
                if (!enemyIds.Add(enemyId))
                {
                    throw new InvalidDataException(
                        $"Worksheet '{sheet.Name}' produces duplicate enemy ID '{enemyId}'.");
                }

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
                    if (level < 1)
                    {
                        throw new InvalidDataException(
                            $"'LV' must be at least 1 in worksheet '{sheet.Name}', row {rowNumber}.");
                    }

                    if (!levelNumbers.Add(level))
                    {
                        throw new InvalidDataException(
                            $"Duplicate level {level} in worksheet '{sheet.Name}', row {rowNumber}.");
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

                enemies.Add(new EnemyData(
                    enemyId,
                    displayName,
                    levels.OrderBy(level => level.Level)));
            }

            if (enemies.Count == 0)
            {
                throw new InvalidDataException("The workbook does not contain any enemy worksheets.");
            }

            EnemyDatabase database = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(OutputAssetPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<EnemyDatabase>();
                AssetDatabase.CreateAsset(database, OutputAssetPath);
            }

            database.ReplaceEnemies(enemies);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = database;

            Debug.Log(
                $"[EnemyDataImporter] Imported {enemies.Count} enemy sheets from " +
                $"'{SourceRelativePath}' into '{OutputAssetPath}'.");
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
            IReadOnlyDictionary<string, int> columns)
        {
            string[] missingColumns = RequiredColumns
                .Where(column => !columns.ContainsKey(column))
                .ToArray();
            if (missingColumns.Length > 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{sheetName}' is missing columns: " +
                    $"{string.Join(", ", missingColumns)}.");
            }
        }

        private static string NormalizeId(string sheetName)
        {
            var builder = new StringBuilder();
            bool needsSeparator = false;

            foreach (char character in sheetName.Trim())
            {
                if (char.IsLetterOrDigit(character))
                {
                    if (needsSeparator && builder.Length > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(character));
                    needsSeparator = false;
                }
                else
                {
                    needsSeparator = true;
                }
            }

            if (builder.Length == 0)
            {
                throw new InvalidDataException(
                    $"Worksheet name '{sheetName}' cannot be converted to an enemy ID.");
            }

            return builder.ToString();
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
