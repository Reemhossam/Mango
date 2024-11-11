﻿using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> CreateOrderAsync(CartDto cartDto);
        Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto);
        Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId);
        Task<ResponseDto?> GetAllOrdersAsync(string? userId = "");
        Task<ResponseDto?> GetOrderByIdAsync(int id);
        Task<ResponseDto?> UpdateOrderStatusASync(int orderId, string newStatus);
    }
}
