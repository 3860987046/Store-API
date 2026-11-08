using MediatR;
using StoreAPI.DTOs;

namespace StoreAPI.Features.Orders.Commands
{
    //  برگه سفارش برای ثبت سفارش. خروجی: آیدی سفارش ساخته شده 
    public class CreateOrderCommand : IRequest<int>
    {
        public string UserId { get; set; } //  از توکن گرفته میشه نه از ورودی کاربر)
        public List<CreateOrderItemDto> Items { get; set; } //  لیست محصولاتی که کاربر میخواد شامل نوع و تعدادش

        public CreateOrderCommand(string userId, List<CreateOrderItemDto> items)
        {
            UserId = userId;
            Items = items;
        }
    }
}