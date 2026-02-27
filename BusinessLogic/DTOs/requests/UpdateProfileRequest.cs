namespace BusinessLogic.DTOs.requests
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}
