namespace Mango.Services.OrderAPI.Models.Dto
{
    public class CartDto
    {
        public CartHeaderDto CartHeaderDto { get; set; }
        public List<CartDetailsDto> CartDetailsDtos { get; set; }
    }
}
