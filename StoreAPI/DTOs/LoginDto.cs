namespace StoreAPI.DTOs
{
    public class LoginDto
    {
        //این ورودی ها ممکن است null باشند یعنی کاربر وارد نکند،
        //که تو در این صورت ان ها را خالی درنظر بگیر تا بعدا توسط validation خطای مربوطه که نباید خالی باشد را بدهد
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
