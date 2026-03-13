using System.ComponentModel.DataAnnotations.Schema;

namespace task1.DataLayer.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [NotMapped]
        public List<Car> Cars { get; set; } = new();
    }
}
