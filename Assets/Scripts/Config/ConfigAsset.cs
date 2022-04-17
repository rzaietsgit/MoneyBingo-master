using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConfigAsset : ScriptableObject
{
    public List<Config_CardRandomReward> config_CardRandomRewards;
    public List<Config_CardExtraReward> config_CardExtraRewards;
    public List<Config_WheelReward> config_WheelRewards;
    public List<Config_Task> config_Tasks;
    public List<Config_Gift> config_Gifts;
    public List<Config_LuckySpin> config_LuckySpins;
    public List<Config_MoreBall> config_MoreBalls;
    public List<Config_GiftBallon> config_GiftBallons;
    public List<Config_DrawBox> config_DrawBoxes;
    public List<Config_Pig> config_Pigs;
}
[System.Serializable]
public struct Config_CardRandomReward
{
    public int cash_max;
    public int cash_cardNum;
    public int cash_rewardNum;
    public int gold_max;
    public int gold_cardNum;
    public int gold_rewardNum;
}
[System.Serializable]
public struct Config_CardExtraReward
{
    public ExtraReward rewardType;
    public int weight;
    public Range numRange;
    public int mustBeInThreeTimes;
    public Reward appearLimitType;
    public int appearLimitMax;
}
[System.Serializable]
public struct Config_WheelReward
{
    public Reward rewardType;
    public int rewardNum;
    public int weight;
    public List<int> blackBox;
    public Reward appearLimitType;
    public int appearLimitMax;
    public List<Vector2Int> randomMultipleAndWeight;
}
[System.Serializable]
public struct Config_Task
{
    public string name;
    public int id;
    public TaskType taskType;
    public int needTimes;
    public List<Reward> rewardTypes;
    public List<int> rewardNums;
    public List<Reward> appearLimitTypes;
    public List<int> appearLimitMaxes;
    public List<Reward> randomMultipleRewardTypes;
    public List<Config_MultipleWeights> randomMultipleAndWeights;
}
[System.Serializable]
public struct Config_MultipleWeights
{
    public List<Vector2Int> multipleWeights;
}
[System.Serializable]
public struct Config_Gift
{
    public Reward giftType;
    public int redeemNeedNum;
}
[System.Serializable]
public struct Config_LuckySpin
{
    public Reward rewardType;
    public int weight;
    public int rewardNum;
    public List<int> blackBox;
    public Reward appearLimitType;
    public int appearLimitMax;
    public int firstGetRewardNum;
}
[System.Serializable]
public struct Config_MoreBall
{
    public int ballCount;
    public int weight;
    public int usefulCount;
    public int unUsefulCount;
    public int blackBox;
}
[System.Serializable]
public struct Config_GiftBallon
{
    public Reward rewardType;
    public int weight;
    public int rewardNum;
    public Reward appearLimitType;
    public int appearLimitMax;
}
[System.Serializable]
public struct Config_DrawBox
{
    public Reward rewardType;
    public int weight;
    public int rewardNum;
    public List<int> blackBox;
    public Reward appearLimitType;
    public int appearLimitMax;
}
[System.Serializable]
public struct Config_Pig
{
    public int maxTotalPigCash;
    public int rewardPigCashNum;
}
