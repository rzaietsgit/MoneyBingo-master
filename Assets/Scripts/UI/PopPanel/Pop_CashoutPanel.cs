using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_CashoutPanel : MonoBehaviour, IUIMessage
{
    public RectTransform bgRect;
    public RectTransform otherRect;
    public Button recordButton;
    public Button backButton;
    public Button helpButton;
    public Button closeButton;

    private const int PtNeedNum1 = 5000000;
    private const int PtNeedNum2 = 10000000;
    private const int PtNeedNum3 = 50000000;
    private const int PaypalNeedGold1 = 1000000;
    private const int PaypalNeedGold2 = 2000000;
    private const int AmazonNeedCard1 = 100;
    private const int AmazonNeedCard2 = 200;
    private const int CashNeedNum = 15000;
    [Space(15)]
    public CanvasGroup gold_cashoutGroup;
    public Text ptText;
    public Text pt_cashText;
    public Button pt_baseButton;
    public Button pt_cashoutButton_1;
    public Button pt_cashoutButton_2;
    public Button pt_cashoutButton_3;
    public Button paypal_cashoutButton_1;
    public Text paypal_cashout_progressText_1;
    public Button paypal_cashoutButton_2;
    public Text paypal_cashout_progressText_2;
    public Button amazon_cashoutButton_1;
    public Text amazon_cashout_progressText_1;
    public Button amazon_cashoutButton_2;
    public Text amazon_cashout_progressText_2;

    [Space(15)]
    public CanvasGroup cash_cashoutGroup;
    public Text cashText;
    public List<Button> cashoutButtons = new List<Button>();
    [Space(15)]
    public CanvasGroup cashout_recordGroup;
    private void Awake()
    {
        pt_baseButton.AddClickEvent(OnPtBaseButtonClick);
        pt_cashoutButton_1.AddClickEvent(() => { OnPtCashoutButtonClick(PtNeedNum1); });
        pt_cashoutButton_2.AddClickEvent(() => { OnPtCashoutButtonClick(PtNeedNum2); });
        pt_cashoutButton_3.AddClickEvent(() => { OnPtCashoutButtonClick(PtNeedNum3); });
        recordButton.AddClickEvent(OnRecordButtonClick);
        backButton.AddClickEvent(OnBackButtonClick);
        helpButton.AddClickEvent(OnHelpButtonClick);
        closeButton.AddClickEvent(OnCloseButtonClick);
        paypal_cashoutButton_1.AddClickEvent(() => { OnPaypalCashoutButtonClick(PaypalNeedGold1); });
        paypal_cashoutButton_2.AddClickEvent(() => { OnPaypalCashoutButtonClick(PaypalNeedGold2); });
        amazon_cashoutButton_1.AddClickEvent(() => { OnAmazonCashoutButtonClick(AmazonNeedCard1); });
        amazon_cashoutButton_2.AddClickEvent(() => { OnAmazonCashoutButtonClick(AmazonNeedCard2); });
        foreach (var btn in cashoutButtons)
            btn.AddClickEvent(OnCashButtonClick);
        if (GameManager.IsBigScreen)
        {
            bgRect.sizeDelta *= GameManager.ExpandCoe;
            otherRect.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
            closeButton.transform.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
            gold_cashoutGroup.GetComponent<RectTransform>().sizeDelta *= GameManager.ExpandCoe;
            cash_cashoutGroup.GetComponent<RectTransform>().sizeDelta *= GameManager.ExpandCoe;
            cashout_recordGroup.GetComponent<RectTransform>().sizeDelta *= GameManager.ExpandCoe;
        }
    }
    private void OnRecordButtonClick()
    {
        cashout_recordGroup.alpha = 1;
        cashout_recordGroup.blocksRaycasts = true;
        backButton.gameObject.SetActive(true);
        recordButton.gameObject.SetActive(false);
        helpButton.gameObject.SetActive(false);
        if (isCashClick)
        {
            cash_cashoutGroup.alpha = 0;
            cash_cashoutGroup.blocksRaycasts = false;
        }
        else
        {
            gold_cashoutGroup.alpha = 0;
            gold_cashoutGroup.blocksRaycasts = false;
        }
    }
    private void OnBackButtonClick()
    {
        cashout_recordGroup.alpha = 0;
        cashout_recordGroup.blocksRaycasts = false;
        backButton.gameObject.SetActive(false);
        recordButton.gameObject.SetActive(true);
        helpButton.gameObject.SetActive(true);
        if (isCashClick)
        {
            gold_cashoutGroup.alpha = 0;
            gold_cashoutGroup.blocksRaycasts = false;
            cash_cashoutGroup.alpha = 1;
            cash_cashoutGroup.blocksRaycasts = true;
        }
        else
        {
            cash_cashoutGroup.alpha = 0;
            cash_cashoutGroup.blocksRaycasts = false;
            gold_cashoutGroup.alpha = 1;
            gold_cashoutGroup.blocksRaycasts = true;
        }
    }
    private void OnHelpButtonClick()
    {
        UIManager.ShowPanel(PopPanel.CashoutHelpPanel);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnPtBaseButtonClick()
    {
        GameManager.Instance.ShowTip("Not enough money, please earn points.", 2);
        needGuidePtEarn = true;
        UIManager.HidePanel(gameObject);
    }
    private void OnPtCashoutButtonClick(int needPt)
    {
        if(SaveManager.SaveData.friendData.user_total >= needPt)
            GameManager.Instance.ShowTip("Illegal behavior, cannot be exchanged.", 2);
        else
        {
            GameManager.Instance.ShowTip("Not enough money, please earn points.", 2);
            needGuidePtEarn = true;
            UIManager.HidePanel(gameObject);
        }
    }
    private void OnPaypalCashoutButtonClick(int needGold)
    {
        if(SaveManager.SaveData.gold >= needGold)
            GameManager.Instance.ShowTip("Illegal behavior, cannot be exchanged.", 2);
        else
            GameManager.Instance.ShowTip("Not enough tokens to exchange.", 2);
    }
    private void OnAmazonCashoutButtonClick(int needAmazon)
    {
        if(SaveManager.SaveData.amazonCard >= needAmazon)
            GameManager.Instance.ShowTip("Illegal behavior, cannot be exchanged.", 2);
        else
            GameManager.Instance.ShowTip("Not enough amazon cards to exchange.", 2);
    }
    private void OnCashButtonClick()
    {
        if(SaveManager.GetShowCashValue() >= CashNeedNum)
            GameManager.Instance.ShowTip("Illegal behavior, cannot be exchanged.", 2);
        else
            GameManager.Instance.ShowTip("Not enough money to exchange.", 2);
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }
    int sourcePanelIndex = -1;
    bool isCashClick = false;
    bool needGuidePtEarn = false;
    public void SendPanelShowArgs(params int[] args)
    {
        needGuidePtEarn = false;
        sourcePanelIndex = args[0];
        isCashClick = args[1] == 1;
        cashout_recordGroup.alpha = 0;
        cashout_recordGroup.blocksRaycasts = false;
        backButton.gameObject.SetActive(false);
        recordButton.gameObject.SetActive(true);
        helpButton.gameObject.SetActive(true);
        if (isCashClick)
        {
            gold_cashoutGroup.alpha = 0;
            gold_cashoutGroup.blocksRaycasts = false;
            cash_cashoutGroup.alpha = 1;
            cash_cashoutGroup.blocksRaycasts = true;
            cashText.text = "$" + SaveManager.GetShowCashValue().GetCashShowString();
        }
        else
        {
            cash_cashoutGroup.alpha = 0;
            cash_cashoutGroup.blocksRaycasts = false;
            gold_cashoutGroup.alpha = 1;
            gold_cashoutGroup.blocksRaycasts = true;
            int gold = SaveManager.SaveData.gold;
            int amazon = SaveManager.SaveData.amazonCard;
            ptText.text = string.Format("{0} <size=50>PT</size>", SaveManager.SaveData.friendData.user_total);
            pt_cashText.text = string.Format("≈ ${0}", ((int)SaveManager.SaveData.friendData.user_total / 1000).GetCashShowString());
            paypal_cashout_progressText_1.text = string.Format("<color=#FFE702>{0}</color>/{1}", gold, PaypalNeedGold1);
            paypal_cashout_progressText_2.text = string.Format("<color=#FFE702>{0}</color>/{1}", gold, PaypalNeedGold2);
            amazon_cashout_progressText_1.text = string.Format("<color=#FFE702>{0}</color>/{1}", amazon, AmazonNeedCard1);
            amazon_cashout_progressText_2.text = string.Format("<color=#FFE702>{0}</color>/{1}", amazon, AmazonNeedCard2);
        }
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.CashoutPanel, BasePanel.Menu, needGuidePtEarn ? 1 : 0);
    }
}
