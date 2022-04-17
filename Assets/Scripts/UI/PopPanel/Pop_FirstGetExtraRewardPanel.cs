using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_FirstGetExtraRewardPanel : MonoBehaviour, IUIMessage
{
    public Text titleText;
    public Button closeButton;
    public Image extraImage;
    public Text desText;
    public Button freeButton;
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
        UIManager.HidePanel(gameObject);
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }
    ExtraReward rewardType;
    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
        rewardType = (ExtraReward)args[0];
        extraImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.FirstGetExtraReward, rewardType.ToString());
        switch (rewardType)
        {
            case ExtraReward.ExtraGold:
                titleText.text = "Coin Squares";
                desText.text = "<color=#FF4700>Add rewards </color>in some empty squares";
                break;
            case ExtraReward.TribleReward:
                titleText.text = "Triple Square";
                desText.text = "<color=#FF4700>Triple rewards </color>in the squares";
                break;
            case ExtraReward.Star:
                titleText.text = "Daub";
                desText.text = "Randomly daub the square on the card";
                break;
        }
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.AfterFirstGetExtraReward();
        SaveManager.SaveData.hasFirstGetExtraReward[(int)rewardType] = true;
    }
}
