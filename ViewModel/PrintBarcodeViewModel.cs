using Break_Bulk_System.Models;

namespace Break_Bulk_System.ViewModels
{
    public class PrintBarcodeViewModel
    {
        // Search inputs
        public string? VesselCode { get; set; }
        public string? ProductBarcode { get; set; }

        // Vessel dropdown for the search form
        public List<VesselMaster> Vessels { get; set; } = new List<VesselMaster>();

        // Whether a lookup was attempted (so the view knows to show results / not-found)
        public bool Searched { get; set; }

        // Result
        public Manifest? Manifest { get; set; }
        public string? BarcodeSvg { get; set; }
        public string? QrDataUri { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
