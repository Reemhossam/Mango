namespace Mango.Web.Models
{
    public class CartDto
    {
        public CartHeaderDto CartHeaderDto { get; set; }
        public List<CartDetailsDto> CartDetailsDtos { get; set; }
    }
}
