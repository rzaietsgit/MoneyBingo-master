using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_MoneyBoxPanel : MonoBehaviour,IUIMessage
{
    public GameObject titleGo;
    public Button helpButton;
    public Button closeButton;
    public RectTransform pigRect;
    public Slider progressSlider;
    public Text progressText;
    public Text add_cashText;
    public Button saveButton;
    public Button cashoutButton;
    private void Awake()
    {
        helpButton.AddClickEvent(OnHelpClick);
        closeButton.AddClickEvent(OnCloseButtonClick);
        saveButton.AddClickEvent(OnSaveClick);
        cashoutButton.AddClickEvent(OnCashoutClick);
        UIManager.SetPigCashFlyTarget(pigRect);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnSaveClick()
    {
        AdsManager.ShowInterstitial(OnAdCallback);
    }
    private void OnAdCallback()
    {
        saveButton.interactable = false;
        GameManager.Instance.AddPigCash(addCash);
        UIManager.Fly_Reward(Reward.Cash_MoneyBox, addCash, add_cashText.transform.position);
        StartCoroutine("WaitAutoIncrease");
    }
    private IEnumerator WaitAutoIncrease()
    {
        int partCount = Mathf.Min(addCash, FlyReward.Instance.all_iconImages.Count);
        float startIncreaseTime = 0.5f;
        float increaseInterval = 0.05f;
        int currentPart = 0;
        yield return new WaitForSeconds(startIncreaseTime);
        WaitForSeconds interval = new WaitForSeconds(increaseInterval);
        int targetNum = SaveManager.SaveData.cash_moneyBox;
        int originNum = targetNum - addCash;
        int maxNum = ConfigManager.MoneyBoxCashTargetExchangeNeedNum;
        float currentNum;
        while (currentPart < partCount)
        {
            currentPart++;
            currentNum = Mathf.Lerp(originNum, targetNum, (float)currentPart / partCount);
            progressSlider.value = currentNum / maxNum;
            progressText.text = ((int)currentNum).GetCashShowString() + "/" + (maxNum/100);
            yield return interval;
        }
        yield return new WaitForSeconds(1);
        if (targetNum >= ConfigManager.MoneyBoxCashTargetExchangeNeedNum)
        {
            saveButton.gameObject.SetActive(false);
            cashoutButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
        }
        else
            UIManager.HidePanel(gameObject);
    }
    private void OnCashoutClick()
    {
        if (progressSlider.value == 1)
        {
            if (SaveManager.SaveData.pig_totalAdTimes >= ConfigManager.Pig_Task_NeedAdTimes || SaveManager.SaveData.pig_totalPlayCard >= ConfigManager.Pig_Task_NeedCardNum)
            {
                GameManager.Instance.AddShowedPigCash(ConfigManager.MoneyBoxCashTargetExchangeNeedNum);
                GameManager.Instance.AddPigCash(-ConfigManager.MoneyBoxCashTargetExchangeNeedNum);
                UIManager.Fly_Reward(Reward.Cash, ConfigManager.MoneyBoxCashTargetExchangeNeedNum, pigRect.position);
                GameManager.Instance.ResetPigCashTask(); UIManager.HidePanel(gameObject);
            }
            else
                UIManager.ShowPanel(PopPanel.EnterCashoutTaskPanel, (int)PopPanel.MoneyBoxPanel);
        }
        else
            GameManager.Instance.ShowTip("Sorry, your balance is not enough to withdraw.");
    }
    private void OnHelpClick()
    {
        UIManager.ShowPanel(PopPanel.PigHelpPanel);
    }
    int sourcePanelIndex = -1;
    int addCash = 0;
    public void SendPanelShowArgs(params int[] args)
    {
        saveButton.interactable = true;
        sourcePanelIndex = args[0];
        int currentCash = SaveManager.SaveData.cash_moneyBox;
        progressSlider.value = (float)currentCash / ConfigManager.MoneyBoxCashTargetExchangeNeedNum;
        progressText.text = currentCash.GetCashShowString() + "/" + (ConfigManager.MoneyBoxCashTargetExchangeNeedNum / 100);
        switch (sourcePanelIndex)
        {
            case (int)BasePanel.GamePanel:
                addCash = args[1];
                if (addCash == -1)
                {
                    add_cashText.gameObject.SetActive(false);
                    saveButton.gameObject.SetActive(false);
                    cashoutButton.gameObject.SetActive(true);
                    titleGo.SetActive(false);
                    closeButton.gameObject.SetActive(true);
                }
                else
                {
                    add_cashText.text = "+$" + addCash.GetCashShowString();
                    add_cashText.gameObject.SetActive(true);
                    saveButton.gameObject.SetActive(true);
                    cashoutButton.gameObject.SetActive(false);
                    titleGo.SetActive(true);
                    closeButton.gameObject.SetActive(false);
                }
                break;
            case (int)BasePanel.Menu:
                add_cashText.gameObject.SetActive(false);
                saveButton.gameObject.SetActive(false);
                cashoutButton.gameObject.SetActive(true);
                titleGo.SetActive(false);
                closeButton.gameObject.SetActive(true);
                break;
        }
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }
    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel((int)PopPanel.MoneyBoxPanel, sourcePanelIndex);
    }
}
