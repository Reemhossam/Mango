using AutoMapper;
using Azure;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    public class CouponAPIController(AppDbContext _db,IMapper _mapper) : ControllerBase
    {
        private ResponseDto _response = new();
        [HttpGet]
        [Authorize]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Coupon> objlist = _db.Coupons.ToList();
                _response.Result = _mapper.Map<IEnumerable<CouponDto>>(objlist);
            }
            catch(Exception ex) 
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
                Coupon obj = _db.Coupons.Find(id)!;
                _response.Result = _mapper.Map<CouponDto>(obj);
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
        [Route("GetByCode/{code}")]
        [Authorize]
        public ResponseDto GetByCode(string code)
        {
            try
            {
                Coupon obj = _db.Coupons.FirstOrDefault(c => c.CouponCode.ToLower() == code.ToLower())!;
                _response.Result = _mapper.Map<CouponDto>(obj);
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
        [Authorize (Roles ="ADMIN")]
        public ResponseDto Post(CouponDto couponDto)
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(couponDto);
                if (obj is null)
                    _response.IsSuccess = false;
                _db.Coupons.Add(obj);
                _db.SaveChanges();

                var options = new Stripe.CouponCreateOptions
                {
                    Duration = "once",
                    Id = couponDto.CouponCode,
                    AmountOff = (long)(couponDto.DiscountAmount * 100),
                    Name = couponDto.CouponCode,
                    Currency = "usd"
                };
                var service = new Stripe.CouponService();
                service.Create(options);
                _response.Result = _mapper.Map<CouponDto>(obj);
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
        public ResponseDto Put(CouponDto couponDto)
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(couponDto);
                if (obj is null)
                    _response.IsSuccess = false;
                _db.Coupons.Update(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<CouponDto>(obj);
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
                Coupon obj = _db.Coupons.Find(id)!;
                if (obj is null)
                    _response.IsSuccess = false;
                _db.Coupons.Remove(obj);
                _db.SaveChanges();
                
                var service = new Stripe.CouponService();
                service.Delete($"{obj.CouponCode}");
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

