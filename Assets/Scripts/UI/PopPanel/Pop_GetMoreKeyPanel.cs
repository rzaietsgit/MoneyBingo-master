using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_GetMoreKeyPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Button adButton;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        adButton.AddClickEvent(OnAdButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnAdButtonClick()
    {
        clickAdTime++;
        AdsManager.ShowRewarded(OnAdCallback);
    }
    private void OnAdCallback()
    {
        GameManager.Instance.AddReward(Reward.Key, 1);
        UIManager.Fly_Reward(Reward.Key, 1, transform.position);
        UIManager.HidePanel(gameObject);
    }
    int clickAdTime = 0;
    public void SendPanelShowArgs(params int[] args)
    {
        clickAdTime = 0;
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.GetMoreKeyPanel, BasePanel.GamePanel);
    }
}
