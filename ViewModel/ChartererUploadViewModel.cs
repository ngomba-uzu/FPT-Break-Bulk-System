// ViewModels/ChartererUploadViewModel.cs
using Microsoft.AspNetCore.Http;

namespace Break_Bulk_System.ViewModels
{
    public class ChartererUploadViewModel
    {
        public IFormFile CsvFile { get; set; }
    }
}