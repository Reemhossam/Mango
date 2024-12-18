﻿using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController(ICartService _cartService, IOrderService _orderService) : Controller
    {
        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCardDtoBasedOnLoggedInUser());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCardDtoBasedOnLoggedInUser());
        }
        [HttpPost]
        [ActionName("CreateOrder")]
        public async Task<IActionResult> CreateOrder(CartDto cartDto)
        {
            CartDto cart = await LoadCardDtoBasedOnLoggedInUser();
            cart.CartHeaderDto.Name = cartDto.CartHeaderDto.Name;
            cart.CartHeaderDto.Email = cartDto.CartHeaderDto.Email;
            cart.CartHeaderDto.Phone = cartDto.CartHeaderDto.Phone;

            ResponseDto? response = await _orderService.CreateOrderAsync(cart);
            OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            if (response != null & response.IsSuccess)
            {
                //get stripe session and redirect to stripe to place order
                var domain = Request.Scheme + "://" + Request.Host.Value + "/"; /* https://localhost/  */
                StripeRequestDto stripeRequestDto = new()
                {
                    orderHeader = orderHeaderDto,
                    ApprovedUrl = domain + $"cart/Confirmation?orderId={orderHeaderDto.OrderHeaderId}",
                    CancelUrl = domain + "cart/Checkout",
                };
                ResponseDto stripeResponse = await _orderService.CreateStripeSessionAsync(stripeRequestDto);
                StripeRequestDto stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponse.Result));
                Response.Headers.Add("location", stripeResponseResult.StripeSessionUrl);
                return new StatusCodeResult (303);  //meaning redirict to another page
            }
            return RedirectToAction("Checkout");
        }

        [Authorize]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            ResponseDto? response = await _orderService.ValidateStripeSessionAsync(orderId);
            if (response != null & response.IsSuccess)
            {
                OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
                if(orderHeaderDto.Status == SD.Status_Approved)
                    return View(orderId);
            }
            //or redirect to another view based on status in this course we dont care to handle this point 
            return View(orderId);
        }
        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            ResponseDto? response = await _cartService.RemoveFromCartAsync(cartDetailsId);
            if (response != null & response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction("CartIndex");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            CartDto cart = await LoadCardDtoBasedOnLoggedInUser();
            cart.CartHeaderDto.CouponCode = cartDto.CartHeaderDto.CouponCode;
            ResponseDto? response = await _cartService.ApplyCouponAsync(cart);
            if (response != null & response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction("CartIndex");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            CartDto cart = await LoadCardDtoBasedOnLoggedInUser();
            cartDto.CartHeaderDto.CouponCode = string.Empty;
            cart.CartHeaderDto.CouponCode = cartDto.CartHeaderDto.CouponCode;
            ResponseDto? response = await _cartService.ApplyCouponAsync(cart);
            if (response != null & response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction("CartIndex");
            }
            return View();
        }
        private async Task<CartDto> LoadCardDtoBasedOnLoggedInUser()
        {
            // to get id of authenticated User
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId);
            if (response != null & response.IsSuccess) 
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return cartDto;
            }
            return new();
        }
        [Authorize]
        [HttpPost]
        [ActionName("EmailCart")]
        public async Task<IActionResult> EmailCart()
        {
            CartDto cart = await LoadCardDtoBasedOnLoggedInUser();
            cart.CartHeaderDto.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;
            ResponseDto? response = await _cartService.EmailCart(cart);
            if (response != null & response.IsSuccess)
            {
                TempData["success"] = "Email will be processed and send shortly.";
                return RedirectToAction("CartIndex");
            }
            return View();
        }
    }
}
