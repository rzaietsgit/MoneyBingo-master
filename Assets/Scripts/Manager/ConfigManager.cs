using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager
{
    public const int NextBingoCardNeedSeconds = 600;
    public const int MaxBingoCardNum = 6;
    public const int OriginBingoCardNum = 6;
    public const int LuckySpinItemCount = 8;
    public const int GetMoreBingoCardsChancePerDay = 20;
    public const int GetMoreBallChancePerDay = 20;
    public const int NewPlayerRewardMoney = 5000;
    public const int MoneyBoxCashTargetExchangeNeedNum = 2000;
    public const int Pig_Task_NeedAdTimes = 5;
    public const int Pig_Task_NeedCardNum = 25;
    private static readonly int[] spinNeedBingoTime = new int[1] { 5 };
    private static ConfigAsset _configAsset;
    private static ConfigAsset ConfigAsset { get { if (_configAsset == null) _configAsset = Resources.Load<ConfigAsset>("Config");return _configAsset; } }
    public static int GetCardRandomRewardCardNum(Reward reward)
    {
        int compareNum;
        int configCount = ConfigAsset.config_CardRandomRewards.Count;
        switch (reward)
        {
            case Reward.Gold:
                compareNum = SaveManager.SaveData.gold;
                for (int i = 0; i < configCount; i++)
                {
                    if (compareNum <= ConfigAsset.config_CardRandomRewards[i].gold_max)
                    {
                        return ConfigAsset.config_CardRandomRewards[i].gold_cardNum;
                    }
                }
                break;
            case Reward.Cash:
                compareNum = SaveManager.SaveData.cash;
                for (int i = 0; i < configCount; i++)
                {
                    if (compareNum <= ConfigAsset.config_CardRandomRewards[i].cash_max)
                    {
                        return ConfigAsset.config_CardRandomRewards[i].cash_cardNum;
                    }
                }
                break;
        }
        return 0;
    }
    public static int GetCardRandomRewradNum(Reward reward)
    {
        int compareNum;
        int configCount = ConfigAsset.config_CardRandomRewards.Count;
        switch (reward)
        {
            case Reward.ManyGold:
            case Reward.Gold:
                compareNum = SaveManager.SaveData.gold;
                for (int i = 0; i < configCount; i++)
                {
                    if (compareNum <= ConfigAsset.config_CardRandomRewards[i].gold_max)
                    {
                        return ConfigAsset.config_CardRandomRewards[i].gold_rewardNum;
                    }
                }
                break;
            case Reward.ManyCash:
            case Reward.Cash:
                compareNum = SaveManager.SaveData.cash;
                for (int i = 0; i < configCount; i++)
                {
                    if (compareNum <= ConfigAsset.config_CardRandomRewards[i].cash_max)
                    {
                        return ConfigAsset.config_CardRandomRewards[i].cash_rewardNum;
                    }
                }
                break;
            case Reward.Key:
                return 1;
        }
        return 0;
    }
    public static List<Config_CardExtraReward> GetRandomExtraReward()
    {
        List<Config_CardExtraReward> configs = new List<Config_CardExtraReward>();
        int configCount = ConfigAsset.config_CardExtraRewards.Count;
        for(int i = 0; i < configCount; i++)
        {
            configs.Add(ConfigAsset.config_CardExtraRewards[i]);
        }
        return configs;
    }
    public static int GetRandomWheelReward(out int multiple,out bool needRandomMultiple)
    {
        int playWheelTime = SaveManager.SaveData.playWheelTimes;
        bool isLockGiveaways = SaveManager.SaveData.islockGiveaways;

        List<Vector2Int> randomList = new List<Vector2Int>();
        int totalWeight = 0;
        List<Config_WheelReward> config_Wheels = ConfigAsset.config_WheelRewards;
        int configCount = config_Wheels.Count;
        for(int i = 0; i < configCount; i++)
        {
            Config_WheelReward config = config_Wheels[i];
            if (config.appearLimitType != Reward.Empty && SaveManager.GetRewardValue(config.appearLimitType) >= config.appearLimitMax)
                continue;
            if (isLockGiveaways)
            {
                Reward rewardType = config.rewardType;
                switch (rewardType)
                {
                    case Reward.Macbook:
                    case Reward.SamsungPhone:
                    case Reward.SonyTV:
                    case Reward.LV_Bag:
                    case Reward.iPad:
                    case Reward.DysonHairDryer:
                    case Reward.Switch:
                    case Reward.PhilipsCoffeeMachine:
                    case Reward.GucciPerfume:
                    case Reward.ChanelLipstick:
                        continue;
                    default:
                        break;
                }
            }
            int config_index = i;
            randomList.Add(new Vector2Int(config_index, config.weight));
            totalWeight += config.weight;

            List<int> blackBox = config.blackBox;
            int blackBoxCount = blackBox.Count;
            if (blackBoxCount == 0)
                continue;
            for(int index = 0; index < blackBoxCount; index++)
            {
                if (playWheelTime == blackBox[index])
                {
                    List<Vector2Int> mutipleList = config.randomMultipleAndWeight;
                    int mutipleCount = mutipleList.Count;
                    multiple = -1;
                    if (mutipleCount == 0)
                        multiple = 1;
                    else
                    {
                        int total = 0;
                        for(int mIndex = 0; mIndex < mutipleCount; mIndex++)
                            total += mutipleList[mIndex].y;
                        int result = Random.Range(0, total);
                        total = 0;
                        for(int mIndex = 0; mIndex < mutipleCount; mIndex++)
                        {
                            total += mutipleList[mIndex].y;
                            if (result < total)
                            {
                                multiple = mutipleList[mIndex].x;
                                break;
                            }
                        }
                    }
                    if (multiple <= 0)
                        throw new System.ArgumentException("wheel reward multiple not be less than one");
                    needRandomMultiple = config.randomMultipleAndWeight.Count > 1;
                    return config_index;
                }
            }
        }
        int randomResult = Random.Range(0, totalWeight);
        totalWeight = 0;
        int randmCount = randomList.Count;
        for(int i = 0; i < randmCount; i++)
        {
            totalWeight += randomList[i].y;
            if (randomResult < totalWeight)
            {
                Config_WheelReward config = config_Wheels[randomList[i].x];
                List<Vector2Int> mutipleList = config.randomMultipleAndWeight;
                int mutipleCount = mutipleList.Count;
                multiple = -1;
                if (mutipleCount == 0)
                    multiple = 1;
                else
                {
                    int total = 0;
                    for (int mIndex = 0; mIndex < mutipleCount; mIndex++)
                        total += mutipleList[mIndex].y;
                    int result = Random.Range(0, total);
                    total = 0;
                    for (int mIndex = 0; mIndex < mutipleCount; mIndex++)
                    {
                        total += mutipleList[mIndex].y;
                        if (result < total)
                        {
                            multiple = mutipleList[mIndex].x;
                            break;
                        }
                    }
                }
                if (multiple <= 0)
                    throw new System.ArgumentException("wheel reward multiple not be less than one");
                needRandomMultiple = config.randomMultipleAndWeight.Count > 1;
                return randomList[i].x;
            }
        }
        throw new System.ArgumentException("wheel reward is error");
    }
    public static List<Vector2Int> GetWheelRewardAllocation()
    {
        List<Config_WheelReward> wheelRewards = ConfigAsset.config_WheelRewards;
        int configCount = wheelRewards.Count;
        List<Vector2Int> wheelAlloc = new List<Vector2Int>();
        for(int i = 0; i < configCount; i++)
        {
            wheelAlloc.Add(new Vector2Int((int)wheelRewards[i].rewardType, wheelRewards[i].rewardNum));
        }
        return wheelAlloc;
    }
    public static List<int> GetTaskTypeData()
    {
        List<int> willSaveData = new List<int>();
        List<Config_Task> tasks = ConfigAsset.config_Tasks;
        int taskCount = tasks.Count;
        for(int i = 0; i < taskCount; i++)
        {
            int taskTypeInteger = (int)tasks[i].taskType;
            if (!willSaveData.Contains(taskTypeInteger))
                willSaveData.Add(taskTypeInteger);
        }
        return willSaveData;
    }
    public static List<int> GetTaskAll_ID()
    {
        List<int> all = new List<int>();
        List<Config_Task> tasks = ConfigAsset.config_Tasks;
        int taskCount = tasks.Count;
        for (int i = 0; i < taskCount; i++)
        {
            if (!all.Contains(tasks[i].id))
                all.Add(tasks[i].id);
            else
                Debug.LogError("has the same task id");
        }
        return all;
    }
    public static List<string> GetTaskShowContent(out List<Data7Int> id_rewardType_rewardNum_rewardMultiple_current_target_state)
    {
        List<string> descriptions = new List<string>();
        id_rewardType_rewardNum_rewardMultiple_current_target_state = new List<Data7Int>();
        List<Config_Task> tasks = ConfigAsset.config_Tasks;
        int taskCount = tasks.Count;
        int gold = SaveManager.SaveData.gold;
        int cash = SaveManager.SaveData.cash;
        for (int i = 0; i < taskCount; i++)
        {
            Config_Task taskData = tasks[i];
            int rewardIndexAtRewardTypeList = 0;
            List<Reward> limitTypes = taskData.appearLimitTypes;
            List<int> limitMax = taskData.appearLimitMaxes;
            int limitTypeCount = limitTypes.Count;
            for(int limitIndex = 0; limitIndex < limitTypeCount; limitIndex++)
            {
                Reward limitType = limitTypes[limitIndex];
                bool overLimit = false;
                switch (limitType)
                {
                    case Reward.Gold:
                        if (gold >= limitMax[limitIndex])
                            overLimit = true;
                        break;
                    case Reward.Cash:
                        if (cash >= limitMax[limitIndex])
                            overLimit = true;
                        break;
                    case Reward.Empty:
                        break;
                    default:
                        Debug.LogError("undefine this limit type " + limitType);
                        break;
                }
                if (overLimit)
                    rewardIndexAtRewardTypeList++;
                else
                    break;
            }

            List<Reward> taskRewardList = taskData.rewardTypes;
            bool showThisTask = rewardIndexAtRewardTypeList <= taskRewardList.Count - 1;
            if (showThisTask)
            {
                descriptions.Add(taskData.name);
                Data7Int otherData = new Data7Int()
                {
                    data1 = taskData.id,
                    data2 = (int)taskRewardList[rewardIndexAtRewardTypeList],
                    data3 = taskData.rewardNums[rewardIndexAtRewardTypeList],
                    data6 = taskData.needTimes,
                };
                //确定任务进度
                List<Data2Int> taskProgress = SaveManager.SaveData.taskProgress;
                foreach(var progress in taskProgress)
                    if (progress.data1 == (int)taskData.taskType)
                    {
                        otherData.data5 = progress.data2;
                        break;
                    }
                //确定任务状态
                List<Data2Int> taskState = SaveManager.SaveData.taskState;
                foreach(var state in taskState)
                    if (state.data1 == taskData.id)
                    {
                        otherData.data7 = state.data2;
                        break;
                    }
                //确定任务奖励倍数
                if (otherData.data7 == 1)
                    otherData.data4 = 0;
                else
                {
                    Reward rewardType = (Reward)otherData.data2;
                    List<Reward> multipleReawrdTypeList = taskData.randomMultipleRewardTypes;
                    int multiplesCount = multipleReawrdTypeList.Count;
                    bool hasSetMultiple = false;
                    for(int rewardTypeIndex = 0; rewardTypeIndex < multiplesCount; rewardTypeIndex++)
                    {
                        if (multipleReawrdTypeList[rewardTypeIndex] == rewardType)
                        {
                            List<Vector2Int> multipleWeights = taskData.randomMultipleAndWeights[rewardTypeIndex].multipleWeights;
                            int weightsCount = multipleWeights.Count;
                            int total = 0;
                            for(int multipleIndex = 0; multipleIndex < weightsCount; multipleIndex++)
                                total += multipleWeights[multipleIndex].y;
                            int result = Random.Range(0, total);
                            total = 0;
                            for (int multipleIndex = 0; multipleIndex < weightsCount; multipleIndex++)
                            {
                                total += multipleWeights[multipleIndex].y;
                                if (result < total)
                                {
                                    hasSetMultiple = true;
                                    otherData.data4 = multipleWeights[multipleIndex].x;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    if (!hasSetMultiple)
                        otherData.data4 = 1;
                }
                id_rewardType_rewardNum_rewardMultiple_current_target_state.Add(otherData);
            }
        }
        return descriptions;
    }
    public static bool CheckTaskHasFinish()
    {
        bool hasFinish = false;
        List<Config_Task> tasks = ConfigAsset.config_Tasks;
        int taskCount = tasks.Count;
        int gold = SaveManager.SaveData.gold;
        int cash = SaveManager.SaveData.cash;
        for (int i = 0; i < taskCount; i++)
        {
            Config_Task taskData = tasks[i];
            int rewardIndexAtRewardTypeList = 0;
            List<Reward> limitTypes = taskData.appearLimitTypes;
            List<int> limitMax = taskData.appearLimitMaxes;
            int limitTypeCount = limitTypes.Count;
            for (int limitIndex = 0; limitIndex < limitTypeCount; limitIndex++)
            {
                Reward limitType = limitTypes[limitIndex];
                bool overLimit = false;
                switch (limitType)
                {
                    case Reward.Gold:
                        if (gold >= limitMax[limitIndex])
                            overLimit = true;
                        break;
                    case Reward.Cash:
                        if (cash >= limitMax[limitIndex])
                            overLimit = true;
                        break;
                    case Reward.Empty:
                        break;
                    default:
                        Debug.LogError("undefine this limit type " + limitType);
                        break;
                }
                if (overLimit)
                    rewardIndexAtRewardTypeList++;
                else
                    break;
            }

            List<Reward> taskRewardList = taskData.rewardTypes;
            bool showThisTask = rewardIndexAtRewardTypeList <= taskRewardList.Count - 1;
            if (showThisTask)
            {
                List<Data2Int> taskProgress = SaveManager.SaveData.taskProgress;
                foreach (var progress in taskProgress)
                    if (progress.data1 == (int)taskData.taskType)
                    {
                        if (progress.data2 >= taskData.needTimes)
                            hasFinish = true;
                        break;
                    }
                bool hasGet = false;
                List<Data2Int> taskState = SaveManager.SaveData.taskState;
                foreach (var state in taskState)
                    if (state.data1 == taskData.id)
                    {
                        hasGet = state.data2 == 1;
                        break;
                    }
                hasFinish = hasFinish && !hasGet;
                if (hasFinish)
                    return true;
            }
        }
        return hasFinish;
    }
    public static string GetRewardDescription(Reward reward)
    {
        switch (reward)
        {
            case Reward.AmazonCard:
                return "Amazon Card";
            case Reward.Macbook:
                return "Macbook Pro\n13 M1 256GB";
            case Reward.PlayBingoTicket:
                Debug.LogError("can not use this function to get playbingo ticket description");
                break;
            case Reward.SamsungPhone:
                return "Samsung\nGalaxy S20 5G";
            case Reward.SonyTV:
                return "Sony 75-inch TV";
            case Reward.LV_Bag:
                return "Louis Vuitton bag";
            case Reward.iPad:
                return "Apple iPad Pro";
            case Reward.DysonHairDryer:
                return "Dyson Supersonic";
            case Reward.Switch:
                return "Nintendo Switch";
            case Reward.PhilipsCoffeeMachine:
                return "Philips\nCoffee Machine";
            case Reward.GucciPerfume:
                return "Gucci Bloom\nPerfume 100ml";
            case Reward.ChanelLipstick:
                return "Chanel Rouge\nCoco Lipstick";
            case Reward.Key:
                return "Lucky key*1";
            default:
                Debug.LogError("can not use this function to get other description");
                break;
        }
        return string.Empty;
    }
    public static List<Config_Gift> GetGiftConfig()
    {
        List<Config_Gift> config_Gifts = new List<Config_Gift>();
        int configCount = ConfigAsset.config_Gifts.Count;
        for(int i = 0; i < configCount; i++)
        {
            config_Gifts.Add(ConfigAsset.config_Gifts[i]);
        }
        return config_Gifts;
    }
    public static List<int> GetDrawPhoneSpinRandomList()
    {
        List<int> luckySpinRewardList = new List<int>();
        int fixRewardCardIndex = Random.Range(0, LuckySpinItemCount);
        List<int> indexList = new List<int>();
        List<Config_LuckySpin> spinItemConfig = ConfigAsset.config_LuckySpins;
        int configCount = spinItemConfig.Count;
        for (int i = 0; i < configCount; i++)
            indexList.Add(i);
        for(int i = 0; i < LuckySpinItemCount; i++)
        {
            if (i == fixRewardCardIndex)
            {
                luckySpinRewardList.Add((int)Reward.PlayBingoTicket);
                continue;
            }
            int configIndex = indexList[Random.Range(0, indexList.Count)];
            indexList.Remove(configIndex);
            Reward thisRewardType = spinItemConfig[configIndex].rewardType;
            if (thisRewardType == Reward.PlayBingoTicket)
            {
                i--;
                continue;
            }
            luckySpinRewardList.Add((int)spinItemConfig[configIndex].rewardType);
        }
        return luckySpinRewardList;
    }
    public static Reward GetRandomGiftFragment()
    {
        List<Reward> rewards = new List<Reward>();
        #region 添加随机奖励
        rewards.Add(Reward.Macbook);
        rewards.Add(Reward.PlayBingoTicket);
        rewards.Add(Reward.SamsungPhone);
        rewards.Add(Reward.SonyTV);
        rewards.Add(Reward.LV_Bag);
        rewards.Add(Reward.iPad);
        rewards.Add(Reward.DysonHairDryer);
        rewards.Add(Reward.Switch);
        rewards.Add(Reward.PhilipsCoffeeMachine);
        rewards.Add(Reward.GucciPerfume);
        rewards.Add(Reward.ChanelLipstick);
        #endregion
        return rewards[Random.Range(0, rewards.Count)];
    }
    public static int GetDrawPhoneSpinItemReawrdNum(Reward reward)
    {
        if (reward == Reward.Cash)
            return GetCardRandomRewradNum(Reward.Cash);
        List<Config_LuckySpin> spinItemConfig = ConfigAsset.config_LuckySpins;
        int configCount = spinItemConfig.Count;
        for (int i = 0; i < configCount; i++)
        {
            if (spinItemConfig[i].rewardType == reward)
            {
                if (SaveManager.GetRewardValue(reward) == 0 && spinItemConfig[i].firstGetRewardNum > 0)
                    return spinItemConfig[i].firstGetRewardNum;
                else
                    return spinItemConfig[i].rewardNum;
            }
        }
        return 0;
    }
    public static Reward GetRandomLuckySpinReward(out int rewardNum,params Reward[] rewardArray)
    {
        int rewardArrayLength = rewardArray.Length;
        Vector3Int[] rewardWeights = new Vector3Int[rewardArrayLength];
        List<Config_LuckySpin> spinItemConfig = ConfigAsset.config_LuckySpins;
        int configCount = spinItemConfig.Count;
        for (int i = 0; i < configCount; i++)
        {
            Config_LuckySpin config = spinItemConfig[i];
            Reward configReward = config.rewardType;
            for(int rewardIndex = 0; rewardIndex < rewardArrayLength; rewardIndex++)
            {
                if (rewardArray[rewardIndex] == configReward)
                {
                    if (config.appearLimitType != Reward.Empty && SaveManager.GetRewardValue(config.appearLimitType) >= config.appearLimitMax)
                        rewardWeights[rewardIndex] = Vector3Int.zero;
                    else
                        rewardWeights[rewardIndex] = new Vector3Int(config.weight, 1, configReward == Reward.Cash ? GetCardRandomRewradNum(Reward.Cash) : config.rewardNum);
                }
            }
        }
        int total = 0;
        for(int i = 0; i < rewardArrayLength; i++)
        {
            if (rewardWeights[i].y == 1)
                total += rewardWeights[i].x;
        }
        int result = Random.Range(0, total);
        total = 0;
        for(int i = 0; i < rewardArrayLength; i++)
        {
            if (rewardWeights[i].y == 1)
            {
                total += rewardWeights[i].x;
                if (result < total)
                {
                    rewardNum = rewardWeights[i].z;
                    return rewardArray[i];
                }
            }
        }
        Debug.LogError("error lucky spin reward");
        rewardNum = 0;
        return Reward.Empty;
    }
    public static int GetCurrentSpinNeedBingoTime()
    {
        int spinTime = SaveManager.SaveData.luckySpinTimes;
        if (spinTime > spinNeedBingoTime.Length - 1)
            return spinNeedBingoTime[spinNeedBingoTime.Length - 1];
        else
            return spinNeedBingoTime[spinTime];
    }
    public static int GetRandomMoreBallNum()
    {
        List<Config_MoreBall> config_MoreBalls = ConfigAsset.config_MoreBalls;
        int configCount = config_MoreBalls.Count;
        int getMoreBallTimes = SaveManager.SaveData.totalGetMoreBallTimes + 1;
        int total = 0;
        for(int i = 0; i < configCount; i++)
        {
            Config_MoreBall data = config_MoreBalls[i];
            total += data.weight;
            if (getMoreBallTimes == data.blackBox)
                return data.ballCount;
        }
        int result = Random.Range(0, total);
        total = 0;
        for(int i = 0; i < configCount; i++)
        {
            Config_MoreBall data = config_MoreBalls[i];
            total += data.weight;
            if (result < total)
                return data.ballCount;
        }
        Debug.LogError("can not find more ball data");
        return 0;
    }
    public static int GetMoreBallUsefulCount(int ballCount)
    {
        List<Config_MoreBall> config_MoreBalls = ConfigAsset.config_MoreBalls;
        int configCount = config_MoreBalls.Count;
        for (int i = 0; i < configCount; i++)
        {
            if (ballCount == config_MoreBalls[i].ballCount)
                return i;
        }
        Debug.LogError("error ball count");
        return 0;
    }
    public static Reward GetRandomGiftBallonReward(out int rewardNum)
    {
        List<Config_GiftBallon> config_GiftBallons = ConfigAsset.config_GiftBallons;
        int configCount = config_GiftBallons.Count;
        int total = 0;
        List<int> canRandomList = new List<int>();
        for(int i = 0; i < configCount; i++)
        {
            Config_GiftBallon giftBallon = config_GiftBallons[i];
            if (giftBallon.appearLimitType == Reward.Empty || SaveManager.GetRewardValue(giftBallon.appearLimitType) < giftBallon.appearLimitMax)
            {
                canRandomList.Add(i);
                total += giftBallon.weight;
            }
        }
        int result = Random.Range(0, total);
        total = 0;
        int canSelectCount = canRandomList.Count;
        for(int i = 0; i < canSelectCount; i++)
        {
            Config_GiftBallon giftBallon = config_GiftBallons[canRandomList[i]];
            total += giftBallon.weight;
            if (result < total)
            {
                rewardNum = giftBallon.rewardNum;
                return giftBallon.rewardType;
            }
        }
        Debug.LogError("gift ballon reward error");
        rewardNum = 0;
        return Reward.Empty;
    }
    public static bool CheckWetherCanFlyGiftBallon()
    {
        List<Config_GiftBallon> config_GiftBallons = ConfigAsset.config_GiftBallons;
        int configCount = config_GiftBallons.Count;
        for(int i = 0; i < configCount; i++)
        {
            Config_GiftBallon giftBallon = config_GiftBallons[i];
            if (giftBallon.appearLimitType == Reward.Empty || SaveManager.GetRewardValue(giftBallon.appearLimitType) < giftBallon.appearLimitMax)
                return true;
        }
        return false;
    }
    public static Reward GetRandomdDrawBoxReward(out int rewardNum,int openBoxTimeThisPanel)
    {
        openBoxTimeThisPanel++;
        bool isLockGiveaways = SaveManager.SaveData.islockGiveaways;
        //int openBoxTime = SaveManager.SaveData.totalOpenBoxTimes + 1;
        List<Config_DrawBox> config_DrawBoxes = ConfigAsset.config_DrawBoxes;
        int configCount = config_DrawBoxes.Count;
        List<Vector3Int> randomList = new List<Vector3Int>();//weight,reward,num
        int total = 0;
        for(int i = 0; i < configCount; i++)
        {
            Config_DrawBox drawBox = config_DrawBoxes[i];
            if (isLockGiveaways)
            {
                Reward rewardType = drawBox.rewardType;
                switch (rewardType)
                {
                    case Reward.Macbook:
                    case Reward.SamsungPhone:
                    case Reward.SonyTV:
                    case Reward.LV_Bag:
                    case Reward.iPad:
                    case Reward.DysonHairDryer:
                    case Reward.Switch:
                    case Reward.PhilipsCoffeeMachine:
                    case Reward.GucciPerfume:
                    case Reward.ChanelLipstick:
                        continue;
                    default:
                        break;
                }
            }
            List<int> blackBox = drawBox.blackBox;
            bool overRange = drawBox.appearLimitType != Reward.Empty && SaveManager.GetRewardValue(drawBox.appearLimitType) >= drawBox.appearLimitMax;
            if (overRange)
                continue;
            foreach(var time in blackBox)
                if (openBoxTimeThisPanel == time)
                {
                    rewardNum = drawBox.rewardNum;
                    return drawBox.rewardType;
                }
            total += drawBox.weight;
            randomList.Add(new Vector3Int(drawBox.weight, (int)drawBox.rewardType, drawBox.rewardNum));
        }
        int result = Random.Range(0, total);
        total = 0;
        int randomCount = randomList.Count;
        for(int i = 0; i < randomCount; i++)
        {
            total += randomList[i].x;
            if (result < total)
            {
                rewardNum = randomList[i].z;
                return (Reward)randomList[i].y;
            }
        }
        Debug.LogError("random box reward error");
        rewardNum = 0;
        return Reward.Empty;
    }
    public static int GetRandomPigCash(int totalOffset)
    {
        int totalCash = SaveManager.SaveData.total_pigCash;
        totalCash += totalOffset;
        List<Config_Pig> config_Pigs = ConfigAsset.config_Pigs;
        int configCount = config_Pigs.Count;
        for(int i = 0; i < configCount; i++)
        {
            if (totalCash <= config_Pigs[i].maxTotalPigCash)
                return config_Pigs[i].rewardPigCashNum;
        }
        return 0;
    }
    public static bool CheckCanShowPig()
    {
        int totalCash = SaveManager.SaveData.total_pigCash;
        return totalCash <= ConfigAsset.config_Pigs[ConfigAsset.config_Pigs.Count - 1].maxTotalPigCash;
    }
}
