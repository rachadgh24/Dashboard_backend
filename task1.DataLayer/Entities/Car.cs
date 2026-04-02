using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task1.DataLayer.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public int maxSpeed { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}