using Break_Bulk_System.Models;

namespace Break_Bulk_System.ViewModels
{
    public class PrintAllBarcodesViewModel
    {
        // Search input
        public string? VesselCode { get; set; }

        // Vessel dropdown for the search form
        public List<VesselMaster> Vessels { get; set; } = new List<VesselMaster>();

        // Whether a lookup was attempted (so the view knows to show results / not-found)
        public bool Searched { get; set; }

        // The selected vessel (for the printed heading)
        public VesselMaster? Vessel { get; set; }

        // One entry per manifest belonging to the vessel, each with its rendered barcode.
        public List<BarcodeLabel> Labels { get; set; } = new List<BarcodeLabel>();

        public string? ErrorMessage { get; set; }

        public class BarcodeLabel
        {
            public Manifest Manifest { get; set; } = null!;
            public string BarcodeSvg { get; set; } = string.Empty;
            public string QrDataUri { get; set; } = string.Empty;
        }
    }
}
