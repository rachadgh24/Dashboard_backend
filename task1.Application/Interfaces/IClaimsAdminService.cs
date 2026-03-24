using task1.DataLayer.Entities;

namespace task1.Application.Interfaces
{
    public interface IClaimsAdminService
    {
        Task<List<Claim>> GetAllAsync();
    }
}

