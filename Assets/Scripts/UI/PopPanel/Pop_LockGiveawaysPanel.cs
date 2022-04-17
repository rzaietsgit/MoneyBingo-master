using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_LockGiveawaysPanel : MonoBehaviour,IUIMessage
{
    public Slider progressSlider;
    public Text progressText;
    public Button closeButton;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }
    float needUnlockNum = 5;
    public void SendPanelShowArgs(params int[] args)
    {
        int current = SaveManager.SaveData.totalCardNum;
        progressSlider.value = current / needUnlockNum;
        progressText.text = current + "/" + needUnlockNum;
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.LockGiveawaysPanel, BasePanel.Menu);
    }
}
