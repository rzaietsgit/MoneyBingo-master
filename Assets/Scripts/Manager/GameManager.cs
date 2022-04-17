using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using ITSoft;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public RectTransform bgRect;
    public static bool IsBigScreen = false;
    public static float ExpandCoe = 1;
    public const float TopMoveDownOffset = 100;
    public const string PackageName = "com.MoneyBingo.LuckyRewards.CashGame.BigWinner";
    public const int BundleID = 25;
    public const string AppleId = "";
    public static bool WillSetPackB = false;
    public static bool isLoadingEnd = false;
    public Transform menuRoot;
    public Transform baseRoot;
    public Transform popRoot;
    public GameObject audioRoot;
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        SaveManager saveManager = new SaveManager();
        UIManager ui = new UIManager(menuRoot, baseRoot, popRoot);
        float coe = (float)Screen.height / Screen.width;
        float originCoe = 16f / 9;
        ExpandCoe = coe > originCoe ? coe / originCoe : originCoe / coe;
        IsBigScreen = ExpandCoe != 1;
        bgRect.sizeDelta *= ExpandCoe;
        AudioManager audioManager = new AudioManager(audioRoot);
        StartCoroutine("NatureAddCardNum");
        UIManager.ShowPanel(PopPanel.LoadingPanel);
    }
    public void SetIsPackB()
    {
        if (!SaveManager.SaveData.isPackB)
        {
            SaveData saveData = SaveManager.SaveData;
            saveData.isPackB = true;
            SaveManager.SaveData = saveData;
            SendAdjustSetPackBEvent();
        }
    }
    public void OnLoadingEnd()
    {
        UIManager.ShowPanel(BasePanel.Menu);
        UIManager.ShowPanel(BasePanel.MainPanel);
        if (WillSetPackB)
        {
            SetIsPackB();
        }
    }
    public void AddReward(Reward reward, int num)
    {
        SaveData saveData = SaveManager.SaveData;
        switch (reward)
        {
            case Reward.ManyGold:
            case Reward.Gold:
                saveData.gold += num;
                break;
            case Reward.ManyCash:
            case Reward.Cash:
                saveData.cash += num;
                break;
            case Reward.AmazonCard:
                saveData.amazonCard += num;
                break;
            case Reward.Macbook:
                saveData.macFragment += num;
                break;
            case Reward.PlayBingoTicket:
                Debug.LogError("Can not use this function to add playbingo ticket , use [ChangeCardNum].");
                return;
            case Reward.SamsungPhone:
                saveData.phoneFragment += num;
                break;
            case Reward.SonyTV:
                saveData.SonyTV_Fragment += num;
                break;
            case Reward.LV_Bag:
                saveData.LV_BagFragment += num;
                break;
            case Reward.iPad:
                saveData.iPadFragment += num;
                break;
            case Reward.DysonHairDryer:
                saveData.DysonHairDryerFragment += num;
                break;
            case Reward.Switch:
                saveData.SwitchFragment += num;
                break;
            case Reward.PhilipsCoffeeMachine:
                saveData.PhilipsCoffeeMachineFragment += num;
                break;
            case Reward.GucciPerfume:
                saveData.GucciPerfumeFragment += num;
                break;
            case Reward.ChanelLipstick:
                saveData.ChanelLipstickFragment += num;
                break;
            case Reward.Key:
                saveData.key += num;
                saveData.key = Mathf.Clamp(saveData.key, 0, 3);
                break;
        }
        SaveManager.SaveData = saveData;
        UIManager.SendMessageToPanel(BasePanel.System, BasePanel.Menu, (int)reward, SaveManager.GetRewardValue(reward));
        if (reward == Reward.Key && num < 0)
            UIManager.SendMessageToPanel(BasePanel.System, BasePanel.GamePanel);
    }
    public void AddShowedPigCash(int value)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.newPlayerCash += value;
        SaveManager.SaveData = saveData;
        UIManager.SendMessageToPanel(BasePanel.System, BasePanel.Menu, (int)Reward.Cash);
    }
    public void SetHasGetNewPlayerReward()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.hasGetNewPlayerReward = true;
        SaveManager.SaveData = saveData;
    }
    public void AddPlayWheelTime(int time=1)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.playWheelTimes += time;
        saveData.totalBingoNum += time;
        saveData.bingoForLuckySpin_Times += time;
        SaveManager.SaveData = saveData;
    }
    public void AddTotalOpenBoxTime(int time = 1)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.totalOpenBoxTimes += time;
        SaveManager.SaveData = saveData;
    }
    public void SetHasClickCashToday()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.hasClickCashToday = true;
        SaveManager.SaveData = saveData;
    }
    public void ChangeCardNum(int changeValue,BasePanel fromBasePanel)
    {
        ChangeCardNum(changeValue, (int)fromBasePanel);
    }
    public void ChangeCardNum(int changeValue, PopPanel fromPopPanel)
    {
        ChangeCardNum(changeValue, (int)fromPopPanel);
    }
    private void ChangeCardNum(int changeValue,int sourcePanelIndex)
    {
        SaveData saveData = SaveManager.SaveData;
        bool needStartTimeDown = saveData.cardNum >= ConfigManager.MaxBingoCardNum;
        saveData.cardNum += changeValue;
        if (sourcePanelIndex == (int)BasePanel.System || needStartTimeDown)
            saveData.lastGetCardTime = System.DateTime.Now;
        SaveManager.SaveData = saveData;
        if (saveData.cardNum >= ConfigManager.MaxBingoCardNum)
            StopCoroutine("NatureAddCardNum");
        else if (needStartTimeDown)
            StartCoroutine("NatureAddCardNum");
        UIManager.SendMessageToPanel(sourcePanelIndex, (int)BasePanel.MainPanel, saveData.cardNum);
    }
    public bool ClickMusicSetting()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.music_on = !saveData.music_on;
        SaveManager.SaveData = saveData;
        return saveData.music_on;
    }
    public bool ClickSoundSetting()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.sound_on = !saveData.sound_on;
        SaveManager.SaveData = saveData;
        return saveData.sound_on;
    }
    public void TriggerTask(TaskType taskType, int num)
    {
        SaveData saveData = SaveManager.SaveData;
        List<Data2Int> taskProgress = saveData.taskProgress;
        int typeInteger = (int)taskType;
        int typeCount = taskProgress.Count;
        for(int i =0; i < typeCount; i++)
        {
            if (taskProgress[i].data1 == typeInteger)
            {
                taskProgress[i] = new Data2Int(typeInteger, taskProgress[i].data2 + num);
                break;
            }
        }
        saveData.taskProgress = taskProgress;
        SaveManager.SaveData = saveData;
    }
    public void FinishTask(int taskId, Reward reward)
    {
        SaveData saveData = SaveManager.SaveData;
        List<Data2Int> taskStates = saveData.taskState;
        int idCount = taskStates.Count;
        for(int i = 0; i < idCount; i++)
        {
            if (taskStates[i].data1 == taskId)
            {
                taskStates[i] = new Data2Int(taskId, 1);
                break;
            }
        }
        saveData.taskState = taskStates;
        SaveManager.SaveData = saveData;
        SendAdjustFinishTaskEvent(taskId, reward);
    }
    public void ReduceBingoTimeForSpinAfterSpin()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.bingoForLuckySpin_Times -= ConfigManager.GetCurrentSpinNeedBingoTime();
        saveData.luckySpinTimes++;
        SaveManager.SaveData = saveData;
    }
    public void RefreshLuckySpinReawrd()
    {
        SaveData saveData = SaveManager.SaveData;
        List<int> itemRewardOrderList = ConfigManager.GetDrawPhoneSpinRandomList();
        saveData.spinItemState = new List<Data2Int>();
        for (int i = 0; i < ConfigManager.LuckySpinItemCount; i++)
            saveData.spinItemState.Add(new Data2Int(itemRewardOrderList[i], 0));
        SaveManager.SaveData = saveData;
    }
    public void SaveGetSpinRewardState(int index)
    {
        SaveData saveData = SaveManager.SaveData;
        if (saveData.spinItemState[index].data1 != (int)Reward.PlayBingoTicket)
            saveData.spinItemState[index] = new Data2Int(saveData.spinItemState[index].data1, 1);
        SaveManager.SaveData = saveData;
    }
    public void ChangeUUID(string uuid)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.uuid = uuid;
        SaveManager.SaveData = saveData;
    }
    public void ChangeADID(string adid)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.adid = adid;
        SaveManager.SaveData = saveData;
    }
    public void SaveFriendData(AllData_FriendData friendData,string inviteCode,string uuid)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.friendData = friendData;
        saveData.inviteCode = inviteCode;
        saveData.uuid = uuid;
        SaveManager.SaveData = saveData;
    }
    public void SaveInviteReward(double totalPt,int currentReceiveTime)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.friendData.reward_conf.invite_receive = currentReceiveTime;
        saveData.friendData.user_total = totalPt;
        SaveManager.SaveData = saveData;
    }
    public void AddTodayGetMoreBingoCardsChance()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.totdayGetMoreCardsTimes++;
        SaveManager.SaveData = saveData;
    }
    public void AddGuideStep()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.guideStep++;
        SaveManager.SaveData = saveData;
    }
    public void AddGetMoreBallTimes(int value = 1)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.todayGetMoreBallTimes += value;
        saveData.totalGetMoreBallTimes += value;
        SaveManager.SaveData = saveData;
    }
    public void AddTotalPlayCardNum(int value)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.totalCardNum += value;
        if (saveData.cash_moneyBox >= ConfigManager.MoneyBoxCashTargetExchangeNeedNum)
            saveData.pig_totalPlayCard += value;
        SaveManager.SaveData = saveData;
    }
    public void AddTotalAdTimes(int value = 1)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.totalAdTimes += value;
        if (saveData.cash_moneyBox >= ConfigManager.MoneyBoxCashTargetExchangeNeedNum)
            saveData.pig_totalAdTimes += value;
        SaveManager.SaveData = saveData;
    }
    public void UnLockCashout()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.hasUnlockCashout = true;
        SaveManager.SaveData = saveData;
    }
    public void AddPigCash(int value)
    {
        SaveData saveData = SaveManager.SaveData;
        int oldNum = saveData.cash_moneyBox;
        if (saveData.cash_moneyBox + value <= ConfigManager.MoneyBoxCashTargetExchangeNeedNum)
            saveData.cash_moneyBox += value;
        else
            saveData.cash_moneyBox = ConfigManager.MoneyBoxCashTargetExchangeNeedNum;
        int offset = saveData.cash_moneyBox - oldNum;
        if (offset > 0)
            saveData.total_pigCash += offset;
        SaveManager.SaveData = saveData;
        UIManager.SendMessageToPanel(BasePanel.System, BasePanel.Menu, (int)Reward.Cash_MoneyBox);
    }
    public void ResetPigCashTask()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.pig_totalAdTimes = 0;
        saveData.pig_totalPlayCard = 0;
        SaveManager.SaveData = saveData;
    }
    public void AddCompleteGameTime(int time = 1)
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.completeGameTimes += time;
        SaveManager.SaveData = saveData;
    }
    public void UnLockPig()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.islockPig = false;
        SaveManager.SaveData = saveData;
    }
    public void UnLockTask()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.islockTask = false;
        SaveManager.SaveData = saveData;
    }
    public void UnLockGiveaways()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.islockGiveaways = false;
        SaveManager.SaveData = saveData;
    }
    public void SetHasFirstEnterDrawPhone()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.isFirstEnterDrawPhone = false;
        SaveManager.SaveData = saveData;
    }
    public void AddGetMorekeysTimes()
    {
        SaveData saveData = SaveManager.SaveData;
        saveData.getMoreKeysTimes++;
        SaveManager.SaveData = saveData;
    }
    private IEnumerator NatureAddCardNum()
    {
        int currentTime = (int)(System.DateTime.Now - SaveManager.SaveData.lastGetCardTime).TotalSeconds;
        WaitForSeconds oneSecond = new WaitForSeconds(1);
        while (true)
        {
            if (SaveManager.SaveData.cardNum < ConfigManager.MaxBingoCardNum)
            {
                while (currentTime >= ConfigManager.NextBingoCardNeedSeconds)
                {
                    currentTime -=ConfigManager.NextBingoCardNeedSeconds;
                    ChangeCardNum(+1, BasePanel.MainPanel);
                    if (SaveManager.SaveData.cardNum >= ConfigManager.MaxBingoCardNum)
                        break;
                }
            }
            else
                yield break;
            yield return oneSecond;
            currentTime++;
            UIManager.SendMessageToPanel(BasePanel.System, BasePanel.MainPanel, ConfigManager.NextBingoCardNeedSeconds - currentTime);
        }
    }
    public GameObject tipGo;
    public Text tipText;
    public void ShowTip(string content, float hideDelayTime = 1)
    {
        tipText.text = content;
        tipGo.SetActive(true);
        StopCoroutine("DelayHideTip");
        StartCoroutine("DelayHideTip", hideDelayTime);
    }
    private IEnumerator DelayHideTip(float time)
    {
        yield return new WaitForSeconds(time);
        tipGo.SetActive(false);
    }
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            SaveManager.Save();
    }
    private void OnApplicationQuit()
    {
        SaveManager.Save();
    }
    public void SendAdjustGameStartEvent()
    {
        AnalyticsManager.Log("start_game");
    }
    public void SendAdjustPlayAdEvent(bool hasAd, bool isRewardAd, string adByWay)
    {
        
    }
    public void SendAdjustGameEndEvent(bool gameOver,int currentCardNum)
    {
        AnalyticsManager.Log("end_game");
    }
    public void SendAdjustSetPackBEvent()
    {
    }
    public void SendAdjustDrawRewardsEvent(Reward reward,bool get)
    {
        string type;
        switch (reward)
        {
            case Reward.Cash:
                type = "0";
                break;
            case Reward.PlayBingoTicket:
                type = "2";
                break;
            default:
                type = "1";
                break;
        }
    }
    public void SendAdjustFinishTaskEvent(int task_id,Reward reward)
    {
        string type = "-1";
        if (reward == Reward.Cash)
            type = "0";
        else if (reward == Reward.Gold)
            type = "1";
    }
    public void SendAdjustFBDeepLink(string uri)
    {
    }
}
public enum Reward
{
    Empty,
    Gold,
    Cash,
    ManyGold,//道具翻倍用
    ManyCash,//道具翻倍用
    AmazonCard,//转盘用
    Macbook,
    PlayBingoTicket,
    SamsungPhone,
    SonyTV,
    LV_Bag,
    iPad,
    DysonHairDryer,
    Switch,
    PhilipsCoffeeMachine,
    GucciPerfume,
    ChanelLipstick,
    Pt,
    Key,
    Cash_MoneyBox,
    Num,
    Ball,
}
public enum BingoLockType
{
    CashLock,
    GoldLock,
    StarLock,
    KeyLock,
}
public enum ExtraReward
{
    ExtraGold,
    TribleReward,
    Star,
    Empty,
}
public enum TaskType
{
    PlayGame,
    PlayWheel,
    PlayLuckySpin,
}
[System.Serializable]
public struct Data7Int
{
    public int data1;
    public int data2;
    public int data3;
    public int data4;
    public int data5;
    public int data6;
    public int data7;
}
[System.Serializable]
public struct Data2Int
{
    public Data2Int(int data1,int data2)
    {
        this.data1 = data1;
        this.data2 = data2;
    }
    public int data1;
    public int data2;
}
