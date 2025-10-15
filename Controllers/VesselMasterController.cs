// Controllers/VesselMasterController.cs
using Break_Bulk_System.Data;
using Break_Bulk_System.Models;
using Break_Bulk_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Break_Bulk_System.Controllers
{
    [Authorize]
    public class VesselMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VesselMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vessels = await _context.VesselMasters.OrderByDescending(v => v.CreatedDate).ToListAsync();
            return View(vessels);
        }



        // Controllers/VesselMasterController.cs (update the Create and Edit methods)
        public async Task<IActionResult> Create()
        {
            var viewModel = new VesselMasterViewModel
            {
                VesselTypes = await _context.VesselTypes.ToListAsync(),
                ShippingLines = await _context.ShippingLines.ToListAsync(),
                CallSigns = await _context.TransportSeas.OrderBy(t => t.TransportID).ToListAsync(),
                  Charterers = await _context.Charterers.OrderBy(c => c.Description).ToListAsync()

            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VesselMasterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if vessel code already exists
                if (await _context.VesselMasters.AnyAsync(v => v.VesselCode == viewModel.VesselMaster.VesselCode))
                {
                    ModelState.AddModelError("VesselMaster.VesselCode", "Vessel code already exists.");
                    viewModel.VesselTypes = await _context.VesselTypes.ToListAsync();
                    viewModel.ShippingLines = await _context.ShippingLines.ToListAsync();
                    return View(viewModel);
                }

                _context.Add(viewModel.VesselMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.VesselTypes = await _context.VesselTypes.ToListAsync();
            viewModel.ShippingLines = await _context.ShippingLines.ToListAsync();
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vesselMaster = await _context.VesselMasters
                .Include(v => v.VesselType)
                .Include(v => v.ShippingLine)
                .FirstOrDefaultAsync(v => v.VesselCode == id);

            if (vesselMaster == null)
            {
                return NotFound();
            }

            var viewModel = new VesselMasterViewModel
            {
                VesselMaster = vesselMaster,
                VesselTypes = await _context.VesselTypes.ToListAsync(),
                ShippingLines = await _context.ShippingLines.ToListAsync(),
                CallSigns = await _context.TransportSeas.OrderBy(t => t.TransportID).ToListAsync(),

                  Charterers = await _context.Charterers.OrderBy(c => c.Description).ToListAsync()
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, VesselMasterViewModel viewModel)
        {
            if (id != viewModel.VesselMaster.VesselCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.VesselMaster.ModifiedDate = DateTime.Now;
                    _context.Update(viewModel.VesselMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VesselMasterExists(viewModel.VesselMaster.VesselCode))
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

            viewModel.VesselTypes = await _context.VesselTypes.ToListAsync();
            viewModel.ShippingLines = await _context.ShippingLines.ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vesselMaster = await _context.VesselMasters
                .FirstOrDefaultAsync(m => m.VesselCode == id);
            if (vesselMaster == null)
            {
                return NotFound();
            }

            return View(vesselMaster);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vesselMaster = await _context.VesselMasters
                .FirstOrDefaultAsync(m => m.VesselCode == id);
            if (vesselMaster == null)
            {
                return NotFound();
            }

            return View(vesselMaster);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var vesselMaster = await _context.VesselMasters.FindAsync(id);
            if (vesselMaster != null)
            {
                _context.VesselMasters.Remove(vesselMaster);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VesselMasterExists(string id)
        {
            return _context.VesselMasters.Any(e => e.VesselCode == id);
        }
    }
}

