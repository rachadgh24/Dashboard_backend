namespace task1.Application.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a user (includes Password and Role; password is not returned).
    /// </summary>
    public class CreateUserModel
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
