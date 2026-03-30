using System.ComponentModel.DataAnnotations.Schema;

namespace task1.DataLayer.Entities
{
    [Table("Roles")]
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
    public Guid TenantId { get; set; }       
    public Tenant Tenant { get; set; } = null!;
    }
}
