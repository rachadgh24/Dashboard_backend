using System.ComponentModel.DataAnnotations.Schema;

namespace task1.DataLayer.Entities
{
    [Table("RoleClaims")]
    public class RoleClaim
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int ClaimId { get; set; }
        public Claim Claim { get; set; } = null!;
    }
}
