using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_MoreSpinPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Button spinButton;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        spinButton.AddClickEvent(OnSpinButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.MoreSpinPanel, PopPanel.WheelPanel, 0);
    }
    private void OnSpinButtonClick()
    {
        AdsManager.ShowRewarded(OnAdCallback);
    }
    private void OnAdCallback()
    {
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.MoreSpinPanel, PopPanel.WheelPanel, 1);
    }

    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
    }
}
