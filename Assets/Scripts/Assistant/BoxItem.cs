using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class BoxItem : MonoBehaviour
{
    public Button boxButton;
    public GameObject rewardGo;
    public Image iconImage;
    public Text numText;
    static Pop_DrawBoxPanel parent = null;
    bool hasOpen = false;
    private void Awake()
    {
        boxButton.AddClickEvent(OnBoxClick);
    }
    public void Init(Pop_DrawBoxPanel _DrawBoxPanel)
    {
        if (parent == null)
            parent = _DrawBoxPanel;
        boxButton.gameObject.SetActive(true);
        rewardGo.SetActive(false);
        hasOpen = false;
    }
    private void SetReward(Reward reward,int rewardNum)
    {
        hasOpen = true;
        iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, reward.ToString());
        if (reward == Reward.Cash)
            numText.text = "+$" + rewardNum.GetCashShowString();
        else
            numText.text = "+" + rewardNum;
        switch (reward)
        {
            case Reward.Gold:
            case Reward.Cash:
                GameManager.Instance.AddReward(reward, rewardNum);
                UIManager.Fly_Reward(reward, rewardNum, iconImage.transform.position);
                CardGenerate.Instance.GainCardReward(reward, rewardNum);
                break;
            default:
                UIManager.ShowPanel(PopPanel.GetSingleRewardPanel, (int)PopPanel.DrawBoxPanel, (int)reward, rewardNum);
                break;
        }
        GameManager.Instance.AddTotalOpenBoxTime();
        boxButton.gameObject.SetActive(false);
        rewardGo.SetActive(true);
    }
    private void OnBoxClick()
    {
        if (hasOpen)
            return;
        if (parent.OnBoxClick())
            SetReward(ConfigManager.GetRandomdDrawBoxReward(out int rewardNum, parent.GetOpenBoxTime()), rewardNum);
        else
            AdsManager.ShowRewarded(OnAdCallback);
    }
    private void OnAdCallback()
    {
        parent.OnBoxClickWhenNoKey();
    }
    public bool GetBoxOpenState()
    {
        return hasOpen;
    }
}
