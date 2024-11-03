using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class OrderService(IBaseService _baseService) : IOrderService
    {
        public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.OrderAPIBase + "/api/order/CreateOrder",
                ApiType = SD.ApiType.POST,
                Data = cartDto
            });
        }

        public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.OrderAPIBase + "/api/order/CreateStripeSession",
                ApiType = SD.ApiType.POST,
                Data = stripeRequestDto
            });
        }
    }
}
