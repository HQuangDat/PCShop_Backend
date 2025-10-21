namespace PCShop_Backend.Dtos.UserDtos.UpdateDto
{
    public class ChangePassWordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
