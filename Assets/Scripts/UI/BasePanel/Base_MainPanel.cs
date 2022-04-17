using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using ITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Base_MainPanel : MonoBehaviour, IUIMessage
{
    public Animator animator;
    public Text card_numText;
    public Button get_more_cardButton;
    public Text card_next_timerText;
    public Text play_spend_cardText;
    public Button helpButton;
    public Image card_play_tipText;
    public Text card_play_numText;
    public Button more_cardButton;
    public Button less_cardButton;
    public Button playButton;
    private static readonly int[] card_can_select_num = new int[3] { 1, 2, 4 };
    private void Awake()
    {
        get_more_cardButton.AddClickEvent(OnGetMoreCardButtonClick);
        helpButton.AddClickEvent(OnHelpButtonClick);
        more_cardButton.AddClickEvent(OnMoreCardClick);
        less_cardButton.AddClickEvent(OnLessCardClick);
        playButton.AddClickEvent(OnPlayButtonClick);
        card_numText.text = SaveManager.SaveData.cardNum.ToString();
        card_next_timerText.gameObject.SetActive(SaveManager.SaveData.cardNum < ConfigManager.NextBingoCardNeedSeconds);
        card_play_numText.text = card_can_select_num[currentCardIndex].ToString();
        card_play_tipText.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "card_" + card_can_select_num[currentCardIndex]);
        if (GameManager.IsBigScreen)
        {
            helpButton.transform.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
        }
    }
    private void OnHelpButtonClick()
    {
        UIManager.ShowPanel(PopPanel.HelpPanel, (int)BasePanel.MainPanel);
    }
    int currentCardIndex = 0;
    private void OnMoreCardClick()
    {
        if (SaveManager.SaveData.guideStep <= (int)GuideType.GameGuide_ClickCard)
            return;
        if (currentCardIndex >= 2)
            return;
        else
        {
            currentCardIndex++;
            card_play_numText.text = card_can_select_num[currentCardIndex].ToString();
            UpdatePlayButtonSprite();
            card_play_tipText.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "card_" + card_can_select_num[currentCardIndex]);
            animator.SetInteger("cardnum", card_can_select_num[currentCardIndex]);
        }
    }
    private void OnLessCardClick()
    {
        if (SaveManager.SaveData.guideStep <= (int)GuideType.GameGuide_ClickCard)
            return;
        if (currentCardIndex <= 0)
            return;
        else
        {
            currentCardIndex--;
            card_play_numText.text = card_can_select_num[currentCardIndex].ToString();
            UpdatePlayButtonSprite();
            card_play_tipText.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "card_" + card_can_select_num[currentCardIndex]);
            animator.SetInteger("cardnum", card_can_select_num[currentCardIndex]);
        }
    }
    private void OnPlayButtonClick()
    {
        if (SaveManager.SaveData.cardNum >= card_can_select_num[currentCardIndex])
        {
            play_spend_cardText.text = "-" + card_can_select_num[currentCardIndex];
            GameManager.Instance.ChangeCardNum(-card_can_select_num[currentCardIndex], BasePanel.MainPanel);
            GameManager.Instance.AddTotalPlayCardNum(card_can_select_num[currentCardIndex]);
            AnalyticsManager.Log("start_new_bingo");
            animator.SetBool("Show", false);
        }
        else
            UIManager.ShowPanel(PopPanel.GetMoreBingoCardsPanel);
    }
    private void OnGetMoreCardButtonClick()
    {
        if (SaveManager.SaveData.cardNum < ConfigManager.MaxBingoCardNum)
            UIManager.ShowPanel(PopPanel.GetMoreBingoCardsPanel);
    }
    public void AtMenuCanPlayTransitionAnimationTimePos()
    {
        UIManager.SendMessageToPanel(BasePanel.MainPanel, BasePanel.Menu, (int)MenuPanel.Args.PlayMainTransitionGameAnimation, card_can_select_num[currentCardIndex]);
    }
    public void OnCloseAnimaitonEnd()
    {
        animator.SetBool("Show", true);
        UIManager.HidePanel(gameObject);
        AdsManager.ShowBanner();
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
        if (sourcePanelIndex == (int)BasePanel.System)
        {
            card_next_timerText.text = string.Format("New card in {0}s", args[0]);
        }
        else
        {
            card_numText.text =args[0].ToString();
            card_next_timerText.gameObject.SetActive(args[0] < ConfigManager.MaxBingoCardNum);
            UpdatePlayButtonSprite();
        }
    }

    public void SendPanelShowArgs(params int[] args)
    {
        UpdatePlayButtonSprite();
        card_next_timerText.gameObject.SetActive(SaveManager.SaveData.cardNum < ConfigManager.MaxBingoCardNum);
        if (SaveManager.SaveData.guideStep <= (int)GuideType.GameGuide_ClickCard)
        {
            currentCardIndex = 0;
            card_play_numText.text = card_can_select_num[currentCardIndex].ToString();
            UpdatePlayButtonSprite();
            card_play_tipText.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "card_" + card_can_select_num[currentCardIndex]);
            animator.SetInteger("cardnum", card_can_select_num[currentCardIndex]);
            less_cardButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "less_off");
            more_cardButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "add_off");
        }
        else
        {
            less_cardButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "less_on");
            more_cardButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "add_on");
        }
    }
    private void UpdatePlayButtonSprite()
    {
        if (SaveManager.SaveData.cardNum < card_can_select_num[currentCardIndex] && SaveManager.SaveData.totdayGetMoreCardsTimes >= ConfigManager.GetMoreBingoCardsChancePerDay)
            playButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "button_off");
        else
            playButton.image.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Main, "button_on");
    }
    public void TriggerPanelHideEvent()
    {
    }
}
