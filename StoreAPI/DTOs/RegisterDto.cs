namespace StoreAPI.DTOs
{
    public class RegisterDto
    {
        //کامپایلر می‌گوید: «اگر کسی این کلاس را بسازد و به این پراپرتی‌ها مقدار ندهد
        //آن‌ها null می‌شوند و این با قانون "نباید null باشند در تضاد است. چوم ما ان ها راnullable تعریف نکردیم
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CaptchaAnswer { get; set; }
        public string CaptchaToken { get; set; }
    }
}
