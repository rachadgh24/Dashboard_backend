namespace task1.Application.Models
{
    public class AdminRoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AdminClaimModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
