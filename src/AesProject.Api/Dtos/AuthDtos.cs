namespace AesProject.Api.Dtos
{
    public record OnboardRequestDto(string Username, string Password);
    public record LoginRequestDto(string Username, string Password, bool RememberMe);
}
