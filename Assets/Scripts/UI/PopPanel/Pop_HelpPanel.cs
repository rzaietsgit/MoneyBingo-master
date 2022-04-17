using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_HelpPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Button okButton;

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }
    int sourcePanelIndex = -1;
    public void SendPanelShowArgs(params int[] args)
    {
        sourcePanelIndex = args[0];
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel((int)PopPanel.HelpPanel, sourcePanelIndex);
    }

    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        okButton.AddClickEvent(OnOkButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnOkButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
}
