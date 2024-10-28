using AutoMapper;
using Mango.Services.ShoppingCart.Models.Dto;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController(AppDbContext _db, IMapper _mapper) : ControllerBase
    {
        ResponseDto response = new();
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
                    d.ProductId == cartDto.CartDetailsDtos.First().ProductId && d.CartHeaderId == cartDto.CartHeaderDto.CartHeaderId);
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
    }
}
