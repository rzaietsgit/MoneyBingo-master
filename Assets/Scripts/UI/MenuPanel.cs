using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour, IUIMessage
{
    public CanvasGroup canvasGroup;
    public RectTransform topRect;
    public Animator animator;
    public Button goldButton;
    public Text goldText;
    public Button cashButton;
    public Text cashText;
    public GameObject paypalGo;
    public Button amazonButton;
    public Text amazonText;
    public Button settingButton;
    public Button backMainButton;
    public Button giftButton;

    public Button taskButton;
    public GameObject taskRedpoint;
    public GameObject task_lockGo;
    public Image phone_progressImage;
    public Button phoneButton;
    public GameObject phone_lockGo;
    public Button pigButton;
    public Image pig_progressImage;
    public GameObject pig_lockGo;
    public Button gift_ballonButton;
    private void Awake()
    {
        goldButton.AddClickEvent(OnGoldButtonClick);
        cashButton.AddClickEvent(OnCashButtonClick);
        amazonButton.AddClickEvent(OnAmazonButtonClick);
        settingButton.AddClickEvent(OnSettingButtonClick);
        backMainButton.AddClickEvent(OnBackMainButtonClick);
        giftButton.AddClickEvent(OnGiftButtonClick);
        taskButton.AddClickEvent(OnTaskButtonClick);
        phoneButton.AddClickEvent(OnPhoneButtonClick);
        pigButton.AddClickEvent(OnPigButtonClick);
        gift_ballonButton.AddClickEvent(OnGiftBallonClick);
        if (GameManager.IsBigScreen)
        {
            topRect.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
        }
        UIManager.SetGoldTransAndCashTrans(goldButton.transform, cashButton.transform);
        if (!SaveManager.SaveData.hasGetNewPlayerReward)
            UIManager.ShowPanel(PopPanel.GetNewPlayerRewardPanel);
        else
        {
            int guideClickCardIndex = (int)GuideType.GameGuide_ClickCard;
            while (SaveManager.SaveData.guideStep < guideClickCardIndex)
                GameManager.Instance.AddGuideStep();
        }
        if (!SaveManager.SaveData.islockGiveaways)
            StartCoroutine(FlyGiftBallon());
        UIManager.SetPigCashFlyTarget(pigButton.transform);
    }
    private void OnGoldButtonClick()
    {
        if (CardGenerate.gameState == GameState.Playing || CardGenerate.gameState == GameState.Unknow)
        {
            if (UIManager.PanelShow(BasePanel.GamePanel))
            {
                if (CardGenerate.Instance.GetWillShowWheelPanel())
                    return;
                UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 1);
            }
            UIManager.ShowPanel(PopPanel.CashoutPanel, (int)BasePanel.Menu, 0);
        }
    }
    private void OnCashButtonClick()
    {
        if (CardGenerate.gameState == GameState.Playing || CardGenerate.gameState == GameState.Unknow)
        {
            if (UIManager.PanelShow(BasePanel.GamePanel))
            {
                if (CardGenerate.Instance.GetWillShowWheelPanel())
                    return;
                UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 1);
            }
            GameManager.Instance.SetHasClickCashToday();
            paypalGo.SetActive(false);
            UIManager.ShowPanel(PopPanel.CashoutPanel, (int)BasePanel.Menu, 1);
        }
    }
    private void OnAmazonButtonClick()
    {
        if (CardGenerate.gameState == GameState.Playing || CardGenerate.gameState == GameState.Unknow)
        {
            if (UIManager.PanelShow(BasePanel.GamePanel))
            {
                if (CardGenerate.Instance.GetWillShowWheelPanel())
                    return;
                UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 1);
            }
            UIManager.ShowPanel(PopPanel.CashoutPanel, (int)BasePanel.Menu, 0);
        }
    }
    private void OnSettingButtonClick()
    {
        UIManager.ShowPanel(PopPanel.SettingPanel);
    }
    private void OnBackMainButtonClick()
    {
        if (CardGenerate.gameState == GameState.Playing)
        {
            if (CardGenerate.Instance.GetWillShowWheelPanel())
                return;
            UIManager.ShowPanel(PopPanel.BackToMainPanel);
            UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 1);
        }
    }
    private void OnGiftButtonClick()
    {
        UIManager.ShowPanel(PopPanel.Friend);
        StopCoroutine("WaitPtGuideEnd");
        animator.SetBool("PtGuide", false);
    }
    private void OnTaskButtonClick()
    {
        if (SaveManager.SaveData.islockTask)
            GameManager.Instance.ShowTip("Play the game 2 times to unlock.");
        else
            UIManager.ShowPanel(PopPanel.TaskPanel);
    }
    private void OnPhoneButtonClick()
    {
        if (CardGenerate.gameState == GameState.Playing || CardGenerate.gameState == GameState.Unknow)
        {
            if (UIManager.PanelShow(BasePanel.GamePanel))
            {
                if (CardGenerate.Instance.GetWillShowWheelPanel())
                    return;
                UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 1);
            }
            if (SaveManager.SaveData.islockGiveaways)
                UIManager.ShowPanel(PopPanel.LockGiveawaysPanel);
            else
                UIManager.ShowPanel(PopPanel.DrawPhonePanel);
        }
    }
    private void OnPigButtonClick()
    {
        if (SaveManager.SaveData.islockPig)
            GameManager.Instance.ShowTip("Play the game once to unlock it.");
        else
            UIManager.ShowPanel(PopPanel.MoneyBoxPanel, (int)BasePanel.Menu);
    }
    public void AtMainToGameTransitionMid()
    {
        UIManager.ShowPanel(BasePanel.GamePanel, gamePanelNum);
    }
    int gamePanelNum = 0;
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
        switch (sourcePanelIndex)
        {
            case (int)BasePanel.MainPanel:
                Args args1 = (Args)args[0];
                if (args1 == Args.PlayMainTransitionGameAnimation)
                {
                    gamePanelNum = args[1];
                    animator.SetBool("ToGame", true);
                    AudioManager.PlayOneShot(AudioPlayArea.EnterGamePage);
                }
                break;
            case (int)BasePanel.GamePanel:
                if (args[0] == 0)
                {
                    animator.SetBool("ToGame", false);
                    taskRedpoint.SetActive(ConfigManager.CheckTaskHasFinish());
                }
                break;
            case (int)BasePanel.System:
                Reward type = (Reward)args[0];
                switch (type)
                {
                    case Reward.Cash:
                        UpdateCashText();
                        break;
                    case Reward.Gold:
                        UpdateGoldText(args[1]);
                        break;
                    case Reward.AmazonCard:
                        UpdateAmazonCard(args[1]);
                        break;
                    case Reward.Cash_MoneyBox:
                        UpdatePigProgress();
                        break;
                }
                break;
            case (int)PopPanel.TaskPanel:
                taskRedpoint.SetActive(ConfigManager.CheckTaskHasFinish());
                CheckGiveawaysGuide();
                break;
            case (int)PopPanel.DrawPhonePanel:
                phone_progressImage.fillAmount = SaveManager.SaveData.bingoForLuckySpin_Times / (float)ConfigManager.GetCurrentSpinNeedBingoTime();
                if (UIManager.PanelShow(BasePanel.GamePanel))
                    UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 0);
                break;
            case (int)PopPanel.LockGiveawaysPanel:
                if (UIManager.PanelShow(BasePanel.GamePanel))
                    UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 0);
                break;
            case (int)PopPanel.CashoutPanel:
                if (UIManager.PanelShow(BasePanel.GamePanel))
                    UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 0);
                else
                {
                    if (args[0] == 1)
                        UIManager.ShowGuidePanel((int)BasePanel.Menu, giftButton.transform.position, (giftButton.transform as RectTransform).sizeDelta, GuideType.Pt_Earn);
                }
                break;
            case (int)PopPanel.WheelPanel:
                phone_progressImage.fillAmount = SaveManager.SaveData.bingoForLuckySpin_Times / (float)ConfigManager.GetCurrentSpinNeedBingoTime();
                break;
            case (int)PopPanel.GetSingleRewardPanel:
                if (UIManager.PanelShow(PopPanel.DrawPhonePanel))
                    UIManager.SendMessageToPanel(BasePanel.Menu, PopPanel.DrawPhonePanel);
                else if(UIManager.PanelShow(BasePanel.GamePanel))
                    UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 0);
                break;
            case (int)PopPanel.GetNewPlayerRewardPanel:
                UIManager.ShowGuidePanel((int)BasePanel.Menu, cashButton.transform.position, (cashButton.transform as RectTransform).sizeDelta, GuideType.Cashout_Cash);
                break;
            case (int)PopPanel.GuidePanel:
                switch (args[0])
                {
                    case (int)GuideType.Cashout_Cash:
                        UIManager.ShowGuidePanel((int)BasePanel.Menu, amazonButton.transform.position, (amazonButton.transform as RectTransform).sizeDelta, GuideType.Cashout_Amazon);
                        break;
                    case (int)GuideType.Pt_Earn:
                        if (args[1] == 1)
                            OnGiftButtonClick();
                        break;
                    case (int)GuideType.Unlock_Pig:
                        if (args[1] == 1)
                            OnPigButtonClick();
                        else
                            CheckGiveawaysGuide();
                        break;
                    case (int)GuideType.Unlock_Task:
                        if (args[1] == 1)
                            OnTaskButtonClick();
                        break;
                    case (int)GuideType.Unlock_Giveaways:
                        if (args[1] == 1)
                            OnPhoneButtonClick();
                        StartCoroutine(FlyGiftBallon());
                        break;
                }
                break;
            case (int)PopPanel.MoneyBoxPanel:
                CheckGiveawaysGuide();
                break;
            default:
                break;
        }
    }
    public void SendPanelShowArgs(params int[] args)
    {
        taskRedpoint.SetActive(ConfigManager.CheckTaskHasFinish());
        goldText.text = SaveManager.SaveData.gold.GetTokenShowString();
        UpdateCashText();
        amazonText.text = SaveManager.SaveData.amazonCard.GetTokenShowString();
        phone_progressImage.fillAmount = SaveManager.SaveData.bingoForLuckySpin_Times / (float)ConfigManager.GetCurrentSpinNeedBingoTime();
        UpdatePigProgress();
        UpdatePigLockState();
        UpdateTaskLockState();
        UpdateGiveawaysLockState();
        paypalGo.SetActive(SaveManager.SaveData.activeDay <= 7 && !SaveManager.SaveData.hasClickCashToday);
    }
    public void OnPanelShowAnimationEnd()
    {
        if (SaveManager.SaveData.islockPig && SaveManager.SaveData.completeGameTimes >= 1)
        {
            RectTransform pigRect = pigButton.transform as RectTransform;
            GameManager.Instance.UnLockPig();
            UpdatePigLockState();
            UIManager.ShowGuidePanel((int)BasePanel.Menu, pigRect.position, pigRect.sizeDelta, GuideType.Unlock_Pig);
        }
        else if (SaveManager.SaveData.islockTask && SaveManager.SaveData.completeGameTimes >= 2)
        {
            RectTransform taskRect = taskButton.transform as RectTransform;
            GameManager.Instance.UnLockTask();
            UpdateTaskLockState();
            UIManager.ShowGuidePanel((int)BasePanel.Menu, taskRect.position, taskRect.sizeDelta, GuideType.Unlock_Task);
        }
        else
            CheckGiveawaysGuide();
    }
    private void CheckGiveawaysGuide()
    {
        if (SaveManager.SaveData.totalCardNum >= 5 && SaveManager.SaveData.islockGiveaways)
        {
            RectTransform giveawaysRect = phoneButton.transform as RectTransform;
            GameManager.Instance.UnLockGiveaways();
            UpdateGiveawaysLockState();
            UIManager.ShowGuidePanel((int)BasePanel.Menu, giveawaysRect.position, giveawaysRect.sizeDelta, GuideType.Unlock_Giveaways);
        }
    }
    private void UpdatePigLockState()
    {
        bool isLock = SaveManager.SaveData.islockPig;
        Color color = isLock ? Color.grey : Color.white;
        Image buttonImage = pigButton.image;
        Image[] images = pigButton.GetComponentsInChildren<Image>();
        foreach (var img in images)
            if (img != buttonImage)
                img.color = color;
        Text[] texts = pigButton.GetComponentsInChildren<Text>();
        foreach (var te in texts)
            te.color = color;
        Animator[] animators = pigButton.GetComponentsInChildren<Animator>();
        foreach (var ac in animators)
            ac.enabled = !isLock;
        pig_lockGo.SetActive(isLock);
        if (isLock)
            pig_lockGo.GetComponent<Image>().color = Color.white;
    }
    private void UpdateTaskLockState()
    {
        bool isLock = SaveManager.SaveData.islockTask;
        Color color = isLock ? Color.grey : Color.white;
        Image buttonImage = taskButton.image;
        Image[] images = taskButton.GetComponentsInChildren<Image>();
        foreach (var img in images)
            if (img != buttonImage)
                img.color = color;
        Text[] texts = taskButton.GetComponentsInChildren<Text>();
        foreach (var te in texts)
            te.color = color;
        Animator[] animators = taskButton.GetComponentsInChildren<Animator>();
        foreach (var ac in animators)
            ac.enabled = !isLock;
        task_lockGo.SetActive(isLock);
        taskRedpoint.GetComponent<Image>().color = color;
        if (isLock)
            task_lockGo.GetComponent<Image>().color = Color.white;
    }
    private void UpdateGiveawaysLockState()
    {
        bool isLock = SaveManager.SaveData.islockGiveaways;
        Color color = isLock ? Color.grey : Color.white;
        Image buttonImage = phoneButton.image;
        Image[] images = phoneButton.GetComponentsInChildren<Image>();
        foreach (var img in images)
            if (img != buttonImage)
                img.color = color;
        Text[] texts = phoneButton.GetComponentsInChildren<Text>();
        foreach (var te in texts)
            te.color = color;
        Animator[] animators = phoneButton.GetComponentsInChildren<Animator>();
        foreach (var ac in animators)
            ac.enabled = !isLock;
        phone_lockGo.SetActive(isLock);
        if (isLock)
            phone_lockGo.GetComponent<Image>().color = Color.white;
    }
    private void UpdateCashText()
    {
        cashText.text = "$" +SaveManager.GetShowCashValue().GetCashShowString();
    }
    private void UpdateGoldText(int num)
    {
        goldText.text = num.GetTokenShowString();
    }
    private void UpdateAmazonCard(int num)
    {
        amazonText.text = num.GetTokenShowString();
    }
    private void UpdatePigProgress()
    {
        int currentCash = SaveManager.SaveData.cash_moneyBox;
        pig_progressImage.fillAmount = (float)currentCash / ConfigManager.MoneyBoxCashTargetExchangeNeedNum;
    }
    public void TriggerPanelHideEvent()
    {
    }
    const float NormalFlyInterval = 120;
    const float FirstFlyInterval = 300;
    private IEnumerator FlyGiftBallon()
    {
        if (!SaveManager.SaveData.hasGetNewPlayerReward)
            yield return new WaitForSeconds(FirstFlyInterval);
        else
            yield return new WaitForSeconds(NormalFlyInterval);
        if (ConfigManager.CheckWetherCanFlyGiftBallon())
            animator.Play("Fly");
        else
            yield break;
        WaitForSeconds flyInterval = new WaitForSeconds(NormalFlyInterval);
        WaitForSeconds animationInterval = new WaitForSeconds(10.083f);
        while (true)
        {
            yield return animationInterval;
            yield return flyInterval;
            animator.Play("idle");
            yield return null;
            if (ConfigManager.CheckWetherCanFlyGiftBallon())
                animator.Play("Fly");
            else
                yield break;
        }
    }
    private void OnGiftBallonClick()
    {
        if (UIManager.GetWillShowWheelPanel()) return;
        animator.Play("idle");
        gift_ballonButton.gameObject.SetActive(false);
        if (ConfigManager.CheckWetherCanFlyGiftBallon())
        {
            UIManager.ShowPanel(PopPanel.GetSingleRewardPanel, (int)BasePanel.Menu, (int)ConfigManager.GetRandomGiftBallonReward(out int rewardnum), rewardnum);
            if (UIManager.PanelShow(BasePanel.GamePanel))
                UIManager.SendMessageToPanel(BasePanel.Menu, BasePanel.GamePanel, 1);
        }
    }
    public enum Args
    {
        PlayMainTransitionGameAnimation,
    }
}
