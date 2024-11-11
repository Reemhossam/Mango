using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController(IOrderService _orderService) : Controller
    {
        [Authorize]
        public IActionResult OrderIndex()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> OrderDetail(int orderId)
        {
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            var userId = User.Claims.Where(u=>u.Type == JwtRegisteredClaimNames.Sub).FirstOrDefault().Value;
            var response = await _orderService.GetOrderByIdAsync(orderId);
            if (response != null && response.IsSuccess)
            {
                orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            }
            if (!User.IsInRole(SD.RoleAdmin) && userId!=orderHeaderDto.UserId)
            {
                return NotFound();
            }
            return View(orderHeaderDto);
        }
        [HttpPost("OrderReadyForPackup")]
        public async Task<IActionResult> OrderReadyForPackup(int orderId)
        {
            var response = await _orderService.UpdateOrderStatusASync(orderId,SD.Status_ReadyForPackup);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status Updated Successfully";
                return RedirectToAction("OrderDetail",new { orderId =orderId});
            }
            return NotFound();
        }
        [HttpPost("CompleteOrder")]
        public async Task<IActionResult> CompleteOrder(int orderId)
        {
            var response = await _orderService.UpdateOrderStatusASync(orderId, SD.Status_Completed);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status Updated Successfully";
                return RedirectToAction("OrderDetail", new { orderId = orderId });
            }
            return NotFound();
        }
        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var response = await _orderService.UpdateOrderStatusASync(orderId, SD.Status_Cancelled);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status Updated Successfully";
                return RedirectToAction("OrderDetail", new { orderId = orderId });
            }
            return NotFound();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(string status) 
        {
            IEnumerable<OrderHeaderDto> list;
            string userId = "";
            if(!User.IsInRole(SD.RoleAdmin) && User.Identity.IsAuthenticated)
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;
            }
            ResponseDto response= await _orderService.GetAllOrdersAsync(userId);
            if (response != null && response.IsSuccess) 
            {
                list = JsonConvert.DeserializeObject<IEnumerable<OrderHeaderDto>>(Convert.ToString(response.Result));
                switch (status){
                  case "approved":   
                    list = list.Where(s=>s.Status==SD.Status_Approved).ToList();
                     break;
                  case "readyforpickup":
                    list = list.Where(s => s.Status == SD.Status_ReadyForPackup).ToList();
                    break;
                  case "cancelled":
                    list = list.Where(s => s.Status == SD.Status_Cancelled || s.Status == SD.Status_Refuned).ToList();
                    break;
                  default:
                    break;
                }
            }
            else
            {
                list = new List<OrderHeaderDto>();
            }
            return Json(new { data= list });
        }
    }
}
