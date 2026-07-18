using System.Threading.Tasks;
using Greenix.Shared.Models;
using Greenix.Application.DTOs.Report;

namespace Greenix.Application.Interfaces
{
    public interface IReportService
    {
        Task<BaseResponse<byte[]>> ExportComplaintsToCsvAsync(ReportFilterRequest filters, string userId, string role);
    }
}
