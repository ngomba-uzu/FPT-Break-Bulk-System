// Controllers/TransportSeaController.cs
using Break_Bulk_System.Data;
using Break_Bulk_System.Models;
using Break_Bulk_System.Services;
using Break_Bulk_System.ViewModels;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Break_Bulk_System.Controllers
{
    [Authorize]
    public class TransportSeaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICsvService _csvService;
        private readonly ILogger<TransportSeaController> _logger;

        public TransportSeaController(ApplicationDbContext context, ICsvService csvService, ILogger<TransportSeaController> logger)
        {
            _context = context;
            _csvService = csvService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var transportSeas = await _context.TransportSeas.OrderBy(t => t.TransportID).ToListAsync();
            return View(transportSeas);
        }

        public IActionResult Upload()
        {
            var viewModel = new TransportSeaUploadViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(TransportSeaUploadViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.CsvFile == null || viewModel.CsvFile.Length == 0)
            {
                ModelState.AddModelError("CsvFile", "Please select a CSV file.");
                return View(viewModel);
            }

            if (!Path.GetExtension(viewModel.CsvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("CsvFile", "Please upload a CSV file.");
                return View(viewModel);
            }

            try
            {
                List<TransportSea> transportSeas;

                using (var stream = viewModel.CsvFile.OpenReadStream())
                {
                    transportSeas = await ParseTransportSeasCsvAsync(stream);
                }

                if (!transportSeas.Any())
                {
                    ModelState.AddModelError("CsvFile", "No valid call signs found in the CSV file.");
                    return View(viewModel);
                }

                // Validate data before saving
                var validationErrors = ValidateTransportSeas(transportSeas);
                if (validationErrors.Any())
                {
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("CsvFile", error);
                    }
                    return View(viewModel);
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

                    _logger.LogInformation("Successfully uploaded {Count} call signs", transportSeas.Count);
                    TempData["SuccessMessage"] = $"Successfully uploaded {transportSeas.Count} call signs.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "Database error while saving call signs");

                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    ModelState.AddModelError("CsvFile", $"Database error: {GetUserFriendlyErrorMessage(innerMessage)}");
                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while saving call signs");
                    ModelState.AddModelError("CsvFile", $"Error saving data: {ex.Message}");
                    return View(viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV file");
                ModelState.AddModelError("CsvFile", $"Error processing CSV file: {GetUserFriendlyErrorMessage(ex.Message)}");
                return View(viewModel);
            }
        }

        private async Task<List<TransportSea>> ParseTransportSeasCsvAsync(Stream fileStream)
        {
            var transportSeas = new List<TransportSea>();

            try
            {
                using (var reader = new StreamReader(fileStream, System.Text.Encoding.UTF8, true))
                using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    Encoding = System.Text.Encoding.UTF8,
                    BadDataFound = null,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim
                }))
                {
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
                            _logger.LogWarning($"Error processing row {csv.Context.Parser.Row}: {ex.Message}");
                            continue;
                        }
                    }
                }

                return transportSeas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing CSV file: {ex.Message}", ex);
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

        private string GetUserFriendlyErrorMessage(string errorMessage)
        {
            if (errorMessage.Contains("PK_TransportSea") || errorMessage.Contains("primary key"))
            {
                return "Duplicate Transport IDs found. Please ensure all Transport IDs are unique.";
            }
            else if (errorMessage.Contains("String or binary data would be truncated"))
            {
                return "Some data is too long for database fields. Please check field lengths.";
            }
            else if (errorMessage.Contains("timeout"))
            {
                return "Database operation timed out. Please try again.";
            }

            return errorMessage;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var count = await _context.TransportSeas.CountAsync();

                if (count == 0)
                {
                    TempData["InfoMessage"] = "No call signs to delete.";
                    return RedirectToAction(nameof(Index));
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Clear call sign references from vessels
                    var vesselsWithCallSigns = await _context.VesselMasters
                        .Where(v => v.CallSign != null)
                        .ToListAsync();

                    foreach (var vessel in vesselsWithCallSigns)
                    {
                        vessel.CallSign = null;
                        vessel.ModifiedDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    // Delete all transport seas
                    await _context.TransportSeas.ExecuteDeleteAsync();
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Successfully deleted {count} call signs and cleared references from {vesselsWithCallSigns.Count} vessel(s).";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during call sign deletion");
                    TempData["ErrorMessage"] = $"Error deleting call signs: {ex.Message}";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAll method");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // CSV record class
    public class TransportSeaCsvRecord
    {
        public string TransportID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CarrierCode { get; set; } = string.Empty;
        public string CarrierName { get; set; } = string.Empty;
    }
}