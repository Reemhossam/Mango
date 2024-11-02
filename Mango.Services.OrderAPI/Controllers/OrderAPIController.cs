using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController(AppDbContext _db, IMapper _mapper, IProductService _productService) : ControllerBase
    {
        ResponseDto response = new ResponseDto();
        
       // [Authorize]
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
                response.Result = orderCreated.OrderHeaderId;
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
