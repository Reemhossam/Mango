using Mango.Services.RewardAPI.Messaging;

namespace Mango.Services.RewardAPI.Service
{
    public interface IRewardService
    {
        Task UpdateReward(RewardMessage rewardMessage);
    }
}
