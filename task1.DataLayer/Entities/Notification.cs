namespace task1.DataLayer.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid TenantId { get; set; }       
    public Tenant Tenant { get; set; } = null!;
    }
}
