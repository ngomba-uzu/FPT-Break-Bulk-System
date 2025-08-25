using Break_Bulk_System.Models;

namespace Break_Bulk_System.ViewModels
{
    public class ManifestViewModel
    {
        public Manifest Manifest { get; set; } = new Manifest();
        public List<VesselMaster> Vessels { get; set; } = new List<VesselMaster>();
    }
}