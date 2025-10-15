// Services/CsvService.cs
using Break_Bulk_System.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Break_Bulk_System.Services
{
    public interface ICsvService
    {
        Task<List<ShippingLine>> ParseShippingLinesCsvAsync(Stream fileStream);
    }

    public class CsvService : ICsvService
    {
        public async Task<List<ShippingLine>> ParseShippingLinesCsvAsync(Stream fileStream)
        {
            var shippingLines = new List<ShippingLine>();

            try
            {
                // Use UTF-8 encoding and configure reader to handle bad data
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, true))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    Encoding = Encoding.UTF8,
                    BadDataFound = null, // Ignore bad data instead of throwing exceptions
                    MissingFieldFound = null, // Ignore missing fields
                    HeaderValidated = null, // Don't validate headers strictly
                    TrimOptions = TrimOptions.Trim // Trim whitespace from fields
                }))
                {
                    // Read the header row
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    // Validate headers
                    if (!csv.HeaderRecord?.Contains("Code") == true || !csv.HeaderRecord?.Contains("Description") == true)
                    {
                        throw new Exception("CSV file must contain 'Code' and 'Description' headers.");
                    }

                    while (await csv.ReadAsync())
                    {
                        try
                        {
                            var code = csv.GetField("Code")?.Trim();
                            var description = csv.GetField("Description")?.Trim();

                            // Skip empty rows or rows with missing essential data
                            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(description))
                            {
                                continue;
                            }

                            // Validate code length
                            if (code.Length > 6)
                            {
                                code = code.Substring(0, 6); // Truncate to max length
                            }

                            // Validate description length
                            if (description.Length > 100)
                            {
                                description = description.Substring(0, 100); // Truncate to max length
                            }

                            // Clean the data - remove any non-printable characters
                            code = CleanString(code);
                            description = CleanString(description);

                            shippingLines.Add(new ShippingLine
                            {
                                Code = code,
                                Name = description,
                                CreatedDate = DateTime.Now
                            });
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue processing other rows
                            Console.WriteLine($"Error processing row {csv.Context.Parser.Row}: {ex.Message}");
                            continue;
                        }
                    }
                }

                return shippingLines;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing CSV file: {ex.Message}", ex);
            }
        }

        private string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove non-printable characters except basic punctuation and letters/numbers
            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||
                char.IsPunctuation(c) ||
                char.IsSymbol(c) ||
                char.IsWhiteSpace(c) ||
                c == ' ' || c == '.' || c == ',' || c == '-' || c == '_' || c == '&' || c == '/'
            ).ToArray()).Trim();
        }
    }

    // CSV record class
    public class ShippingLineCsvRecord
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}