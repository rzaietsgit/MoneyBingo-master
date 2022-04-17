using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_EnterCasoutTaskPanel : MonoBehaviour,IUIMessage
{
    public Text cashText;
    public Button closeButton;
    public Text first_task_des;
    public Slider first_taskSlider;
    public Text first_task_progressText;
    public Text second_task_des;
    public Slider second_taskSlider;
    public Text second_task_progressText;
    public Button cashoutButton;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        cashoutButton.AddClickEvent(OnCashoutButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnCashoutButtonClick()
    {
        if (first_taskSlider.value == 1 || second_taskSlider.value == 1)
        {
            UIManager.HidePanel(gameObject);
            if (sourcePanelIndex == (int)BasePanel.System)
            {
                GameManager.Instance.UnLockCashout();
                UIManager.ShowPanel(PopPanel.CashoutPanel, (int)PopPanel.EnterCashoutTaskPanel, 1);
            }
            else
                Debug.LogError("pig task error");
        }
        else
            GameManager.Instance.ShowTip("You need to complete any one task first.");
    }
    const int NeedAdTimes = 100;
    const int NeedActiveDays = 7;
    int sourcePanelIndex = -1;
    public void SendPanelShowArgs(params int[] args)
    {
        sourcePanelIndex = args[0];
        switch (sourcePanelIndex)
        {
            case (int)BasePanel.System:
                cashText.text = "$" + (SaveManager.SaveData.cash + SaveManager.SaveData.newPlayerCash).GetCashShowString();
                int currentAdTimes = SaveManager.SaveData.totalAdTimes;
                int currentDay = SaveManager.SaveData.activeDay;
                first_taskSlider.value = (float)currentAdTimes / NeedAdTimes;
                second_taskSlider.value = (float)currentDay / NeedActiveDays;
                first_task_progressText.text = currentAdTimes + "/" + NeedAdTimes;
                second_task_progressText.text = currentDay + "/" + NeedActiveDays;
                first_task_des.text = "Watch 100 Videos";
                second_task_des.text = "Active 7 days";
                break;
            case (int)PopPanel.MoneyBoxPanel:
                cashText.text = "$" + SaveManager.SaveData.cash_moneyBox.GetCashShowString();
                int currentPigAdTimes = SaveManager.SaveData.pig_totalAdTimes;
                int currentPigPlayCard = SaveManager.SaveData.pig_totalPlayCard;
                first_taskSlider.value = (float)currentPigAdTimes / ConfigManager.Pig_Task_NeedAdTimes;
                second_taskSlider.value = (float)currentPigPlayCard / ConfigManager.Pig_Task_NeedCardNum;
                first_task_progressText.text = currentPigAdTimes + "/" + ConfigManager.Pig_Task_NeedAdTimes;
                second_task_progressText.text = currentPigPlayCard + "/" + ConfigManager.Pig_Task_NeedCardNum;
                first_task_des.text = "Watch RV100 times";
                second_task_des.text = "Play 100 bingo cards";
                break;
        }
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
        if (UIManager.PanelShow(BasePanel.GamePanel))
            UIManager.SendMessageToPanel(PopPanel.EnterCashoutTaskPanel, BasePanel.GamePanel);
    }
}
