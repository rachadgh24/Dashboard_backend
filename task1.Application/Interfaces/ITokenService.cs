namespace task1.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string phoneNumber, string name, string role, Guid tenantId);
    }
}
