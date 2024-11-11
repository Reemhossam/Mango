using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardsAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.RewardAPI.Service
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> options;

        public RewardService(DbContextOptions<AppDbContext> options)
        {
            this.options = options;
        }

        public async Task UpdateReward(RewardMessage rewardMessage)
        {
            try
            {
                using var _db = new AppDbContext(this.options);
                Rewards reward = new Rewards()
                {
                    OrderId = rewardMessage.OrderId,
                    UserId = rewardMessage.UserId,
                    RewardsActivity = rewardMessage.RewardsActivity,
                    RewardsDate = DateTime.Now,
                };
                _db.Rewards.Add(reward);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) { }
        }
    }
}
