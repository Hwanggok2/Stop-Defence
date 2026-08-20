using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StopDefence.GameData;
using UnityEditor;
using UnityEngine;

namespace StopDefence.Editor
{
    public static class PlayerDataImporter
    {
        private const string SourceRelativePath = "GameData/GameData_Player.xlsx";
        private const string OutputAssetPath = "Assets/GameData/PlayerDatabase.asset";
        private const int HeaderRow = 1;
        private const int FirstDataRow = 2;

        private static readonly string[] RequiredColumns =
        {
            "LV",
            "NeedExp",
            "Hp"
        };

        [MenuItem("Tools/Game Data/Import Player Data")]
        public static void Import()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root could not be resolved.");
            string sourcePath = Path.Combine(projectRoot, SourceRelativePath);

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Player data workbook was not found.", sourcePath);
            }

            XlsxSheet sheet = XlsxWorkbookReader.ReadWorksheets(sourcePath).FirstOrDefault()
                ?? throw new InvalidDataException("Player data workbook has no worksheets.");
            Dictionary<string, int> columns = sheet.GetRow(HeaderRow)
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .ToDictionary(
                    pair => pair.Value.Trim(),
                    pair => pair.Key,
                    StringComparer.OrdinalIgnoreCase);

            string[] missing = RequiredColumns
                .Where(column => !columns.ContainsKey(column))
                .ToArray();
            if (missing.Length > 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{sheet.Name}' is missing columns: {string.Join(", ", missing)}.");
            }

            var levels = new List<PlayerLevelData>();
            foreach (int rowNumber in sheet.RowNumbers.Where(row => row >= FirstDataRow))
            {
                IReadOnlyDictionary<int, string> row = sheet.GetRow(rowNumber);
                string levelText = GetValue(row, columns, "LV").Trim();
                if (string.IsNullOrEmpty(levelText))
                {
                    continue;
                }

                int level = ParseInt(row, columns, "LV", sheet.Name, rowNumber);
                int requiredExperience = ParseInt(
                    row, columns, "NeedExp", sheet.Name, rowNumber);
                float maxHp = ParseFloat(row, columns, "Hp", sheet.Name, rowNumber);

                if (level != levels.Count || requiredExperience < 1 || maxHp < 1f)
                {
                    throw new InvalidDataException(
                        $"Player levels must start at 0 and be consecutive, with NeedExp and Hp above 0. " +
                        $"Invalid row: {rowNumber}.");
                }

                levels.Add(new PlayerLevelData(level, requiredExperience, maxHp));
            }

            if (levels.Count == 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{sheet.Name}' does not contain any player levels.");
            }

            PlayerDatabase database = AssetDatabase.LoadAssetAtPath<PlayerDatabase>(OutputAssetPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<PlayerDatabase>();
                AssetDatabase.CreateAsset(database, OutputAssetPath);
            }

            database.ReplaceLevels(levels);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Selection.activeObject = database;

            Debug.Log(
                $"[PlayerDataImporter] Imported {levels.Count} levels from " +
                $"'{SourceRelativePath}' into '{OutputAssetPath}'.");
        }

        private static string GetValue(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column)
        {
            return row.TryGetValue(columns[column], out string value) ? value : string.Empty;
        }

        private static int ParseInt(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column,
            string sheetName,
            int rowNumber)
        {
            string value = GetValue(row, columns, column).Trim();
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                throw new InvalidDataException(
                    $"'{column}' must be an integer in worksheet '{sheetName}', row {rowNumber}.");
            }

            return result;
        }

        private static float ParseFloat(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column,
            string sheetName,
            int rowNumber)
        {
            string value = GetValue(row, columns, column).Trim();
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                throw new InvalidDataException(
                    $"'{column}' must be a number in worksheet '{sheetName}', row {rowNumber}.");
            }

            return result;
        }
    }
}
