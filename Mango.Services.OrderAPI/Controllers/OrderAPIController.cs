using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.RabbitMQSender;
using Mango.Services.OrderAPI.Service;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController(AppDbContext _db, IMapper _mapper, IProductService _productService,
        IRabbitMQCartMessageSender _rabbitMQCartMessageSender) : ControllerBase
    {
        ResponseDto response = new ResponseDto();

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder(CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeaderDto);
                orderHeaderDto.OrderTime = DateTime.Now;
                orderHeaderDto.Status = SD.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetailsDtos);
                OrderHeader orderCreated = _mapper.Map<OrderHeader>(orderHeaderDto);
                _db.OrderHeaders.Add(orderCreated);
                //_db.OrderDetails.AddRange(orderCreated.OrderDetails);
                await _db.SaveChangesAsync();
                response.Result = orderCreated;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment"
                };
                var discountOptions = new List<SessionDiscountOptions>
                { new SessionDiscountOptions()
                   {
                     Coupon= $"{stripeRequestDto.orderHeader.CouponCode}"
                   }
                };
                if (stripeRequestDto.orderHeader.Discount > 0)
                {
                    options.Discounts = discountOptions;
                }
                foreach (var item in stripeRequestDto.orderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.ProductPrice * 100),  //20.99 =>2099 to save as string
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductName,
                            }
                        },
                        Quantity = item.Count,

                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;
                OrderHeader orderHeader = _db.OrderHeaders.First(h => h.OrderHeaderId == stripeRequestDto.orderHeader.OrderHeaderId);
                orderHeader.StripeSessionId = session.Id;
                _db.SaveChanges();
                response.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.Find(orderHeaderId);
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.StripeSessionId);
                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
                if(paymentIntent.Status == "succeeded")
                {
                    //then payment was successful
                    orderHeader.PaymentIntendId = paymentIntent.Id;
                    orderHeader.Status = SD.Status_Approved;
                    _db.SaveChanges();
                    RewardsDto rewardsDto = new RewardsDto()
                    {
                        OrderId = orderHeaderId,
                        UserId = orderHeader.UserId,
                        RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal)
                    };
                    _rabbitMQCartMessageSender.Send(rewardsDto);
                    response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        
        [Authorize]
        [HttpGet("GetOrders")]
        public async Task<ResponseDto?> GetOrders([FromBody] string? userId="")
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders = new List<OrderHeader>();
                if (User.IsInRole(SD.RoleAdmin))
                    orderHeaders=_db.OrderHeaders.Include(o => o.OrderDetails).OrderByDescending(o =>o.OrderHeaderId).ToList();
                else
                    orderHeaders=_db.OrderHeaders.Include(o =>o.OrderDetails).Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.OrderHeaderId).ToList();
                response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(orderHeaders);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public async Task<ResponseDto?> GetOrderById(int id)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.Include(o => o.OrderDetails).First(i => i.OrderHeaderId == id);
                response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto?> UpdateOrderStatus(int orderId, [FromBody] string newStatus)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.Find(orderId);
                if (orderHeader != null)
                {
                    if(newStatus == SD.Status_Cancelled)
                    {
                        // we will give refund هنرجع الفلوس 
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntendId
                        };
                        var service = new RefundService();
                        Refund refund = service.Create(options);
                        //orderHeader.Status = SD.Status_Cancelled;
                    }
                    orderHeader.Status = newStatus;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
