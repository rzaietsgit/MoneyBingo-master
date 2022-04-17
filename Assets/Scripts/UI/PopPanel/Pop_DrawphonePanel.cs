using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_DrawphonePanel : MonoBehaviour, IUIMessage
{
    public Button backButton;
    public Button helpButton;
    public GiftItem single_giftItem;
    private readonly List<GiftItem> allGifts = new List<GiftItem>();
    private readonly List<Reward> allGiftRewardTypes = new List<Reward>();
    public SpinItem single_spinItem;
    public List<SpinItem> allSpinItems = new List<SpinItem>();
    private readonly List<Reward> allSpinItemRewardTypes = new List<Reward>();
    private readonly List<int> allSpinItemUnClaimIndex = new List<int>();
    public Image progress_fillImage;
    public Text progressText;
    public Button refreshButton;
    public Button spinButton;
    private void Awake()
    {
        backButton.AddClickEvent(OnBackButtonClick);
        helpButton.AddClickEvent(OnHelpButtonClick);
        refreshButton.AddClickEvent(OnRefreshButtonClick);
        spinButton.AddClickEvent(OnSpinButtonClick);

        if (GameManager.IsBigScreen)
        {
            backButton.transform.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
            helpButton.transform.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
        }

        allGifts.Add(single_giftItem);
        List<Config_Gift> config_Gifts = ConfigManager.GetGiftConfig();
        giftCount = config_Gifts.Count;
        for(int i = 0; i < giftCount; i++)
        {
            Config_Gift giftData = config_Gifts[i];
            if (i > allGifts.Count - 1)
                allGifts.Add(Instantiate(single_giftItem.gameObject, single_giftItem.transform.parent).GetComponent<GiftItem>());
            allGifts[i].Init(giftData.giftType, giftData.redeemNeedNum);
            if (i > allGiftRewardTypes.Count - 1)
                allGiftRewardTypes.Add(giftData.giftType);
            else
                allGiftRewardTypes[i] = giftData.giftType;
        }
        RefreshSpinItem();
    }
    private void RefreshSpinItem()
    {
        List<Data2Int> spinItemData = SaveManager.SaveData.spinItemState;
        allSpinItemUnClaimIndex.Clear();
        for (int i = 0; i < ConfigManager.LuckySpinItemCount; i++)
        {
            Reward rewardType = (Reward)spinItemData[i].data1;
            bool hasClaim = spinItemData[i].data2 == 1;
            allSpinItems[i].Init(rewardType, hasClaim);
            if (i > allSpinItemRewardTypes.Count - 1)
                allSpinItemRewardTypes.Add(rewardType);
            else
                allSpinItemRewardTypes[i] = rewardType;
        }
    }
    private void OnBackButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnHelpButtonClick()
    {
        UIManager.ShowPanel(PopPanel.DrawPhoneHelpPanel);
    }
    private void OnRefreshButtonClick()
    {
        AdsManager.ShowRewarded(OnRefreshAdCallback);
    }
    private void OnRefreshAdCallback()
    {
        GameManager.Instance.RefreshLuckySpinReawrd();
        RefreshSpinItem();
    }
    private void OnSpinButtonClick()
    {
        if (isSpin)
            return;
        AdsManager.ShowRewarded(OnSpinAdCallback);
    }
    private void OnSpinAdCallback()
    {
        isSpin = true;
        GameManager.Instance.ReduceBingoTimeForSpinAfterSpin();
        RefreshSpinState();
        List<Reward> canGetReward = new List<Reward>();
        List<Data2Int> rewardState = SaveManager.SaveData.spinItemState;
        allSpinItemUnClaimIndex.Clear();
        for (int i = 0; i < ConfigManager.LuckySpinItemCount; i++)
        {
            if (rewardState[i].data2 == 0)
            {
                canGetReward.Add(allSpinItemRewardTypes[i]);
                allSpinItemUnClaimIndex.Add(i);
            }
        }
        Reward resultReward = ConfigManager.GetRandomLuckySpinReward(out int rewardNum, canGetReward.ToArray());
        StartCoroutine(AutoStartSpin(resultReward, rewardNum));
    }
    bool isSpin = false;
    int currentIndex = 0;
    private IEnumerator AutoStartSpin(Reward reward,int rewardNum)
    {
        int endIndex = -1;
        for (int i = 0; i < ConfigManager.LuckySpinItemCount; i++)
            if (allSpinItemRewardTypes[i] == reward)
            {
                endIndex = i;
                break;
            }
        if (endIndex < 0)
        {
            Debug.LogError("unknow reward spin "+reward);
            yield break;
        }
        int unClaimStartIndex;
        int unClaimEndIndex = 0;
        int unClaimLength = allSpinItemUnClaimIndex.Count;
        int minOffset = 99;
        int minIndex = -1;
        for(int i = 0; i < unClaimLength; i++)
        {
            int offset = Mathf.Abs(allSpinItemUnClaimIndex[i] - currentIndex);
            if (offset <= minOffset)
            {
                minOffset = offset;
                minIndex = i;
            }
            if (allSpinItemUnClaimIndex[i] == endIndex)
                unClaimEndIndex = i;
        }
        if (minIndex != -1)
        {
            unClaimStartIndex = minIndex;
            currentIndex = allSpinItemUnClaimIndex[minIndex];
        }
        else
        {
            Debug.LogError("no nearest index");
            yield break;
        }
        int turn = 3;
        float normalIntervalTime = 0.05f;
        int speedDownStep = 4;
        int speedDownBeforeEndIndex = unClaimEndIndex - speedDownStep;
        while (speedDownBeforeEndIndex < 0)
            speedDownBeforeEndIndex += unClaimLength;
        bool isSpeedDown = false;
        float speedDownOffset = 0.1f;
        allSpinItems[allSpinItemUnClaimIndex[unClaimStartIndex]].SetBg(true);
        while (speedDownStep >= 0)
        {
            yield return new WaitForSeconds(normalIntervalTime);
            unClaimStartIndex++;
            if (unClaimStartIndex > unClaimLength - 1)
                unClaimStartIndex = 0;
            if (unClaimStartIndex == unClaimEndIndex)
                turn--;
            if (turn <= 0)
            {
                if (unClaimStartIndex == speedDownBeforeEndIndex)
                    isSpeedDown = true;
                if (isSpeedDown)
                {
                    speedDownStep--;
                    normalIntervalTime += speedDownOffset;
                }
            }
            allSpinItems[allSpinItemUnClaimIndex[unClaimStartIndex]].SetBg(true);
            allSpinItems[allSpinItemUnClaimIndex[unClaimStartIndex == 0 ? unClaimLength - 1 : unClaimStartIndex - 1]].SetBg(false);
        }
        GameManager.Instance.TriggerTask(TaskType.PlayLuckySpin, 1);
        GameManager.Instance.SaveGetSpinRewardState(allSpinItemUnClaimIndex[unClaimEndIndex]);
        if (allSpinItemRewardTypes[endIndex] != Reward.PlayBingoTicket)
            allSpinItems[endIndex].Claim();
        switch (reward)
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
                AudioManager.PlayOneShot(AudioPlayArea.DrawPhone_getFragment);
                break;
        }
        yield return new WaitForSeconds(1);
        UIManager.ShowPanel(PopPanel.GetSingleRewardPanel, (int)PopPanel.DrawPhonePanel, (int)reward, rewardNum);
        isSpin = false;
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
        if (sourcePanelIndex == (int)PopPanel.GetSingleRewardPanel)
            RefreshAllGiftProgress();
        else if (sourcePanelIndex == (int)BasePanel.Menu)
            RefreshAllGiftProgress();
    }
    int giftCount = 0;
    public void SendPanelShowArgs(params int[] args)
    {
        RefreshAllGiftProgress();
        RefreshSpinState();
        if (SaveManager.SaveData.isFirstEnterDrawPhone)
            UIManager.ShowPanel(PopPanel.GetSingleRewardPanel, (int)PopPanel.DrawPhonePanel, (int)ConfigManager.GetRandomGiftFragment(), 1);
    }
    private void RefreshAllGiftProgress()
    {
        for (int i = 0; i < giftCount; i++)
            allGifts[i].SetProgress(SaveManager.GetRewardValue(allGiftRewardTypes[i]));
    }
    private void RefreshSpinState()
    {
        int current = SaveManager.SaveData.bingoForLuckySpin_Times;
        int target = ConfigManager.GetCurrentSpinNeedBingoTime();
        progressText.text = current + "/" + target;
        progress_fillImage.fillAmount = current / (float)target;
        spinButton.interactable = current >= target;
    }
    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.DrawPhonePanel, BasePanel.Menu);
    }
}
