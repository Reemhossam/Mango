using IdentityModel;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class HomeController(ILogger<HomeController> _logger,IProductService _productService, ICartService _cartService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            List<ProductDto>? list = new();
            ResponseDto? response = await _productService.GetAllProductsAsync();
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(list);
        }

        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDto product=new();
            ResponseDto? response = await _productService.GetProductByIdAsync(productId);
            if (response != null && response.IsSuccess)
            {
                product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(product);
        }

        [Authorize]
        [HttpPost]
        [ActionName("ProductDetails")]
        public async Task<IActionResult> ProductDetails(ProductDto productDto)
        {
            CartDto cartDto = new CartDto()
            {
                CartHeaderDto= new CartHeaderDto()
                {
                    UserId = User.Claims.Where(u=> u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value,
                },
                CartDetailsDtos = new List<CartDetailsDto>()
                {
                    new CartDetailsDto()
                    {
                        ProductId = productDto.ProductId,
                        Count = productDto.Count
                    }
                } 
            };
            ResponseDto? response = await _cartService.UpsertCartAsync(cartDto);
          //  ResponseDto? response = await _cartService.EmailCart(cartDto);


            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Item Added To Shopping Cart";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = response?.Message;
                return View(productDto);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
