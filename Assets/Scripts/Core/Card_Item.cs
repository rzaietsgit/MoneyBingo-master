using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_Item : MonoBehaviour
{
    public Image rewardImage;
    public Animator rewardAnimator;
    public Animator numAnimator;
    public GameObject bingoEffect;
    public Image afterBingoLockImage;
    public Text numText;
    public Button button;
    private Vector2Int pos;
    private int panelIndex;
    private RectTransform rect;
    public RectTransform Rect { get { if (rect == null) rect = GetComponent<RectTransform>(); return rect; } }
    public Animator click_bingo_effectAnimator;
    [Space(15)]
    public GameObject click_bingo_rewardAnimation;
    public Image reward_animation_reward_iconImage;
    public Text reward_animation_reward_numText;
    private Reward cardReward;
    public GameObject propsEffect;
    private void Awake()
    {
        button.AddClickEvent(OnButtonClick);
    }
    public void Init(int x, int y, int panelIndex, int num, bool hasOpen, Reward reward, int numSize, Reward bingoReward)
    {
        click_bingo_rewardAnimation.SetActive(false);
        this.panelIndex = panelIndex;
        pos = new Vector2Int(x, y);
        numText.gameObject.SetActive(!hasOpen);
        bingoEffect.SetActive(hasOpen);
        numText.text = num == -1 ? "" : num.ToString();
        numAnimator.SetBool("Notice", false);
        numText.fontSize = numSize;
        rewardImage.gameObject.SetActive(!hasOpen);
        rewardImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, reward.ToString());
        cardReward = reward;
        if (bingoReward == Reward.Empty)
            afterBingoLockImage.gameObject.SetActive(false);
        else
        {
            afterBingoLockImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, bingoReward.ToString());
            afterBingoLockImage.gameObject.SetActive(true);
        }
    }
    public void OnButtonClick()
    {
        if(CardGenerate.Instance.ClickCard(pos, panelIndex))
        {
            AudioManager.PlayOneShot(AudioPlayArea.ClickCardBingo);
            bingoEffect.SetActive(true);
            numText.gameObject.SetActive(false);
            click_bingo_effectAnimator.Play("CardBingo", 0);
            if (cardReward != Reward.Empty)
            {
                int rewardNum = ConfigManager.GetCardRandomRewradNum(cardReward);
                switch (cardReward)
                {
                    case Reward.Gold:
                        reward_animation_reward_numText.text = "+" + rewardNum;
                        reward_animation_reward_numText.color = Color.yellow;
                        reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Gold.ToString());
                        GameManager.Instance.AddReward(Reward.Gold, rewardNum);
                        CardGenerate.Instance.GainCardReward(Reward.Gold, rewardNum);
                        break;
                    case Reward.Cash:
                        reward_animation_reward_numText.text = "+$" + rewardNum.GetCashShowString();
                        reward_animation_reward_numText.color = Color.green;
                        reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Cash.ToString());
                        GameManager.Instance.AddReward(Reward.Cash, rewardNum);
                        CardGenerate.Instance.GainCardReward(Reward.Cash, rewardNum);
                        break;
                    case Reward.ManyGold:
                        reward_animation_reward_numText.text = "+" + rewardNum * 3;
                        reward_animation_reward_numText.color = Color.yellow;
                        reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Gold.ToString());
                        GameManager.Instance.AddReward(Reward.Gold, rewardNum * 3);
                        CardGenerate.Instance.GainCardReward(Reward.Gold, rewardNum * 3);
                        break;
                    case Reward.ManyCash:
                        reward_animation_reward_numText.text = "+$" + (rewardNum * 3).GetCashShowString();
                        reward_animation_reward_numText.color = Color.green;
                        reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Cash.ToString());
                        GameManager.Instance.AddReward(Reward.Cash, rewardNum * 3);
                        CardGenerate.Instance.GainCardReward(Reward.Cash, rewardNum * 3);
                        break;
                    case Reward.Key:
                        reward_animation_reward_numText.text = "+" + rewardNum;
                        reward_animation_reward_numText.color = Color.yellow;
                        reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Key.ToString());
                        GameManager.Instance.AddReward(Reward.Key, rewardNum);
                        UIManager.Fly_Reward(Reward.Key, rewardNum, transform.position);
                        CardGenerate.Instance.GainCardReward(Reward.Key, rewardNum);
                        if (SaveManager.SaveData.key >= 3 && !CardGenerate.Instance.GetWillShowWheelPanel())
                        {
                            CardGenerate.Instance.Pause();
                            UIManager.ShowPanel(PopPanel.DrawBoxPanel, (int)BasePanel.GamePanel);
                        }
                        break;
                }
                click_bingo_rewardAnimation.SetActive(true);
            }
            rewardImage.gameObject.SetActive(false);
        }
    }
    public void ForceBingo()
    {
        if (!CardGenerate.Instance.ForceBingoCard(pos, panelIndex))
            return;
        bingoEffect.SetActive(true);
        numText.gameObject.SetActive(false);
        click_bingo_effectAnimator.Play("CardBingo", 0);
        if (cardReward != Reward.Empty)
        {
            int rewardNum = ConfigManager.GetCardRandomRewradNum(cardReward);
            switch (cardReward)
            {
                case Reward.Gold:
                    reward_animation_reward_numText.text = "+" + rewardNum;
                    reward_animation_reward_numText.color = Color.yellow;
                    reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Gold.ToString());
                    GameManager.Instance.AddReward(Reward.Gold, rewardNum);
                    CardGenerate.Instance.GainCardReward(Reward.Gold, rewardNum);
                    break;
                case Reward.Cash:
                    reward_animation_reward_numText.text = "+$" + rewardNum.GetCashShowString();
                    reward_animation_reward_numText.color = Color.green;
                    reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Cash.ToString());
                    GameManager.Instance.AddReward(Reward.Cash, rewardNum);
                    CardGenerate.Instance.GainCardReward(Reward.Cash, rewardNum);
                    break;
                case Reward.ManyGold:
                    reward_animation_reward_numText.text = "+" + rewardNum * 3;
                    reward_animation_reward_numText.color = Color.yellow;
                    reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Gold.ToString());
                    GameManager.Instance.AddReward(Reward.Gold, rewardNum * 3);
                    CardGenerate.Instance.GainCardReward(Reward.Gold, rewardNum * 3);
                    break;
                case Reward.ManyCash:
                    reward_animation_reward_numText.text = "+$" + (rewardNum * 3).GetCashShowString();
                    reward_animation_reward_numText.color = Color.green;
                    reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Cash.ToString());
                    GameManager.Instance.AddReward(Reward.Cash, rewardNum * 3);
                    CardGenerate.Instance.GainCardReward(Reward.Cash, rewardNum * 3);
                    break;
                case Reward.Key:
                    reward_animation_reward_numText.text = "+" + rewardNum;
                    reward_animation_reward_numText.color = Color.yellow;
                    reward_animation_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, Reward.Key.ToString());
                    GameManager.Instance.AddReward(Reward.Key, rewardNum);
                    CardGenerate.Instance.GainCardReward(Reward.Key, rewardNum);
                    UIManager.Fly_Reward(Reward.Key, rewardNum, transform.position);
                    if (SaveManager.SaveData.key >= 3 && !CardGenerate.Instance.GetWillShowWheelPanel())
                    {
                        CardGenerate.Instance.Pause();
                        UIManager.ShowPanel(PopPanel.DrawBoxPanel, (int)BasePanel.GamePanel);
                    }
                    break;
            }
            click_bingo_rewardAnimation.SetActive(true);
        }
        rewardImage.gameObject.SetActive(false);
    }
    public void SetReward(Reward reward)
    {
        if (!CardGenerate.Instance.SetCardReward(pos, panelIndex, reward))
            return;
        rewardImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, reward.ToString());
        rewardAnimator.Play("RewardGenerate");
        cardReward = reward;
        click_bingo_rewardAnimation.SetActive(false);
        propsEffect.SetActive(true);
    }
    public void MultipleReward()
    {
        Reward willBeSet = cardReward;
        if (cardReward == Reward.Cash)
            willBeSet = Reward.ManyCash;
        else if (cardReward == Reward.Gold)
            willBeSet = Reward.ManyGold;
        if (!CardGenerate.Instance.SetCardReward(pos, panelIndex, willBeSet))
            return;
        cardReward = willBeSet;
        rewardImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, cardReward.ToString());
        rewardAnimator.Play("RewardGenerate");
        click_bingo_rewardAnimation.SetActive(false);
        propsEffect.SetActive(true);
    }
    public void SetBingoRewardLockImage(BingoLockType bingoReward)
    {
        afterBingoLockImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, bingoReward.ToString());
        afterBingoLockImage.gameObject.SetActive(true);
    }
    public void NoticeThisCard()
    {
        numAnimator.SetBool("Notice", true);
    }
}
