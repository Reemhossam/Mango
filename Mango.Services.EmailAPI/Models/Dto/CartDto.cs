﻿namespace Mango.Services.EmailAPI.Models.Dto
{
    public class CartDto
    {
        public CartHeaderDto CartHeaderDto { get; set; }
        public List<CartDetailsDto> CartDetailsDtos { get; set; }
    }
}
