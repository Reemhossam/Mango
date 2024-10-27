using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductAPIController(AppDbContext _db, IMapper _mapper) : ControllerBase
    {
        private ResponseDto _response = new();
        [HttpGet]
        [Authorize]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Product> objlist = _db.Products.ToList();
                _response.Result = _mapper.Map<IEnumerable<ProductDto>>(objlist);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        [Authorize]
        public ResponseDto Get(int id)
        {
            try
            {
                Product obj = _db.Products.Find(id)!;
                _response.Result = _mapper.Map<ProductDto>(obj);
                if (obj is null)
                    _response.IsSuccess = false;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("GetByName/{name}")]
        [Authorize]
        public ResponseDto GetByCode(string name)
        {
            try
            {
                Product obj = _db.Products.FirstOrDefault(p => p.Name.ToLower() == name.ToLower())!;
                _response.Result = _mapper.Map<ProductDto>(obj);
                if (obj is null)
                    _response.IsSuccess = false;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post(ProductDto productDto)
        {
            try
            {
                Product obj = _mapper.Map<Product>(productDto);
                if (obj is null)
                    _response.IsSuccess = false;
                _db.Products.Add(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put(ProductDto productDto)
        {
            try
            {
                Product obj = _mapper.Map<Product>(productDto);
                if (obj is null)
                    _response.IsSuccess = false;
                _db.Products.Update(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Delete(int id)
        {
            try
            {
                Product obj = _db.Products.Find(id)!;
                if (obj is null)
                    _response.IsSuccess = false;
                _db.Products.Remove(obj);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}
