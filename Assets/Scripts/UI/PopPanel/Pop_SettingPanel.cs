using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_SettingPanel : MonoBehaviour,IUIMessage
{
    public Button closeButton;
    public Button withdrawButton;
    public Button musicButton;
    public Text music_contentText;
    public Button soundButton;
    public Text sound_contentText;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        withdrawButton.AddClickEvent(OnWithdrawButtonClick);
        musicButton.AddClickEvent(OnMusicButtonClick);
        soundButton.AddClickEvent(OnSoundButtonClick);
        SetMusicButton(SaveManager.SaveData.music_on);
        SetSoundButton(SaveManager.SaveData.sound_on);
    }
    private void SetMusicButton(bool musicState)
    {
        if (musicState)
        {
            musicButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Setting, "switch_on");
            music_contentText.text = "ON";
            music_contentText.transform.localPosition = new Vector3(38.4f, 0);
        }
        else
        {
            musicButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Setting, "switch_off");
            music_contentText.text = "OFF";
            music_contentText.transform.localPosition = new Vector3(-40.5f, 0);
        }
    }
    private void SetSoundButton(bool soundState)
    {
        if (soundState)
        {
            soundButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Setting, "switch_on");
            sound_contentText.text = "ON";
            sound_contentText.transform.localPosition = new Vector3(38.4f, 0);
        }
        else
        {
            soundButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Setting, "switch_off");
            sound_contentText.text = "OFF";
            sound_contentText.transform.localPosition = new Vector3(-40.5f, 0);
        }
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnWithdrawButtonClick()
    {
        UIManager.HidePanel(gameObject);
        UIManager.ShowPanel(PopPanel.CashoutPanel, (int)PopPanel.SettingPanel, 1);
    }
    private void OnMusicButtonClick()
    {
        bool afterState = GameManager.Instance.ClickMusicSetting();
        SetMusicButton(afterState);
        AudioManager.SetMusicState(afterState);
    }
    private void OnSoundButtonClick()
    {
        bool afterState = GameManager.Instance.ClickSoundSetting();
        SetSoundButton(afterState);
        AudioManager.SetSoundState(afterState);
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
