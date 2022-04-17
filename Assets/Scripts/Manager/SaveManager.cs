using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager
{
    private static SaveData saveData;
    public SaveManager()
    {
        if (!saveData.isNotNull)
        {
            string dataString = PlayerPrefs.GetString("bingoData", "");
            if (string.IsNullOrEmpty(dataString))
                saveData = new SaveData()
                {
                    isNotNull = true,
                    isPackB = false,
                    cash = 0,
                    gold = 0,
                    amazonCard = 0,
                    phoneFragment = 0,
                    macFragment = 0,
                    SonyTV_Fragment = 0,
                    LV_BagFragment = 0,
                    iPadFragment = 0,
                    DysonHairDryerFragment = 0,
                    SwitchFragment = 0,
                    PhilipsCoffeeMachineFragment = 0,
                    GucciPerfumeFragment = 0,
                    ChanelLipstickFragment = 0,
                    playWheelTimes = 0,
                    sound_on = true,
                    music_on = true,
                    cardNum = ConfigManager.OriginBingoCardNum,
                    lastGetCardTime = System.DateTime.Now,
                    totalBingoNum = 0,
                    totalCardNum = 0,
                    lastLoginTime = System.DateTime.Now,
                    taskProgress = null,
                    taskState = null,
                    spinItemState = null,
                    bingoForLuckySpin_Times = 0,
                    luckySpinTimes = 0,
                    uuid = string.Empty,
                    adid = string.Empty,
                    totdayGetMoreCardsTimes = 0,
                    hasGetNewPlayerReward = false,
                    newPlayerCash = 0,
                    guideStep = 0,
                    inviteCode = string.Empty,
                    todayGetMoreBallTimes = 0,
                    totalGetMoreBallTimes = 0,
                    totalOpenBoxTimes = 0,
                    key = 0,
                    totalAdTimes = 0,
                    activeDay = 0,
                    hasUnlockCashout = false,
                    cash_moneyBox = 0,
                    total_pigCash = 0,
                    hasClickCashToday = false,
                    islockPig = true,
                    islockTask = true,
                    completeGameTimes = 0,
                    islockGiveaways = true,
                    isFirstEnterDrawPhone = true,
                    getMoreKeysTimes = -2,
                };
            else
                saveData = JsonMapper.ToObject<SaveData>(dataString);
            if (saveData.hasFirstGetExtraReward == null || saveData.hasFirstGetExtraReward.Length < (int)ExtraReward.Empty)
            {
                saveData.hasFirstGetExtraReward = new bool[(int)ExtraReward.Empty];
                int count = (int)ExtraReward.Empty;
                for (int i = 0; i < count; i++)
                    saveData.hasFirstGetExtraReward[i] = false;
            }
            System.DateTime now = System.DateTime.Now;
            #region 计算离线时间
            int totalOfflineSecond = (int)(now - saveData.lastGetCardTime).TotalSeconds;
            if (saveData.cardNum >= ConfigManager.MaxBingoCardNum)
                saveData.lastGetCardTime = System.DateTime.Now;
            else
            {
                saveData.cardNum += totalOfflineSecond / ConfigManager.NextBingoCardNeedSeconds;
                saveData.cardNum = Mathf.Clamp(saveData.cardNum, 0, ConfigManager.MaxBingoCardNum);
                saveData.lastGetCardTime = System.DateTime.Now.AddSeconds(-totalOfflineSecond % ConfigManager.NextBingoCardNeedSeconds);
            }
            #endregion
            bool isTomorrow = CheckTomorrow(saveData.lastLoginTime, now);
            if (isTomorrow)
                saveData.totdayGetMoreCardsTimes = 0;
            #region 更正任务进度
            List<int> taskTypeList = ConfigManager.GetTaskTypeData();
            int taskTypeCount = taskTypeList.Count;
            if (saveData.taskProgress == null)
            {
                saveData.taskProgress = new List<Data2Int>();
                for (int i = 0; i < taskTypeCount; i++)
                {
                    int typeIndex = taskTypeList[i];
                    saveData.taskProgress.Add(new Data2Int(typeIndex, 0));
                }
            }
            else
            {
                List<Data2Int> oldData = saveData.taskProgress;
                List<Data2Int> newData = new List<Data2Int>();
                for (int i = 0; i < taskTypeCount; i++)
                {
                    Data2Int newTaskData = new Data2Int() { data1 = taskTypeList[i], data2 = 0 };
                    if (!isTomorrow)
                        foreach (var old in oldData)
                            if (old.data1 == taskTypeList[i])
                            {
                                newTaskData.data2 = old.data2;
                                break;
                            }
                    newData.Add(newTaskData);
                }
                saveData.taskProgress = newData;
            }
            #endregion
            #region 更正任务状态
            List<int> all_id = ConfigManager.GetTaskAll_ID();
            int idCount = all_id.Count;
            if (saveData.taskState == null)
            {
                saveData.taskState = new List<Data2Int>();
                for (int i = 0; i < idCount; i++)
                {
                    saveData.taskState.Add(new Data2Int(all_id[i], 0));
                }
            }
            else
            {
                List<Data2Int> oldData = saveData.taskState;
                List<Data2Int> newData = new List<Data2Int>();
                for (int i = 0; i < idCount; i++)
                {
                    Data2Int newTaskState = new Data2Int() { data1 = all_id[i], data2 = 0 };
                    if (!isTomorrow)
                        foreach (var old in oldData)
                            if (old.data1 == all_id[i])
                            {
                                newTaskState.data2 = old.data2;
                                break;
                            }
                    newData.Add(newTaskState);
                }
                saveData.taskState = newData;
            }
            #endregion
            #region 更正抽奖列表
            if (saveData.spinItemState == null || saveData.spinItemState.Count != ConfigManager.LuckySpinItemCount)
            {
                List<int> itemRewardOrderList = ConfigManager.GetDrawPhoneSpinRandomList();
                saveData.spinItemState = new List<Data2Int>();
                for (int i = 0; i < ConfigManager.LuckySpinItemCount; i++)
                    saveData.spinItemState.Add(new Data2Int(itemRewardOrderList[i], 0));
            }
            #endregion
            if (isTomorrow)
            {
                saveData.todayGetMoreBallTimes = 0;
                saveData.activeDay++;
            }
            if (saveData.activeDay == 0)
                saveData.activeDay = 1;
            if (isTomorrow)
                saveData.hasClickCashToday = false;
            saveData.lastLoginTime = now;
            Save();
        }
    }
    public static SaveData SaveData { 
        get
        {
            SaveData result = saveData;
            return result;
        }
        set
        {
            saveData = value;
        }
    }
    public static void Save()
    {
        PlayerPrefs.SetString("bingoData", JsonMapper.ToJson(saveData));
    }
    public static int GetRewardValue(Reward reward)
    {
        switch (reward)
        {
            case Reward.ManyGold:
            case Reward.Gold:
                return SaveData.gold;
            case Reward.ManyCash:
            case Reward.Cash:
                return SaveData.cash;
            case Reward.AmazonCard:
                return SaveData.amazonCard;
            case Reward.Macbook:
                return SaveData.macFragment;
            case Reward.PlayBingoTicket:
                return SaveData.cardNum;
            case Reward.SamsungPhone:
                return SaveData.phoneFragment;
            case Reward.SonyTV:
                return SaveData.SonyTV_Fragment;
            case Reward.LV_Bag:
                return SaveData.LV_BagFragment;
            case Reward.iPad:
                return SaveData.iPadFragment;
            case Reward.DysonHairDryer:
                return SaveData.DysonHairDryerFragment;
            case Reward.Switch:
                return SaveData.SwitchFragment;
            case Reward.PhilipsCoffeeMachine:
                return SaveData.PhilipsCoffeeMachineFragment;
            case Reward.GucciPerfume:
                return SaveData.GucciPerfumeFragment;
            case Reward.ChanelLipstick:
                return SaveData.ChanelLipstickFragment;
            default:
                return 0;
        }
    }
    public static int GetShowCashValue()
    {
        return SaveData.cash + SaveData.newPlayerCash;
    }
    private static bool CheckTomorrow(System.DateTime oldDate,System.DateTime now)
    {
        bool isTomorrow = false;
        if (oldDate.Year == now.Year)
        {
            if (oldDate.Month == now.Month)
            {
                if (oldDate.Day < now.Day)
                    isTomorrow = true;
            }
            else if (oldDate.Month < now.Month)
                isTomorrow = true;
        }
        else if (oldDate.Year < now.Year)
            isTomorrow = true;
        return isTomorrow;
    }
}
public struct SaveData
{
    public bool isNotNull;
    public bool isPackB;
    public int cash;
    public int gold;
    public int amazonCard;
    //public int playBingoTicket;  -->cardNum
    #region 碎片
    public int phoneFragment;//SamsungPhone
    public int macFragment;//Macbook
    public int SonyTV_Fragment;
    public int LV_BagFragment;
    public int iPadFragment;
    public int DysonHairDryerFragment;
    public int SwitchFragment;
    public int PhilipsCoffeeMachineFragment;
    public int GucciPerfumeFragment;
    public int ChanelLipstickFragment;
    #endregion
    public int playWheelTimes;
    public bool[] hasFirstGetExtraReward;
    public bool sound_on;
    public bool music_on;
    public int cardNum;
    public System.DateTime lastGetCardTime;
    public int totalCardNum;
    public int totalBingoNum;
    public System.DateTime lastLoginTime;
    public List<Data2Int> taskProgress;
    public List<Data2Int> taskState;
    public List<Data2Int> spinItemState;
    public int bingoForLuckySpin_Times;
    public int luckySpinTimes;
    public string uuid;
    public string adid;
    public AllData_FriendData friendData;
    public int totdayGetMoreCardsTimes;
    public bool hasGetNewPlayerReward;
    public int newPlayerCash;//pig cash , only show
    public int guideStep;
    public string inviteCode;
    public int todayGetMoreBallTimes;
    public int totalGetMoreBallTimes;
    public int totalOpenBoxTimes;
    public int key;
    public int totalAdTimes;
    public int activeDay;
    public bool hasUnlockCashout;
    public int cash_moneyBox;
    public int total_pigCash;
    public int pig_totalAdTimes;
    public int pig_totalPlayCard;
    public bool hasClickCashToday;
    public bool islockPig;
    public bool islockTask;
    public int completeGameTimes;
    public bool islockGiveaways;
    public bool isFirstEnterDrawPhone;
    public int getMoreKeysTimes;
}
