// ViewModels/TransportSeaUploadViewModel.cs
using Microsoft.AspNetCore.Http;

namespace Break_Bulk_System.ViewModels
{
    public class TransportSeaUploadViewModel
    {
        public IFormFile CsvFile { get; set; }
    }
}