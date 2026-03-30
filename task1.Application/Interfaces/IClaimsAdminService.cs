using task1.Application.Models;

namespace task1.Application.Interfaces
{
    public interface IClaimsAdminService
    {
        Task<List<AdminClaimModel>> GetAllAsync();
    }
}

