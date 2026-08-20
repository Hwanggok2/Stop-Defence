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
    public static class SkillDataImporter
    {
        private const string SourceRelativePath = "GameData/GameData_Skill.xlsx";
        private const string OutputAssetPath = "Assets/GameData/SkillDatabase.asset";
        private const string SkillInfoSheetName = "SkillInfo";
        private const string JudgementBalanceSheetName = "JudgementBalance";
        private const int HeaderRow = 1;
        private const int FirstDataRow = 2;

        private static readonly string[] Columns =
        {
            "SkillId",
            "DisplayName",
            "Description",
            "Category",
            "Grade",
            "ImagePath",
            "Enabled",
            "StatType",
            "StatValue",
            "StatCap"
        };

        private static readonly string[] RequiredValues =
        {
            "SkillId",
            "DisplayName",
            "Description",
            "Category",
            "Grade",
            "Enabled",
            "StatType",
            "StatValue",
            "StatCap"
        };

        [MenuItem("Tools/Game Data/Import Skill Data")]
        public static void Import()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root could not be resolved.");
            string sourcePath = Path.Combine(projectRoot, SourceRelativePath);

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Skill data workbook was not found.", sourcePath);
            }

            IReadOnlyList<XlsxSheet> workbook = XlsxWorkbookReader.ReadWorksheets(sourcePath);
            XlsxSheet sheet = workbook
                .FirstOrDefault(value => string.Equals(
                    value.Name,
                    SkillInfoSheetName,
                    StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException(
                    $"Worksheet '{SkillInfoSheetName}' was not found.");

            Dictionary<string, int> columns = BuildColumnMap(sheet.GetRow(HeaderRow));
            ValidateColumns(columns);

            List<int> dataRows = sheet.RowNumbers
                .Where(rowNumber => rowNumber >= FirstDataRow)
                .Where(rowNumber => HasData(sheet.GetRow(rowNumber)))
                .ToList();
            ValidateUniqueIds(sheet, dataRows, columns);

            var skills = new List<SkillData>(dataRows.Count);
            foreach (int rowNumber in dataRows)
            {
                IReadOnlyDictionary<int, string> row = sheet.GetRow(rowNumber);
                string[] missingValues = RequiredValues
                    .Where(column => string.IsNullOrWhiteSpace(GetValue(row, columns, column)))
                    .ToArray();
                if (missingValues.Length > 0)
                {
                    WarnAndExclude(
                        rowNumber,
                        $"missing required values: {string.Join(", ", missingValues)}");
                    continue;
                }

                string skillId = GetValue(row, columns, "SkillId").Trim();
                string displayName = GetValue(row, columns, "DisplayName").Trim();
                string description = GetValue(row, columns, "Description").Trim();
                string categoryText = GetValue(row, columns, "Category").Trim();
                string gradeText = GetValue(row, columns, "Grade").Trim();
                string imagePath = GetValue(row, columns, "ImagePath").Trim();
                string enabledText = GetValue(row, columns, "Enabled").Trim();
                string statTypeText = GetValue(row, columns, "StatType").Trim();
                string statValueText = GetValue(row, columns, "StatValue").Trim();
                string statCapText = GetValue(row, columns, "StatCap").Trim();

                if (!Enum.TryParse(categoryText, true, out SkillCategory category) ||
                    !Enum.IsDefined(typeof(SkillCategory), category))
                {
                    WarnAndExclude(rowNumber, $"unknown Category '{categoryText}'");
                    continue;
                }

                if (!int.TryParse(
                        gradeText,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int grade))
                {
                    WarnAndExclude(rowNumber, $"Grade must be an integer, but was '{gradeText}'");
                    continue;
                }

                if (!TryParseBool(enabledText, out bool enabled))
                {
                    WarnAndExclude(rowNumber, $"Enabled must be a boolean value, but was '{enabledText}'");
                    continue;
                }

                if (!Enum.TryParse(statTypeText, true, out PlayerStatType statType) ||
                    !Enum.IsDefined(typeof(PlayerStatType), statType))
                {
                    WarnAndExclude(rowNumber, $"unknown StatType '{statTypeText}'");
                    continue;
                }

                if (!TryParseNonNegativeFloat(statValueText, out float statValue))
                {
                    WarnAndExclude(
                        rowNumber,
                        $"StatValue must be a non-negative number, but was '{statValueText}'");
                    continue;
                }

                if (!TryParseNonNegativeFloat(statCapText, out float statCap))
                {
                    WarnAndExclude(
                        rowNumber,
                        $"StatCap must be a non-negative number, but was '{statCapText}'");
                    continue;
                }

                if (!ValidateCategoryValues(
                        rowNumber,
                        category,
                        grade,
                        statType,
                        statValue,
                        statCap))
                {
                    continue;
                }

                Sprite image = null;
                if (!string.IsNullOrEmpty(imagePath))
                {
                    image = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                    if (image == null)
                    {
                        Debug.LogWarning(
                            $"[SkillDataImporter] Sprite '{imagePath}' for skill '{skillId}' was not found. " +
                            "The default card image will be used.");
                    }
                }

                skills.Add(new SkillData(
                    skillId,
                    displayName,
                    description,
                    category,
                    grade,
                    imagePath,
                    image,
                    enabled,
                    statType,
                    statValue,
                    statCap));
            }

            if (skills.Count == 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{SkillInfoSheetName}' does not contain any valid skills.");
            }

            List<JudgementBalanceData> judgementBalances =
                ReadJudgementBalances(workbook);

            SkillDatabase database = AssetDatabase.LoadAssetAtPath<SkillDatabase>(OutputAssetPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<SkillDatabase>();
                AssetDatabase.CreateAsset(database, OutputAssetPath);
            }

            database.ReplaceData(skills, judgementBalances);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = database;

            Debug.Log(
                $"[SkillDataImporter] Imported {skills.Count} skills and " +
                $"{judgementBalances.Count} judgement balances from " +
                $"'{SourceRelativePath}' into '{OutputAssetPath}'.");
        }

        private static List<JudgementBalanceData> ReadJudgementBalances(
            IReadOnlyList<XlsxSheet> workbook)
        {
            XlsxSheet sheet = workbook.FirstOrDefault(value => string.Equals(
                value.Name,
                JudgementBalanceSheetName,
                StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidDataException(
                    $"Worksheet '{JudgementBalanceSheetName}' was not found.");

            Dictionary<string, int> columns = BuildColumnMap(
                sheet.GetRow(HeaderRow),
                JudgementBalanceSheetName);
            string[] requiredColumns = { "Judgement", "DamageMultiplier" };
            string[] missingColumns = requiredColumns
                .Where(column => !columns.ContainsKey(column))
                .ToArray();
            if (missingColumns.Length > 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{JudgementBalanceSheetName}' is missing columns: " +
                    $"{string.Join(", ", missingColumns)}.");
            }

            var balances = new List<JudgementBalanceData>(4);
            var judgements = new HashSet<TimingJudgement>();
            foreach (int rowNumber in sheet.RowNumbers
                         .Where(value => value >= FirstDataRow)
                         .Where(value => HasData(sheet.GetRow(value))))
            {
                IReadOnlyDictionary<int, string> row = sheet.GetRow(rowNumber);
                string judgementText = GetValue(row, columns, "Judgement").Trim();
                string multiplierText = GetValue(row, columns, "DamageMultiplier").Trim();

                if (!Enum.TryParse(judgementText, true, out TimingJudgement judgement) ||
                    !Enum.IsDefined(typeof(TimingJudgement), judgement))
                {
                    throw new InvalidDataException(
                        $"Worksheet '{JudgementBalanceSheetName}', row {rowNumber} has " +
                        $"unknown Judgement '{judgementText}'.");
                }

                if (!judgements.Add(judgement))
                {
                    throw new InvalidDataException(
                        $"Worksheet '{JudgementBalanceSheetName}' has duplicate " +
                        $"Judgement '{judgement}'.");
                }

                if (!TryParsePositiveFloat(multiplierText, out float multiplier))
                {
                    throw new InvalidDataException(
                        $"Worksheet '{JudgementBalanceSheetName}', row {rowNumber} requires " +
                        $"a DamageMultiplier above 0, but was '{multiplierText}'.");
                }

                balances.Add(new JudgementBalanceData(judgement, multiplier));
            }

            TimingJudgement[] missingJudgements = Enum.GetValues(typeof(TimingJudgement))
                .Cast<TimingJudgement>()
                .Where(value => !judgements.Contains(value))
                .ToArray();
            if (missingJudgements.Length > 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{JudgementBalanceSheetName}' is missing judgements: " +
                    $"{string.Join(", ", missingJudgements)}.");
            }

            return balances;
        }

        private static Dictionary<string, int> BuildColumnMap(
            IReadOnlyDictionary<int, string> headerRow)
        {
            return BuildColumnMap(headerRow, SkillInfoSheetName);
        }

        private static Dictionary<string, int> BuildColumnMap(
            IReadOnlyDictionary<int, string> headerRow,
            string sheetName)
        {
            var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<int, string> cell in headerRow)
            {
                string columnName = cell.Value.Trim();
                if (string.IsNullOrEmpty(columnName))
                {
                    continue;
                }

                if (!columns.TryAdd(columnName, cell.Key))
                {
                    throw new InvalidDataException(
                        $"Worksheet '{sheetName}' has a duplicate column '{columnName}'.");
                }
            }

            return columns;
        }

        private static void ValidateColumns(IReadOnlyDictionary<string, int> columns)
        {
            string[] missingColumns = Columns
                .Where(column => !columns.ContainsKey(column))
                .ToArray();
            if (missingColumns.Length > 0)
            {
                throw new InvalidDataException(
                    $"Worksheet '{SkillInfoSheetName}' is missing columns: " +
                    $"{string.Join(", ", missingColumns)}.");
            }
        }

        private static void ValidateUniqueIds(
            XlsxSheet sheet,
            IEnumerable<int> dataRows,
            IReadOnlyDictionary<string, int> columns)
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (int rowNumber in dataRows)
            {
                string skillId = GetValue(
                    sheet.GetRow(rowNumber), columns, "SkillId").Trim();
                if (!string.IsNullOrEmpty(skillId) && !ids.Add(skillId))
                {
                    throw new InvalidDataException(
                        $"Duplicate skill ID '{skillId}' in worksheet " +
                        $"'{SkillInfoSheetName}', row {rowNumber}.");
                }
            }
        }

        private static bool HasData(IReadOnlyDictionary<int, string> row)
        {
            return row.Values.Any(value => !string.IsNullOrWhiteSpace(value));
        }

        private static string GetValue(
            IReadOnlyDictionary<int, string> row,
            IReadOnlyDictionary<string, int> columns,
            string column)
        {
            return row.TryGetValue(columns[column], out string value) ? value : string.Empty;
        }

        private static bool TryParseBool(string value, out bool result)
        {
            if (bool.TryParse(value, out result))
            {
                return true;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "1":
                case "yes":
                case "y":
                case "on":
                    result = true;
                    return true;
                case "0":
                case "no":
                case "n":
                case "off":
                    result = false;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }

        private static bool TryParseNonNegativeFloat(string value, out float result)
        {
            return float.TryParse(
                       value,
                       NumberStyles.Float,
                       CultureInfo.InvariantCulture,
                       out result) &&
                   float.IsFinite(result) &&
                   result >= 0f;
        }

        private static bool TryParsePositiveFloat(string value, out float result)
        {
            return TryParseNonNegativeFloat(value, out result) && result > 0f;
        }

        private static bool ValidateCategoryValues(
            int rowNumber,
            SkillCategory category,
            int grade,
            PlayerStatType statType,
            float statValue,
            float statCap)
        {
            if (category == SkillCategory.Active)
            {
                if (grade < 1 || grade > 10)
                {
                    WarnAndExclude(rowNumber, $"Active skill Grade must be from 1 to 10, but was '{grade}'");
                    return false;
                }

                if (statType != PlayerStatType.None || statValue != 0f || statCap != 0f)
                {
                    WarnAndExclude(
                        rowNumber,
                        "Active skills require StatType None, StatValue 0 and StatCap 0");
                    return false;
                }

                return true;
            }

            if (grade != 0)
            {
                WarnAndExclude(rowNumber, $"StatUpgrade Grade must be 0, but was '{grade}'");
                return false;
            }

            if (statType == PlayerStatType.None || statValue <= 0f)
            {
                WarnAndExclude(
                    rowNumber,
                    "StatUpgrade requires a non-None StatType and StatValue above 0");
                return false;
            }

            if (statCap > 0f && statCap < statValue)
            {
                WarnAndExclude(
                    rowNumber,
                    "StatCap must be 0 (unlimited) or at least StatValue");
                return false;
            }

            return true;
        }

        private static void WarnAndExclude(int rowNumber, string reason)
        {
            Debug.LogWarning(
                $"[SkillDataImporter] Excluded worksheet '{SkillInfoSheetName}' " +
                $"row {rowNumber}: {reason}.");
        }
    }
}
