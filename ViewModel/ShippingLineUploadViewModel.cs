// ViewModels/ShippingLineUploadViewModel.cs
using Microsoft.AspNetCore.Http;

namespace Break_Bulk_System.ViewModels
{
    public class ShippingLineUploadViewModel
    {
        public IFormFile CsvFile { get; set; }
    }
}