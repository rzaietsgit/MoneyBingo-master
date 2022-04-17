using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_SlotsPanel : MonoBehaviour,IUIMessage
{
    public GameObject cash_topGo;
    public GameObject cash_titleGo;
    public GameObject other_titleGo;
    public Button closeButton;
    public Animator animator;
    public Image rewardImage;
    public Text numText;
    public Text tipText;
    public Button moreButton;
    public Button claimButton;
    public Text claimText;
    public RectTransform aRect;
    private readonly MultipleOffset[] multipleOffsets = new MultipleOffset[5]
    {
        new MultipleOffset(){Multiple=1,xOffset=-304},
        new MultipleOffset(){Multiple=2,xOffset=330},
        new MultipleOffset(){Multiple=3,xOffset=175},
        new MultipleOffset(){Multiple=4,xOffset=-146},
        new MultipleOffset(){Multiple=5,xOffset=12}
    };
    struct MultipleOffset
    {
        public int Multiple;
        public float xOffset;
    }
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButton);
        moreButton.AddClickEvent(OnMoreButtonClick);
        claimButton.AddClickEvent(OnClaimButtonClick);
    }
    private void OnIVCallback()
    {
        UIManager.HidePanel(gameObject);
        if (reward == Reward.PlayBingoTicket)
            GameManager.Instance.ChangeCardNum(rewardNum, PopPanel.GetSingleRewardPanel);
        else
            GameManager.Instance.AddReward(reward, rewardNum);
        if (reward == Reward.Gold)
            UIManager.Fly_Reward(Reward.Gold, rewardNum, rewardImage.transform.position);
        else if (reward == Reward.Cash)
            UIManager.Fly_Reward(Reward.Cash, rewardNum, rewardImage.transform.position);
        if (sourcePanelIndex == (int)PopPanel.WheelPanel)
            CardGenerate.Instance.GainCardReward(reward, rewardNum);
    }
    private void OnMoreButtonClick()
    {
        AdsManager.ShowRewarded(OnAdCallback);
    }
    private void OnAdCallback()
    {
        moreButton.interactable = false;
        claimButton.interactable = false;
        closeButton.interactable = false;
        animator.SetBool("Spin", true);
        StartCoroutine(WaitSpinTime());
    }
    private void OnClaimButtonClick()
    {
        if (reward == Reward.Ball)
            return;
        AdsManager.ShowInterstitial(OnIVCallback);
    }
    private void OnCloseButton()
    {
        if (reward == Reward.Ball)
        {
            UIManager.HidePanel(gameObject);
            return;
        }
        AdsManager.ShowInterstitial(OnIVCallback);
    }
    bool hasGetBall = false;
    public void OnSpinEnd()
    {
        animator.SetBool("Spin", false);
        animator.SetBool("SpinEnd", false);
        animator.SetBool("Show", false);
        switch (reward)
        {
            case Reward.Gold:
                GameManager.Instance.AddReward(reward, rewardNum * rewardMultiple);
                UIManager.Fly_Reward(Reward.Gold,rewardNum, rewardImage.transform.position);
                break;
            case Reward.Cash:
                GameManager.Instance.AddReward(reward, rewardNum * rewardMultiple);
                UIManager.Fly_Reward(Reward.Cash, rewardNum, rewardImage.transform.position);
                break;
            case Reward.PlayBingoTicket:
                GameManager.Instance.ChangeCardNum(rewardNum * rewardMultiple, PopPanel.GetSingleRewardPanel);
                break;
            case Reward.Ball:
                hasGetBall = true;
                GameManager.Instance.AddGetMoreBallTimes();
                UIManager.Fly_Reward(Reward.Ball, rewardMultiple, rewardImage.transform.position);
                break;
            default:
                GameManager.Instance.AddReward(reward, rewardNum * rewardMultiple);
                break;
        }
        if (sourcePanelIndex == (int)PopPanel.WheelPanel)
            CardGenerate.Instance.GainCardReward(reward, rewardNum * rewardMultiple);
        UIManager.HidePanel(gameObject);
    }
    private IEnumerator WaitSpinTime()
    {
        yield return new WaitForSeconds(1.25f);
        aRect.localPosition = new Vector3(offset, 0);
        yield return new WaitForSeconds(1.25f);
        animator.SetBool("SpinEnd", true);
    }
    float offset = 0;
    Reward reward = Reward.Empty;
    int rewardNum = 0;
    int rewardMultiple = 1;
    int sourcePanelIndex = -1;
    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
        moreButton.interactable = true;
        claimButton.interactable = true;
        closeButton.interactable = true;
        sourcePanelIndex = args[0];
        reward = (Reward)args[1];
        rewardNum = args[2];
        rewardMultiple = args[3];
        rewardImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Slots, reward.ToString());
        rewardImage.SetNativeSize();
        switch (reward)
        {
            case Reward.Cash:
                cash_titleGo.SetActive(true);
                cash_topGo.SetActive(true);
                other_titleGo.SetActive(false);
                numText.gameObject.SetActive(true);
                numText.text = "+$" + rewardNum.GetCashShowString();
                tipText.text = "You get a chance to increase reward!";
                claimText.text = "Claim";
                break;
            case Reward.Ball:
                cash_titleGo.SetActive(false);
                cash_topGo.SetActive(false);
                other_titleGo.SetActive(true);
                hasGetBall = false;
                numText.gameObject.SetActive(false);
                tipText.text = "Just one more daub to bingo in some cards. Want more balls?";
                claimText.text = string.Format("free chance:<color=#166A00>{0}</color>", ConfigManager.GetMoreBallChancePerDay - SaveManager.SaveData.todayGetMoreBallTimes);
                break;
            default:
                cash_titleGo.SetActive(false);
                cash_topGo.SetActive(false);
                other_titleGo.SetActive(true);
                numText.gameObject.SetActive(true);
                numText.text = "+" + rewardNum;
                tipText.text = "You get a chance to increase reward!";
                claimText.text = "Claim";
                break;
        }
        offset = GetMultipleXoffset(rewardMultiple);
        animator.SetBool("Show", true);
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {

    }
    private float GetMultipleXoffset(int multiple)
    {
        int length = multipleOffsets.Length;
        for(int i = 0; i < length; i++)
        {
            if (multipleOffsets[i].Multiple == multiple)
                return multipleOffsets[i].xOffset;
        }
        Debug.LogError("error slots multiple");
        return multipleOffsets[0].xOffset;
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel((int)PopPanel.SlotsPanel, sourcePanelIndex, hasGetBall ? rewardMultiple : 0);
    }
}
