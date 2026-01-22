// Services/ITransportSeaCsvProcessor.cs
using Break_Bulk_System.Models;

namespace Break_Bulk_System.Services
{
    public interface ITransportSeaCsvProcessor
    {
        Task<bool> ProcessCsvFromUrlAsync();
        Task<List<TransportSea>> DownloadAndParseCsvAsync(string url);
    }
}