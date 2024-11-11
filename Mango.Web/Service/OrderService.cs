using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using static Mango.Web.Utility.SD;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

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

        public async Task<ResponseDto?> GetAllOrdersAsync(string? userId = "")
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.OrderAPIBase + "/api/order/GetOrders",
                ApiType = SD.ApiType.GET,
                Data = userId
            });
        }

        public async Task<ResponseDto?> GetOrderByIdAsync(int id)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.OrderAPIBase + "/api/order/GetOrder/"+id,
                ApiType = SD.ApiType.GET
            });
        }

        public async Task<ResponseDto?> UpdateOrderStatusASync(int orderId, string newStatus)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.OrderAPIBase + "/api/order/UpdateOrderStatus/"+ orderId,
                ApiType = SD.ApiType.POST,
                Data = newStatus
            });
        }

        public async Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.OrderAPIBase + "/api/order/ValidateStripeSession",
                ApiType = SD.ApiType.POST,
                Data = orderHeaderId
            });
        }
    }
}
