using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_CashoutHelpPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
    }

    public void TriggerPanelHideEvent()
    {
    }

    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
}
