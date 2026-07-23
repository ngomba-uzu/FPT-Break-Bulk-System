// Services/ExcelService.cs
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Break_Bulk_System.Services
{
    /// <summary>
    /// Reads .xlsx workbooks without an external dependency and converts the first
    /// worksheet to CSV text, so existing CsvHelper based parsers can be reused.
    /// </summary>
    public static class ExcelService
    {
        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        /// <summary>
        /// Converts the first worksheet of an .xlsx file into CSV text.
        /// Sheets with a single column are assumed to already hold CSV lines
        /// (a CSV that was re-saved as a workbook) and are passed through as-is.
        /// </summary>
        public static string ConvertFirstSheetToCsv(Stream fileStream)
        {
            var rows = ReadFirstSheet(fileStream);

            // A single column workbook is a CSV that was opened and saved in Excel:
            // every cell already contains a full comma separated line.
            var isSingleColumn = rows.All(r => r.Count <= 1);

            var builder = new StringBuilder();
            foreach (var row in rows)
            {
                if (row.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                builder.AppendLine(isSingleColumn
                    ? row[0].Trim()
                    : string.Join(",", row.Select(EscapeCsvField)));
            }

            return builder.ToString();
        }

        private static List<List<string>> ReadFirstSheet(Stream fileStream)
        {
            // ZipArchive needs a seekable stream; IFormFile streams are not always seekable.
            using var buffer = new MemoryStream();
            fileStream.CopyTo(buffer);
            buffer.Position = 0;

            using var archive = new ZipArchive(buffer, ZipArchiveMode.Read);

            var sharedStrings = ReadSharedStrings(archive);
            var sheetPath = GetFirstSheetPath(archive);

            var entry = FindEntry(archive, sheetPath)
                ?? throw new Exception("The Excel file does not contain a readable worksheet.");

            using var sheetStream = entry.Open();
            var sheet = XDocument.Load(sheetStream);

            var rows = new List<List<string>>();

            foreach (var rowElement in sheet.Descendants(Main + "row"))
            {
                var cells = new List<string>();

                foreach (var cell in rowElement.Elements(Main + "c"))
                {
                    // Honour the cell reference so blank cells keep their column position.
                    var columnIndex = GetColumnIndex((string?)cell.Attribute("r"));
                    if (columnIndex >= 0)
                    {
                        while (cells.Count < columnIndex)
                        {
                            cells.Add(string.Empty);
                        }
                    }

                    cells.Add(GetCellValue(cell, sharedStrings));
                }

                rows.Add(cells);
            }

            return rows;
        }

        private static string GetCellValue(XElement cell, List<string> sharedStrings)
        {
            var type = (string?)cell.Attribute("t");

            switch (type)
            {
                case "s":
                    var raw = cell.Element(Main + "v")?.Value;
                    return int.TryParse(raw, out var index) && index >= 0 && index < sharedStrings.Count
                        ? sharedStrings[index]
                        : string.Empty;

                case "inlineStr":
                    return string.Concat(cell.Element(Main + "is")?.Descendants(Main + "t").Select(t => t.Value)
                                         ?? Enumerable.Empty<string>());

                case "b":
                    return cell.Element(Main + "v")?.Value == "1" ? "TRUE" : "FALSE";

                default:
                    return cell.Element(Main + "v")?.Value ?? string.Empty;
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var strings = new List<string>();

            var entry = FindEntry(archive, "xl/sharedStrings.xml");
            if (entry == null)
            {
                return strings;
            }

            using var stream = entry.Open();
            var document = XDocument.Load(stream);

            foreach (var si in document.Root?.Elements(Main + "si") ?? Enumerable.Empty<XElement>())
            {
                // Skip phonetic runs (rPh) so only the displayed text is returned.
                var text = si.Descendants(Main + "t")
                    .Where(t => t.Parent?.Name != Main + "rPh" && t.Ancestors(Main + "rPh").Any() == false)
                    .Select(t => t.Value);

                strings.Add(string.Concat(text));
            }

            return strings;
        }

        private static string GetFirstSheetPath(ZipArchive archive)
        {
            const string fallback = "xl/worksheets/sheet1.xml";

            var workbookEntry = FindEntry(archive, "xl/workbook.xml");
            var relsEntry = FindEntry(archive, "xl/_rels/workbook.xml.rels");
            if (workbookEntry == null || relsEntry == null)
            {
                return fallback;
            }

            using var workbookStream = workbookEntry.Open();
            var relationshipId = (string?)XDocument.Load(workbookStream)
                .Descendants(Main + "sheet")
                .FirstOrDefault()?
                .Attribute(Rel + "id");

            if (string.IsNullOrEmpty(relationshipId))
            {
                return fallback;
            }

            using var relsStream = relsEntry.Open();
            var target = (string?)XDocument.Load(relsStream)
                .Descendants(PkgRel + "Relationship")
                .FirstOrDefault(r => (string?)r.Attribute("Id") == relationshipId)?
                .Attribute("Target");

            if (string.IsNullOrEmpty(target))
            {
                return fallback;
            }

            target = target.TrimStart('/');
            return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? target : "xl/" + target;
        }

        /// <summary>
        /// Looks up a part by name, tolerating writers that use backslashes
        /// or a different casing than the OPC specification.
        /// </summary>
        private static ZipArchiveEntry? FindEntry(ZipArchive archive, string path)
        {
            return archive.GetEntry(path)
                   ?? archive.Entries.FirstOrDefault(e =>
                       Normalise(e.FullName).Equals(Normalise(path), StringComparison.OrdinalIgnoreCase));

            static string Normalise(string value) => value.Replace('\\', '/').TrimStart('/');
        }

        /// <summary>Converts a cell reference such as "C12" to a zero based column index.</summary>
        private static int GetColumnIndex(string? cellReference)
        {
            if (string.IsNullOrEmpty(cellReference))
            {
                return -1;
            }

            var column = 0;
            foreach (var c in cellReference)
            {
                if (!char.IsLetter(c))
                {
                    break;
                }

                column = column * 26 + (char.ToUpperInvariant(c) - 'A' + 1);
            }

            return column - 1;
        }

        private static string EscapeCsvField(string field)
        {
            field ??= string.Empty;

            return field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r')
                ? $"\"{field.Replace("\"", "\"\"")}\""
                : field;
        }
    }
}
