using AutoMapper;
using Mango.Services.ShoppingCart.Models.Dto;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.RabbitMQSender;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController(AppDbContext _db, IMapper _mapper, IProductService _productService, ICouponService _couponService,
        IRabbitMQCartMessageSender _rabbitMQCartMessageSender, IConfiguration _configuration) : ControllerBase
    {
        ResponseDto response = new();
        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cartDto = new CartDto() ;
                cartDto.CartHeaderDto = _mapper.Map<CartHeaderDto>(await _db.CartHeaders.FirstAsync(h => h.UserId == userId));
                cartDto.CartDetailsDtos = _mapper.Map<List<CartDetailsDto>>(await _db.CartDetails.Where(d => d.CartHeaderId == cartDto.CartHeaderDto.CartHeaderId).ToListAsync());
                // here service cartAPI send request to productAPI
                IEnumerable<ProductDto> products = await _productService.GetProducts();

                foreach (var item in cartDto.CartDetailsDtos)
                {
                    item.Product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                    cartDto.CartHeaderDto.CartTotal += (item.Count * item.Product.Price);
                }
                // apply coupon discount in shopping cart
                if (!string.IsNullOrEmpty(cartDto.CartHeaderDto.CouponCode))
                {
                    CouponDto coupon = await _couponService.GetCoupon(cartDto.CartHeaderDto.CouponCode);
                    if (coupon !=null && cartDto.CartHeaderDto.CartTotal >= coupon.MinAmount)
                    {
                        cartDto.CartHeaderDto.CartTotal -= coupon.DiscountAmount;
                        cartDto.CartHeaderDto.Discount = coupon.DiscountAmount;
                    }
                }
                
                response.Result = cartDto;
            }
            catch (Exception ex) 
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> Upsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = _db.CartHeaders.FirstOrDefault(c => c.UserId == cartDto.CartHeaderDto.UserId);
                if (cartHeaderFromDb == null) 
                { 
                    // create header and details
                    CartHeader header = _mapper.Map<CartHeader>(cartDto.CartHeaderDto);
                    _db.CartHeaders.Add(header);
                    await _db.SaveChangesAsync();

                    cartDto.CartDetailsDtos.First().CartHeaderId =header.CartHeaderId; //for populate CartHeaderId to detail
                    var detail = _mapper.Map<CartDetails>(cartDto.CartDetailsDtos.First());
                    _db.CartDetails.Add(detail);
                    await _db.SaveChangesAsync();

                }
                else
                {
                    // there is header ,check if detail is exist  // كل مره الdetail دي بيبقي فيها product واحد عشان كده بنستخدم ال frist 
                    var cartDetailFromDb = _db.CartDetails.FirstOrDefault(d=> 
                    d.ProductId == cartDto.CartDetailsDtos.First().ProductId && d.CartHeaderId == cartHeaderFromDb.CartHeaderId);
                    if(cartDetailFromDb == null)
                    {
                        // create detail
                        cartDto.CartDetailsDtos.First().CartHeaderId = cartHeaderFromDb.CartHeaderId; //for populate CartHeaderId to detail
                        var detail = _mapper.Map<CartDetails>(cartDto.CartDetailsDtos.First());
                        _db.CartDetails.Add(detail);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        cartDetailFromDb.Count += cartDto.CartDetailsDtos.First().Count;
                        await _db.SaveChangesAsync();
                        //update for returning to result only
                         cartDto.CartDetailsDtos.First().Count = cartDetailFromDb.Count;
                         cartDto.CartDetailsDtos.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                         cartDto.CartDetailsDtos.First().CartDetailsId = cartDetailFromDb.CartDetailsId;
                    }
                }
                response.Result = cartDto;
                response.Message = "enter cartAPI controller";
            }
            catch (Exception ex) 
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }

        [HttpPost("EmailCartRequest")]
        public async Task<ResponseDto> EmailCartRequest(CartDto cartDto)
        {
            try
            {
                _rabbitMQCartMessageSender.Send(cartDto, _configuration.GetValue<string>("QueueNames:Checkoutqueue"));
                response.Result = true;
                response.Message = "enter cartAPI controller";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody]int cartDetailsId)
        {
            try
            {
               CartDetails cartDetails = _db.CartDetails.FirstOrDefault(d =>d.CartDetailsId == cartDetailsId);
                //get count as if is a only item in cart we will remove header with detail
                var totalCountOfCartItem = _db.CartDetails.Where(d=>d.CartHeaderId == cartDetails.CartHeaderId).Count();

                if (totalCountOfCartItem == 1) 
                {
                    var cartHeaderForRemove= _db.CartHeaders.FirstOrDefault(h =>h.CartHeaderId==cartDetails.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeaderForRemove); //not need to remove cartDetail As relation between them cascede
                }
                else
                {
                    _db.CartDetails.Remove(cartDetails);
                }
                await _db.SaveChangesAsync();
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }
        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var CartFromDb = await _db.CartHeaders.FirstAsync(c=> c.UserId == cartDto.CartHeaderDto.UserId);
                CartFromDb.CouponCode = cartDto.CartHeaderDto?.CouponCode;
                _db.CartHeaders.Update(CartFromDb);
                await _db.SaveChangesAsync();
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }
    }
}
