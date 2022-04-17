using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_GetNewPlayerRewardPanel : MonoBehaviour
{
    public RectTransform iconRect;
    public Button saveButton;
    private void Awake()
    {
        saveButton.AddClickEvent(OnSaveButtonClick);
    }
    private void OnSaveButtonClick()
    {
        GameManager.Instance.AddShowedPigCash(ConfigManager.NewPlayerRewardMoney);
        GameManager.Instance.SetHasGetNewPlayerReward();
        UIManager.Fly_Reward(Reward.Cash, ConfigManager.NewPlayerRewardMoney, iconRect.position);
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.GetNewPlayerRewardPanel, BasePanel.Menu);
    }
}
