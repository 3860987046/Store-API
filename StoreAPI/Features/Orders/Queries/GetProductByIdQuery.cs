using MediatR;
using StoreAPI.Models;

namespace StoreAPI.Features.Orders.Queries
{//  به MediatR می‌گه من یک درخواستم و جوابی که از من برمی‌گرده از نوع Productهست که میتونه null باشه 
    public class GetProductByIdQuery : IRequest<Product?>
    {
        public int Id { get; set; }
    }
}