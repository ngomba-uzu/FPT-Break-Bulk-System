// Controllers/ManifestController.cs
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
    public class ManifestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBarcodeService _barcodeService;

        public ManifestController(ApplicationDbContext context, IBarcodeService barcodeService)
        {
            _context = context;
            _barcodeService = barcodeService;
        }

        public async Task<IActionResult> Index()
        {
            var manifests = await _context.Manifests
                .Include(m => m.VesselMaster) // Include the VesselMaster
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();
            return View(manifests);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new ManifestViewModel
            {
                Vessels = await _context.VesselMasters.ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManifestViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Verify the vessel exists
                var vessel = await _context.VesselMasters
                    .FirstOrDefaultAsync(v => v.VesselCode == viewModel.Manifest.VesselCode);

                if (vessel == null)
                {
                    ModelState.AddModelError("Manifest.VesselCode", "Vessel not found");
                    viewModel.Vessels = await _context.VesselMasters.ToListAsync();
                    return View(viewModel);
                }

                _context.Add(viewModel.Manifest);
                await _context.SaveChangesAsync();

                // Now that the manifest has an Id, assign its manifest number and the
                // next sequential product barcode (PRBC), then persist them. Retry a
                // few times in case another manifest grabbed the same PRBC concurrently.
                viewModel.Manifest.ManifestNumber = GenerateManifestNumber(viewModel.Manifest.Id);

                for (int attempt = 0; ; attempt++)
                {
                    viewModel.Manifest.ProductBarcode = await GenerateNextProductBarcodeAsync();
                    try
                    {
                        await _context.SaveChangesAsync();
                        break;
                    }
                    catch (DbUpdateException) when (attempt < 4)
                    {
                        // PRBC collision - recompute the next number and try again.
                    }
                }

                TempData["SuccessMessage"] =
                    $"Manifest {viewModel.Manifest.ManifestNumber} created. Product barcode (PRBC): {viewModel.Manifest.ProductBarcode}.";
                return RedirectToAction(nameof(Details), new { id = viewModel.Manifest.Id });
            }

            viewModel.Vessels = await _context.VesselMasters.ToListAsync();
            return View(viewModel);
        }

        // MAN000001 style, zero-padded and unique per manifest.
        private static string GenerateManifestNumber(int id) => $"MAN{id:D6}";

        // The PRBC (product barcode) sequence starts here on a fresh system. Change this
        // to continue from your existing sequence (e.g. 16120).
        private const long ProductBarcodeStart = 10001;

        // Returns the next sequential product barcode (PRBC), matching the legacy system's
        // short numeric codes (e.g. 16112, 16116, 16120 ...).
        private async Task<string> GenerateNextProductBarcodeAsync()
        {
            var existingCodes = await _context.Manifests
                .Where(m => m.ProductBarcode != null)
                .Select(m => m.ProductBarcode!)
                .ToListAsync();

            long max = ProductBarcodeStart - 1;
            foreach (var code in existingCodes)
            {
                if (long.TryParse(code, out var value) && value > max)
                {
                    max = value;
                }
            }

            return (max + 1).ToString();
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var manifest = await _context.Manifests
                .Include(m => m.VesselMaster)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manifest == null)
            {
                return NotFound();
            }

            var viewModel = new ManifestViewModel
            {
                Manifest = manifest,
                Vessels = await _context.VesselMasters.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ManifestViewModel viewModel)
        {
            if (id != viewModel.Manifest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verify the vessel exists
                    var vessel = await _context.VesselMasters
                        .FirstOrDefaultAsync(v => v.VesselCode == viewModel.Manifest.VesselCode);

                    if (vessel == null)
                    {
                        ModelState.AddModelError("Manifest.VesselCode", "Vessel not found");
                        viewModel.Vessels = await _context.VesselMasters.ToListAsync();
                        return View(viewModel);
                    }

                    viewModel.Manifest.ModifiedDate = DateTime.Now;
                    _context.Update(viewModel.Manifest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ManifestExists(viewModel.Manifest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.Vessels = await _context.VesselMasters.ToListAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var manifest = await _context.Manifests
                .Include(m => m.VesselMaster)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manifest == null)
            {
                return NotFound();
            }

            return View(manifest);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var manifest = await _context.Manifests
                .Include(m => m.VesselMaster)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manifest == null)
            {
                return NotFound();
            }

            return View(manifest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manifest = await _context.Manifests.FindAsync(id);
            if (manifest != null)
            {
                _context.Manifests.Remove(manifest);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Manifest/PrintBarcode
        // Shows the lookup form. When a vessel number + product barcode are supplied
        // (via the form or a scanned QR link) it renders the manifest details plus a
        // scannable Code 128 barcode and QR code.
        [HttpGet]
        public async Task<IActionResult> PrintBarcode(string? vesselCode, string? productBarcode)
        {
            var viewModel = new PrintBarcodeViewModel
            {
                VesselCode = vesselCode,
                ProductBarcode = productBarcode,
                Vessels = await _context.VesselMasters
                    .OrderBy(v => v.VesselCode)
                    .ToListAsync()
            };

            // Only attempt a lookup once both fields are provided.
            if (string.IsNullOrWhiteSpace(vesselCode) && string.IsNullOrWhiteSpace(productBarcode))
            {
                return View(viewModel);
            }

            viewModel.Searched = true;

            if (string.IsNullOrWhiteSpace(vesselCode) || string.IsNullOrWhiteSpace(productBarcode))
            {
                viewModel.ErrorMessage = "Please enter both the vessel number and the product barcode.";
                return View(viewModel);
            }

            var trimmedVessel = vesselCode.Trim();
            var trimmedBarcode = productBarcode.Trim();

            var manifest = await _context.Manifests
                .Include(m => m.VesselMaster)
                .FirstOrDefaultAsync(m =>
                    m.VesselCode == trimmedVessel &&
                    m.ProductBarcode == trimmedBarcode);

            if (manifest == null)
            {
                viewModel.ErrorMessage =
                    $"No manifest found for vessel '{trimmedVessel}' with product barcode '{trimmedBarcode}'.";
                return View(viewModel);
            }

            viewModel.Manifest = manifest;
            viewModel.BarcodeSvg = _barcodeService.GetCode128Svg(manifest.ProductBarcode!);

            // The QR code points back to this page pre-filled, so scanning it with a
            // phone/tablet opens the manifest details directly.
            var scanUrl = Url.Action(
                nameof(PrintBarcode),
                "Manifest",
                new { vesselCode = manifest.VesselCode, productBarcode = manifest.ProductBarcode },
                Request.Scheme);
            viewModel.QrDataUri = _barcodeService.GetQrCodePngDataUri(scanUrl ?? manifest.ProductBarcode!);

            return View(viewModel);
        }

        private bool ManifestExists(int id)
        {
            return _context.Manifests.Any(e => e.Id == id);
        }
    }
}