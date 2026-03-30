using System.ComponentModel.DataAnnotations.Schema;

namespace task1.DataLayer.Entities
{
    [Table("Claims")]
    public class Claim
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
        
    }
}
