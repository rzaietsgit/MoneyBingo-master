using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_GameOverPanel : MonoBehaviour, IUIMessage
{
    public Button closeButton;
    public GameOverItemOne singleItem;
    private GameOverItemOne[] items = new GameOverItemOne[(int)Reward.Num];
    public Button homeButton;
    public Button againButton;
    public Text againText;
    private void Awake()
    {
        items[(int)Reward.Gold] = singleItem;
        closeButton.AddClickEvent(OnCloseButtonClick);
        homeButton.AddClickEvent(OnHomeButtonClick);
        againButton.AddClickEvent(OnAgainButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.GameOverPanel, BasePanel.GamePanel, 1);
    }
    private void OnHomeButtonClick()
    {
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.GameOverPanel, BasePanel.GamePanel, 1);
    }
    private void OnAgainButtonClick()
    {
        UIManager.HidePanel(gameObject);
        UIManager.SendMessageToPanel(PopPanel.GameOverPanel, BasePanel.GamePanel, 0);
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
        againText.text = "PLAY AGAIN\n" + (args[0] == 1 ? "1  Cards" : args[0] + "  Cards");
        int argLength = args.Length;
        foreach (var item in items)
            if (item != null)
                item.gameObject.SetActive(false);
        for(int i = 1; i < argLength; i += 2)
        {
            GameOverItemOne gameOverItem = items[args[i]];
            if (gameOverItem == null)
            {
                gameOverItem = Instantiate(singleItem.gameObject, singleItem.transform.parent).GetComponent<GameOverItemOne>();
                items[args[i]] = gameOverItem;
            }
            gameOverItem.gameObject.SetActive(true);
            gameOverItem.Init((Reward)args[i], args[i + 1]);
        }
        againButton.interactable = SaveManager.SaveData.cardNum >= args[0];
    }

    public void TriggerPanelHideEvent()
    {
    }
}
