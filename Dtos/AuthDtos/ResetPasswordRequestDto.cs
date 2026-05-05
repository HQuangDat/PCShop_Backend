namespace PCShop_Backend.Dtos.AuthDtos
{
    public class ResetPasswordRequestDto
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
