using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Messaging;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Services.EmailAPI.Service
{
    public class EmailService: IEmailService
    {
        private DbContextOptions<AppDbContext> _dbOptions;

        public EmailService(DbContextOptions<AppDbContext> dbOptions)
        {
            this._dbOptions = dbOptions;
        }

        public async Task EmailCartAndLog(CartDto cartDto)
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("<br/>Cart Email Requested ");
            message.AppendLine("<br/>Total "+cartDto.CartHeaderDto.CartTotal);
            message.Append("<br/>");
            message.Append("<ul>");
            foreach(var item in cartDto.CartDetailsDtos)
            {
                message.Append("<li>");
                message.Append(item.ProductId +" x "+item.Count);
                message.Append("<li/>");
            }
            message.Append("<ul/>");
            await LogAndEmail(message.ToString(), cartDto.CartHeaderDto.Email);
        }

        public async Task EmailCartAndLog(string email)
        {


            EmailLogger emailLogger = new()
            {
                Email = email,
                EmailSend = DateTime.Now,
                Message = "new user register",
            };
            using var _db = new AppDbContext(_dbOptions);
            _db.EmailLoggers.Add(emailLogger);
            await _db.SaveChangesAsync();

            }

        public async Task LogOrderPlaced(RewardMessage rewardMessage)
        {
            string message = "New Order Placed. <br/> Order ID:"+ rewardMessage.OrderId;
            await LogAndEmail(message, "reemhossam@gmail.com");
        }

        private async Task<bool>LogAndEmail(string message, string email)
        {
            try
            {
                EmailLogger emailLogger = new ()
                {
                    Email = email,
                    EmailSend = DateTime.Now,
                    Message = message,
                };
                using var _db = new AppDbContext(_dbOptions);
                _db.EmailLoggers.Add(emailLogger);
                await _db.SaveChangesAsync();
                return true;
            }
            catch 
            { 
                return false;
            }
        }
    }
}
