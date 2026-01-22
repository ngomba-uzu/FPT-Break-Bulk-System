// Services/TransportSeaCsvProcessor.cs
using Break_Bulk_System.Data;
using Break_Bulk_System.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Break_Bulk_System.Services
{
    public class TransportSeaCsvProcessor : ITransportSeaCsvProcessor
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransportSeaCsvProcessor> _logger;
        private readonly HttpClient _httpClient;

        public TransportSeaCsvProcessor(ApplicationDbContext context,
            ILogger<TransportSeaCsvProcessor> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<bool> ProcessCsvFromUrlAsync()
        {
            var csvUrl = "https://tools.sars.gov.za/ACM_code_Tables/TransportSea.csv";

            try
            {
                _logger.LogInformation("Starting CSV download from: {Url}", csvUrl);

                var transportSeas = await DownloadAndParseCsvAsync(csvUrl);

                if (!transportSeas.Any())
                {
                    _logger.LogWarning("No data found in CSV file");
                    return false;
                }

                // Validate data
                var validationErrors = ValidateTransportSeas(transportSeas);
                if (validationErrors.Any())
                {
                    _logger.LogError("Validation errors: {Errors}", string.Join("; ", validationErrors));
                    return false;
                }

                // Use transaction for data consistency
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Remove all existing transport seas
                    await _context.TransportSeas.ExecuteDeleteAsync();

                    // Add new records
                    await _context.TransportSeas.AddRangeAsync(transportSeas);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully processed {Count} TransportSea records", transportSeas.Count);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Database error while saving TransportSea records");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV from URL: {Url}", csvUrl);
                return false;
            }
        }

        public async Task<List<TransportSea>> DownloadAndParseCsvAsync(string url)
        {
            var transportSeas = new List<TransportSea>();

            try
            {
                // Download CSV file
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    Encoding = System.Text.Encoding.UTF8,
                    BadDataFound = null,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                });

                // Read the header row
                await csv.ReadAsync();
                csv.ReadHeader();

                // Validate headers
                var requiredHeaders = new[] { "TransportID", "Name", "CarrierCode", "CarrierName" };
                var missingHeaders = requiredHeaders.Where(h => !csv.HeaderRecord?.Contains(h) == true).ToList();

                if (missingHeaders.Any())
                {
                    throw new Exception($"CSV file is missing required headers: {string.Join(", ", missingHeaders)}");
                }

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var transportID = csv.GetField("TransportID")?.Trim();
                        var name = csv.GetField("Name")?.Trim();
                        var carrierCode = csv.GetField("CarrierCode")?.Trim();
                        var carrierName = csv.GetField("CarrierName")?.Trim();

                        // Skip empty rows or rows with missing essential data
                        if (string.IsNullOrWhiteSpace(transportID))
                        {
                            continue;
                        }

                        // Validate field lengths
                        if (transportID.Length > 10)
                        {
                            transportID = transportID.Substring(0, 10);
                        }
                        if (name?.Length > 100)
                        {
                            name = name.Substring(0, 100);
                        }
                        if (carrierCode?.Length > 10)
                        {
                            carrierCode = carrierCode.Substring(0, 10);
                        }
                        if (carrierName?.Length > 100)
                        {
                            carrierName = carrierName.Substring(0, 100);
                        }

                        // Clean the data
                        transportID = CleanString(transportID);
                        name = CleanString(name);
                        carrierCode = CleanString(carrierCode);
                        carrierName = CleanString(carrierName);

                        transportSeas.Add(new TransportSea
                        {
                            TransportID = transportID,
                            Name = name,
                            CarrierCode = carrierCode,
                            CarrierName = carrierName
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error processing row {Row}: {Error}", csv.Context.Parser.Row, ex.Message);
                        continue;
                    }
                }

                return transportSeas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading or parsing CSV file: {ex.Message}", ex);
            }
        }

        private List<string> ValidateTransportSeas(List<TransportSea> transportSeas)
        {
            var errors = new List<string>();

            // Check for duplicate TransportIDs
            var duplicateIds = transportSeas
                .GroupBy(x => x.TransportID)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Any())
            {
                errors.Add($"Duplicate Transport IDs found: {string.Join(", ", duplicateIds)}");
            }

            // Check for empty TransportIDs
            var emptyIds = transportSeas.Where(x => string.IsNullOrWhiteSpace(x.TransportID)).ToList();
            if (emptyIds.Any())
            {
                errors.Add($"Found {emptyIds.Count} records with empty Transport IDs");
            }

            return errors;
        }

        private string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return new string(input.Where(c =>
                char.IsLetterOrDigit(c) ||
                char.IsPunctuation(c) ||
                char.IsSymbol(c) ||
                char.IsWhiteSpace(c) ||
                c == ' ' || c == '.' || c == ',' || c == '-' || c == '_' || c == '&' || c == '/'
            ).ToArray()).Trim();
        }
    }
}