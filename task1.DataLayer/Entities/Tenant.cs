using System.ComponentModel.DataAnnotations.Schema;

namespace task1.DataLayer.Entities{

    [Table("Tenants")]
    public class Tenant{
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);

public ICollection<User> Users { get; set; } = new List<User>();
public ICollection<Car> Cars { get; set; } = new List<Car>();
public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
public ICollection<Role> Roles { get; set; } = new List<Role>();
public ICollection<Customer> Customers { get; set; } = new List<Customer>();






    }

}