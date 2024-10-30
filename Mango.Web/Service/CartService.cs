using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service
{
    public class CartService(IBaseService _baseService) : ICartService
    {
        public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new()
            {
                ApiType = ApiType.POST,
                Url = ShoppingCartAPIBase + "/api/cart/ApplyCoupon",
                Data = cartDto
            }) ;
        }

        public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new()
            {
                ApiType = ApiType.GET,
                Url = ShoppingCartAPIBase + $"/api/cart/GetCart/{userId}",
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
        {
            return await _baseService.SendAsync(new()
            {
                ApiType = ApiType.POST,
                Url = ShoppingCartAPIBase + $"/api/cart/RemoveCart",
                Data = cartDetailsId
            });
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new()
            {
                ApiType = ApiType.POST,
                Url = ShoppingCartAPIBase + $"/api/cart/CartUpsert",
                Data = cartDto
            });
        }
    }
}
