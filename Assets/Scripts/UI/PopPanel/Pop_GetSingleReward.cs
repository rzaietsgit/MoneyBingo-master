using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_GetSingleReward : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Image rewardImage;
    public Text reward_nameText;
    public Button claimButton;
    public GameObject adGo;
    private void Awake()
    {
        claimButton.AddClickEvent(OnClaimButtonClick);
        closeButton.AddClickEvent(OnCloseButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject); 
    }
    private void OnClaimButtonClick()
    {
        if (rewardType == Reward.Key)
            OnAdCallback();
        else
        {
            switch (adType)
            {
                case AdType.NoAd:
                    OnAdCallback();
                    break;
                case AdType.RV:
                    clickAdTime++;
                    AdsManager.ShowRewarded(OnAdCallback);
                    break;
                case AdType.IV:
                    AdsManager.ShowRewarded(OnAdCallback);
                    break;
                default:
                    break;
            }
        }
    }
    private void OnAdCallback()
    {
        if (rewardType == Reward.PlayBingoTicket)
            GameManager.Instance.ChangeCardNum(rewardNum, PopPanel.GetSingleRewardPanel);
        else
            GameManager.Instance.AddReward(rewardType, rewardNum);
        if (rewardType == Reward.Key)
            UIManager.Fly_Reward(Reward.Key, rewardNum, rewardImage.transform.position);
        if (sourcePanelIndex == (int)PopPanel.WheelPanel||sourcePanelIndex==(int)PopPanel.DrawBoxPanel)
            CardGenerate.Instance.GainCardReward(rewardType, rewardNum);
        else if (sourcePanelIndex == (int)PopPanel.DrawPhonePanel)
            GameManager.Instance.SendAdjustDrawRewardsEvent(rewardType, true);
        UIManager.HidePanel(gameObject);
    }
    private void OnNoAdCallback()
    {
        UIManager.HidePanel(gameObject);
    }
    private string GetAdDes()
    {
        switch (sourcePanelIndex)
        {
            case (int)BasePanel.Menu:
                return "礼盒气球,获得奖励";
            case (int)PopPanel.DrawBoxPanel:
                return "钥匙宝箱,获得奖励";
            case (int)PopPanel.TaskPanel:
                return "任务完成,获得奖励";
            case (int)PopPanel.DrawPhonePanel:
                return "抽实物碎片,获得奖励";
            case (int)PopPanel.WheelPanel:
                return "转盘,获得奖励";
        }
        return "未知位置,获得奖励";
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {

    }
    Reward rewardType;
    AdType adType;
    int rewardNum;
    int sourcePanelIndex = -1;
    int clickAdTime = 0;
    public void SendPanelShowArgs(params int[] args)
    {
        clickAdTime = 0;
        sourcePanelIndex = args[0];
        rewardType = (Reward)args[1];
        rewardNum = args[2];
        rewardImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, rewardType.ToString());
        switch (rewardType)
        {
            case Reward.PlayBingoTicket:
                adType = AdType.RV;
                reward_nameText.text = "Bingo Card *" + rewardNum;
                rewardImage.transform.localPosition = Vector3.zero;
                break;
            case Reward.Cash:
                adType = AdType.RV;
                reward_nameText.text = "Lucky Money\n$" + rewardNum.GetCashShowString();
                rewardImage.transform.localPosition = Vector3.zero;
                break;
            case Reward.AmazonCard:
                adType = AdType.IV;
                reward_nameText.text = ConfigManager.GetRewardDescription(rewardType);
                rewardImage.transform.localPosition = Vector3.zero;
                break;
            case Reward.Key:
                adType = AdType.RV;
                reward_nameText.text = ConfigManager.GetRewardDescription(rewardType);
                rewardImage.transform.localPosition = Vector3.zero;
                break;
            default:
                if (SaveManager.SaveData.isFirstEnterDrawPhone)
                {
                    adType = AdType.IV;
                    GameManager.Instance.SetHasFirstEnterDrawPhone();
                }
                else if (Random.Range(0, 10) >= 7)
                    adType = AdType.IV;
                else
                    adType = AdType.RV;
                reward_nameText.text = ConfigManager.GetRewardDescription(rewardType);
                rewardImage.transform.localPosition = new Vector3(23, 0, 0);
                break;
        }
        adGo.SetActive(adType == AdType.RV);
    }

    public void TriggerPanelHideEvent()
    {
        if (rewardType == Reward.Key && SaveManager.SaveData.key >= 3)
            UIManager.ShowPanel(PopPanel.DrawBoxPanel, sourcePanelIndex);
        else
            UIManager.SendMessageToPanel((int)PopPanel.GetSingleRewardPanel, sourcePanelIndex);
        if (sourcePanelIndex == (int)PopPanel.DrawPhonePanel)
            GameManager.Instance.SendAdjustDrawRewardsEvent(rewardType, false);
    }
    private enum AdType
    {
        NoAd,
        RV,
        IV
    }
}
