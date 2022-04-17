using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_BackMainPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Button leaveButton;
    public Button stayButton;

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.BackToMainPanel, BasePanel.GamePanel, 0);
    }

    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        leaveButton.AddClickEvent(OnLeaveButtonClick);
        stayButton.AddClickEvent(OnStayButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnLeaveButtonClick()
    {
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.BackToMainPanel, BasePanel.GamePanel, 1);
        AdsManager.HideBanner();
    }
    private void OnStayButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
}
