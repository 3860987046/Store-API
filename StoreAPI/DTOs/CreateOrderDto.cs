using System.ComponentModel.DataAnnotations;

namespace StoreAPI.DTOs
{

    public class CreateOrderDto // اطلاعات ورودی برای ایجاد سفارش
    {
        [Required] // ارسال فهرست محصولات الزامی است
        [MinLength(1)] // سفارش باید حداقل یک ردیف محصول داشته باشد
        public List<CreateOrderItemDto> Items { get; set; } = new(); // فهرست محصولات سفارش
    }

    public class CreateOrderItemDto // اطلاعات هر محصول در سفارش
    {
        [Range(1, int.MaxValue)] // شناسه محصول باید حداقل 1 باشد
        public int ProductId { get; set; } // شناسه محصول انتخاب‌شده

        [Range(1, int.MaxValue)] // تعداد باید حداقل 1 باشد
        public int Quantity { get; set; } // تعداد درخواستی محصول
    }
}
