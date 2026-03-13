namespace task1.Application.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string City { get; set; }
        public string? Email { get; set; }
        public List<CarModel> Cars { get; set; } = new();
    }
}
