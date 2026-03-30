namespace task1.Application.Models
{
    public class AuthLoginResultModel
    {
        public string Token { get; set; } = string.Empty;
    }

    public class AuthRegisterResultModel
    {
        public string Token { get; set; } = string.Empty;
    }

    public class AuthPermissionsResultModel
    {
        public List<AuthClaimModel> Claims { get; set; } = new();
    }

    public class AuthClaimModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
