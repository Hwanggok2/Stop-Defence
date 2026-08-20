using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace StopDefence.Editor
{
    internal sealed class XlsxSheet
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
    }

    internal static class XlsxWorkbookReader
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

            using Stream stream = entry.Open();
            XDocument document = XDocument.Load(stream);
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
