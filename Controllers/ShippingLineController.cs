// Controllers/ShippingLineController.cs
using Break_Bulk_System.Data;
using Break_Bulk_System.Models;
using Break_Bulk_System.Services;
using Break_Bulk_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Break_Bulk_System.Controllers
{
    [Authorize]
    public class ShippingLineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICsvService _csvService;
        private readonly ILogger<ShippingLineController> _logger;

        public ShippingLineController(ApplicationDbContext context, ICsvService csvService, ILogger<ShippingLineController> logger)
        {
            _context = context;
            _csvService = csvService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var shippingLines = await _context.ShippingLines
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(shippingLines);
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ShippingLineUploadViewModel viewModel)
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
                List<ShippingLine> shippingLines;

                using (var stream = viewModel.CsvFile.OpenReadStream())
                {
                    shippingLines = await _csvService.ParseShippingLinesCsvAsync(stream);
                }

                if (!shippingLines.Any())
                {
                    ModelState.AddModelError("CsvFile", "No valid shipping lines found in the CSV file.");
                    return View(viewModel);
                }

                // Validate data before saving
                var validationErrors = ValidateShippingLines(shippingLines);
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
                    // Remove all existing shipping lines
                    var existingLines = await _context.ShippingLines.ToListAsync();
                    _context.ShippingLines.RemoveRange(existingLines);

                    // Add new shipping lines with validation
                    foreach (var line in shippingLines)
                    {
                        // Ensure data meets constraints
                        if (line.Code.Length > 6)
                        {
                            line.Code = line.Code.Substring(0, 6);
                        }
                        if (line.Name.Length > 100)
                        {
                            line.Name = line.Name.Substring(0, 100);
                        }

                        _context.ShippingLines.Add(line);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully uploaded {Count} shipping lines", shippingLines.Count);
                    TempData["SuccessMessage"] = $"Successfully uploaded {shippingLines.Count} shipping lines.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(dbEx, "Database error while saving shipping lines");

                    // Extract inner exception message for better error reporting
                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    ModelState.AddModelError("CsvFile", $"Database error: {GetUserFriendlyErrorMessage(innerMessage)}");
                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while saving shipping lines");
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

        private List<string> ValidateShippingLines(List<ShippingLine> shippingLines)
        {
            var errors = new List<string>();

            // Check for duplicate codes
            var duplicateCodes = shippingLines
                .GroupBy(x => x.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateCodes.Any())
            {
                errors.Add($"Duplicate codes found: {string.Join(", ", duplicateCodes)}");
            }

            // Check for empty codes or names
            var emptyCodes = shippingLines.Where(x => string.IsNullOrWhiteSpace(x.Code)).ToList();
            var emptyNames = shippingLines.Where(x => string.IsNullOrWhiteSpace(x.Name)).ToList();

            if (emptyCodes.Any())
            {
                errors.Add($"Found {emptyCodes.Count} records with empty codes");
            }

            if (emptyNames.Any())
            {
                errors.Add($"Found {emptyNames.Count} records with empty names");
            }

            return errors;
        }

        private string GetUserFriendlyErrorMessage(string errorMessage)
        {
            if (errorMessage.Contains("PK_ShippingLine") || errorMessage.Contains("primary key"))
            {
                return "Duplicate shipping line codes found. Please ensure all codes are unique.";
            }
            else if (errorMessage.Contains("String or binary data would be truncated"))
            {
                return "Some data is too long for database fields. Codes should be 6 characters or less, names should be 100 characters or less.";
            }
            else if (errorMessage.Contains("timeout"))
            {
                return "Database operation timed out. Please try again.";
            }

            return errorMessage;
        }

        // In ShippingLineController.cs - use this DeleteAll method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var shippingLines = await _context.ShippingLines.ToListAsync();

                if (!shippingLines.Any())
                {
                    TempData["InfoMessage"] = "No shipping lines to delete.";
                    return RedirectToAction(nameof(Index));
                }

                // Use transaction
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // First, clear all shipping line references from vessels
                    var vesselsWithShippingLines = await _context.VesselMasters
                        .Where(v => v.ShippingLineCode != null && shippingLines.Select(sl => sl.Code).Contains(v.ShippingLineCode))
                        .ToListAsync();

                    foreach (var vessel in vesselsWithShippingLines)
                    {
                        vessel.ShippingLineCode = null;
                        vessel.ModifiedDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    // Now delete the shipping lines
                    _context.ShippingLines.RemoveRange(shippingLines);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var clearedVessels = vesselsWithShippingLines.Count;
                    TempData["SuccessMessage"] =
                        $"Successfully deleted {shippingLines.Count} shipping lines and cleared references from {clearedVessels} vessel(s).";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during shipping line deletion");
                    TempData["ErrorMessage"] = $"Error deleting shipping lines: {ex.Message}";
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