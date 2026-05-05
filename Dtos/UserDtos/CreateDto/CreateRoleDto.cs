namespace PCShop_Backend.Dtos.UserDtos.CreateDto
{
    public class CreateRoleDto
    {
        public required string RoleName { get; set; }
        public string? Description { get; set; }
    }
}
