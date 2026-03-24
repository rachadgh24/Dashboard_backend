using System.ComponentModel.DataAnnotations.Schema;

namespace task1.DataLayer.Entities
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}
