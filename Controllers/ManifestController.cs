// Controllers/ManifestController.cs
using Break_Bulk_System.Data;
using Break_Bulk_System.Models;
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

        public ManifestController(ApplicationDbContext context)
        {
            _context = context;
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
                return RedirectToAction(nameof(Index));
            }

            viewModel.Vessels = await _context.VesselMasters.ToListAsync();
            return View(viewModel);
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

        private bool ManifestExists(int id)
        {
            return _context.Manifests.Any(e => e.Id == id);
        }
    }
}