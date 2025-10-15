// ViewModels/VesselMasterViewModel.cs
using Break_Bulk_System.Models;

namespace Break_Bulk_System.ViewModels
{
    public class VesselMasterViewModel
    {
        public VesselMaster VesselMaster { get; set; } = new VesselMaster();
        public List<VesselType> VesselTypes { get; set; } = new List<VesselType>();
        public List<ShippingLine> ShippingLines { get; set; } = new List<ShippingLine>();
        public List<TransportSea> CallSigns { get; set; } = new List<TransportSea>();

        public List<Charterer> Charterers { get; set; } = new List<Charterer>();


    }
}