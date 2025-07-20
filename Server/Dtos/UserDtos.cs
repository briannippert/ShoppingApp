namespace Server.Dtos
{
    public record UserDto(string FullName, string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record UpdateUserDto(string? FullName, string? Email);
    public record ChangePasswordDto(string OldPassword, string NewPassword);
}
