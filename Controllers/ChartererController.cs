// Controllers/ChartererController.cs
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
    public class ChartererController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICsvService _csvService;
        private readonly ILogger<ChartererController> _logger;

        public ChartererController(ApplicationDbContext context, ICsvService csvService, ILogger<ChartererController> logger)
        {
            _context = context;
            _csvService = csvService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var charterers = await _context.Charterers
                .OrderBy(c => c.Description)
                .ToListAsync();
            return View(charterers);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ChartererUploadViewModel viewModel)
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
                List<Charterer> charterers;

                using (var stream = viewModel.CsvFile.OpenReadStream())
                {
                    charterers = await ParseCharterersCsvAsync(stream);
                }

                if (!charterers.Any())
                {
                    ModelState.AddModelError("CsvFile", "No valid charterers found in the CSV file.");
                    return View(viewModel);
                }

                // Validate data before saving
                var validationErrors = ValidateCharterers(charterers);
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
                    // Remove all existing charterers
                    var existingCharterers = await _context.Charterers.ToListAsync();
                    _context.Charterers.RemoveRange(existingCharterers);

                    // Add new charterers with validation
                    foreach (var charterer in charterers)
                    {
                        // Ensure data meets constraints
                        if (charterer.KeyCode.Length > 6)
                        {
                            charterer.KeyCode = charterer.KeyCode.Substring(0, 6);
                        }
                        if (charterer.Description.Length > 50)
                        {
                            charterer.Description = charterer.Description.Substring(0, 50);
                        }
                        if (charterer.LongDescription?.Length > 100)
                        {
                            charterer.LongDescription = charterer.LongDescription.Substring(0, 100);
                        }

                        _context.Charterers.Add(charterer);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully uploaded {Count} charterers", charterers.Count);
                    TempData["SuccessMessage"] = $"Successfully uploaded {charterers.Count} charterers.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "Database error while saving charterers");

                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    ModelState.AddModelError("CsvFile", $"Database error: {GetUserFriendlyErrorMessage(innerMessage)}");
                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while saving charterers");
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

        private async Task<List<Charterer>> ParseCharterersCsvAsync(Stream fileStream)
        {
            var charterers = new List<Charterer>();

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
                    var requiredHeaders = new[] { "Key Code", "Description", "Long Description" };
                    var missingHeaders = requiredHeaders.Where(h => !csv.HeaderRecord?.Contains(h) == true).ToList();

                    if (missingHeaders.Any())
                    {
                        throw new Exception($"CSV file is missing required headers: {string.Join(", ", missingHeaders)}");
                    }

                    while (await csv.ReadAsync())
                    {
                        try
                        {
                            var keyCode = csv.GetField("Key Code")?.Trim();
                            var description = csv.GetField("Description")?.Trim();
                            var longDescription = csv.GetField("Long Description")?.Trim();

                            // Skip empty rows or rows with missing essential data
                            if (string.IsNullOrWhiteSpace(keyCode) || string.IsNullOrWhiteSpace(description))
                            {
                                continue;
                            }

                            // Clean the data
                            keyCode = CleanString(keyCode);
                            description = CleanString(description);
                            longDescription = CleanString(longDescription);

                            charterers.Add(new Charterer
                            {
                                KeyCode = keyCode,
                                Description = description,
                                LongDescription = longDescription,
                                CreatedDate = DateTime.Now
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error processing row {csv.Context.Parser.Row}: {ex.Message}");
                            continue;
                        }
                    }
                }

                return charterers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing CSV file: {ex.Message}", ex);
            }
        }

        private List<string> ValidateCharterers(List<Charterer> charterers)
        {
            var errors = new List<string>();

            // Check for duplicate key codes
            var duplicateCodes = charterers
                .GroupBy(x => x.KeyCode)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateCodes.Any())
            {
                errors.Add($"Duplicate key codes found: {string.Join(", ", duplicateCodes)}");
            }

            // Check for empty key codes or descriptions
            var emptyCodes = charterers.Where(x => string.IsNullOrWhiteSpace(x.KeyCode)).ToList();
            var emptyDescriptions = charterers.Where(x => string.IsNullOrWhiteSpace(x.Description)).ToList();

            if (emptyCodes.Any())
            {
                errors.Add($"Found {emptyCodes.Count} records with empty key codes");
            }

            if (emptyDescriptions.Any())
            {
                errors.Add($"Found {emptyDescriptions.Count} records with empty descriptions");
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
            if (errorMessage.Contains("PK_Charterer") || errorMessage.Contains("primary key"))
            {
                return "Duplicate charterer key codes found. Please ensure all key codes are unique.";
            }
            else if (errorMessage.Contains("String or binary data would be truncated"))
            {
                return "Some data is too long for database fields. Key codes should be 6 characters or less, descriptions should be 50 characters or less, and long descriptions should be 100 characters or less.";
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
                var charterers = await _context.Charterers.ToListAsync();

                if (!charterers.Any())
                {
                    TempData["InfoMessage"] = "No charterers to delete.";
                    return RedirectToAction(nameof(Index));
                }

                // Use transaction
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // First, clear all charterer references from vessels
                    var vesselsWithCharterers = await _context.VesselMasters
                        .Where(v => v.Charterer != null && charterers.Select(c => c.KeyCode).Contains(v.Charterer))
                        .ToListAsync();

                    foreach (var vessel in vesselsWithCharterers)
                    {
                        vessel.Charterer = null;
                        vessel.ModifiedDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    // Now delete the charterers
                    _context.Charterers.RemoveRange(charterers);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var clearedVessels = vesselsWithCharterers.Count;
                    TempData["SuccessMessage"] =
                        $"Successfully deleted {charterers.Count} charterers and cleared references from {clearedVessels} vessel(s).";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during charterer deletion");
                    TempData["ErrorMessage"] = $"Error deleting charterers: {ex.Message}";
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
}