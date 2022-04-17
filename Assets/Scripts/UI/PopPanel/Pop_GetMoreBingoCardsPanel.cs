using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_GetMoreBingoCardsPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Button freeButton;
    public Text left_chanceText;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        freeButton.AddClickEvent(OnFreeButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnFreeButtonClick()
    {
        if (SaveManager.SaveData.totdayGetMoreCardsTimes < ConfigManager.GetMoreBingoCardsChancePerDay)
            AdsManager.ShowRewarded(OnFreeAdCallback);
        else
            GameManager.Instance.ShowTip("There is no more times today.");
    }
    private void OnFreeAdCallback()
    {
        GameManager.Instance.ChangeCardNum(2, PopPanel.GetMoreBingoCardsPanel);
        GameManager.Instance.AddTodayGetMoreBingoCardsChance();
        UIManager.HidePanel(gameObject);
    }

    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
        left_chanceText.text = string.Format("<color=#60C117>{0}</color> free chance left", ConfigManager.GetMoreBingoCardsChancePerDay - SaveManager.SaveData.totdayGetMoreCardsTimes);
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
    }
}
