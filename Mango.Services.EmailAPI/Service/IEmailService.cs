using Mango.Services.EmailAPI.Messaging;
using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Service
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task EmailCartAndLog(string email);
        Task LogOrderPlaced(RewardMessage rewardMessage);
    }
}
