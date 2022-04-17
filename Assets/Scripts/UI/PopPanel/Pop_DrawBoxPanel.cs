using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class Pop_DrawBoxPanel : MonoBehaviour,IUIMessage
{
    public Image single_best_prizeImage;
    private RectTransform all_best_prizeRect;
    private readonly List<Image> all_best_prizeImages = new List<Image>();
    public List<BoxItem> allBoxItems;
    public GameObject all_key_parentGo;
    public List<GameObject> all_keysGo;
    public Button adButton;
    public RectTransform ad_iconRect;
    public RectTransform ad_textRect;
    public Button continueButton;
    public Button nothanksButton;
    private void Awake()
    {
        all_best_prizeImages.Add(single_best_prizeImage);
        adButton.AddClickEvent(OnAdButtonClick);
        continueButton.AddClickEvent(OnContinueButtonClick);
        nothanksButton.AddClickEvent(OnNothanksButtonClick);
        int startRewardIndex = (int)Reward.Macbook;
        int rewardCount = (int)Reward.Pt - startRewardIndex;
        all_best_prizeRect = single_best_prizeImage.transform.parent as RectTransform;
        #region 按顺序排列奖励
        single_best_prizeImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.Gold.ToString());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages.Add(Instantiate(single_best_prizeImage.gameObject, all_best_prizeRect).GetComponent<Image>());
        all_best_prizeImages[1].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.Cash.ToString());
        all_best_prizeImages[2].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.AmazonCard.ToString());
        all_best_prizeImages[3].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.PlayBingoTicket.ToString());
        all_best_prizeImages[4].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.SamsungPhone.ToString());
        all_best_prizeImages[5].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.Macbook.ToString());
        all_best_prizeImages[6].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.SonyTV.ToString());
        all_best_prizeImages[7].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.LV_Bag.ToString());
        all_best_prizeImages[8].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.iPad.ToString());
        all_best_prizeImages[9].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.DysonHairDryer.ToString());
        all_best_prizeImages[10].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.Switch.ToString());
        all_best_prizeImages[11].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.PhilipsCoffeeMachine.ToString());
        all_best_prizeImages[12].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.GucciPerfume.ToString());
        all_best_prizeImages[13].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.GetSingleReward, Reward.ChanelLipstick.ToString());
        #endregion
    }
    private IEnumerator AutoMoveBestPrize()
    {
        yield break;
        //int startIndex = 0;
        //float speed = 100;
        //float interval = 183.7044f;
        //float endLocalX = -662.7103f;
        //int maxIndex = all_best_prizeImages.Count - 1;
        //while (true)
        //{
        //    yield return null;
        //    all_best_prizeRect.localPosition -= new Vector3(speed * Time.fixedDeltaTime, 0);
        //    if (all_best_prizeRect.localPosition.x <= endLocalX)
        //    {
        //        all_best_prizeRect.localPosition += new Vector3(interval, 0);
        //        all_best_prizeImages[startIndex].transform.SetAsLastSibling();
        //        startIndex++;
        //        if (startIndex > maxIndex)
        //            startIndex = 0;
        //    }
        //}
    }
    private void OnAdButtonClick()
    {
        AdsManager.ShowRewarded(OnAdCallback);
    }
    private void OnAdCallback()
    {
        GameManager.Instance.AddReward(Reward.Key, 3);
        GameManager.Instance.AddGetMorekeysTimes();
        ResetKeys();
        adButton.gameObject.SetActive(false);
        nothanksButton.gameObject.SetActive(false);
    }
    public void OnBoxClickWhenNoKey()
    {
        GameManager.Instance.AddReward(Reward.Key, 3);
        ResetKeys();
        adButton.gameObject.SetActive(false);
        nothanksButton.gameObject.SetActive(false);
    }
    private void OnContinueButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnNothanksButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    public bool OnBoxClick()
    {
        if (SaveManager.SaveData.key > 0)
        {
            GameManager.Instance.AddReward(Reward.Key, -1);
            int currrentKey = SaveManager.SaveData.key;
            all_keysGo[currrentKey].gameObject.SetActive(false);
            if (CheckBoxAllOpen())
            {
                continueButton.gameObject.SetActive(true);
                nothanksButton.gameObject.SetActive(false);
                all_key_parentGo.SetActive(false);
            }
            else
            {
                continueButton.gameObject.SetActive(false);
                if (currrentKey == 0)
                {
                    if (SaveManager.SaveData.getMoreKeysTimes < 0)
                    {
                        ad_iconRect.gameObject.SetActive(false);
                        ad_textRect.localPosition = new Vector3(0, ad_textRect.localPosition.y);
                        nothanksButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        ad_iconRect.gameObject.SetActive(true);
                        ToolManager.SetMiddleAnchor(ad_iconRect, ad_textRect);
                        nothanksButton.gameObject.SetActive(true);
                    }
                    adButton.gameObject.SetActive(true);
                    all_key_parentGo.SetActive(false);
                }
                else
                {
                    adButton.gameObject.SetActive(false);
                    nothanksButton.gameObject.SetActive(false);
                    all_key_parentGo.SetActive(true);
                }
            }
            return true;
        }
        else
            return false;
    }
    public void RefreshState()
    {
        if (CheckBoxAllOpen())
        {
            continueButton.gameObject.SetActive(true);
            nothanksButton.gameObject.SetActive(false);
            all_key_parentGo.SetActive(false);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
            if (SaveManager.SaveData.key == 0)
            {
                if (SaveManager.SaveData.getMoreKeysTimes < 0)
                {
                    ad_iconRect.gameObject.SetActive(false);
                    ad_textRect.localPosition = new Vector3(0, ad_textRect.localPosition.y);
                    nothanksButton.gameObject.SetActive(false);
                }
                else
                {
                    ad_iconRect.gameObject.SetActive(true);
                    ToolManager.SetMiddleAnchor(ad_iconRect, ad_textRect);
                    nothanksButton.gameObject.SetActive(true);
                }
                adButton.gameObject.SetActive(true);
                all_key_parentGo.SetActive(false);
            }
            else
            {
                adButton.gameObject.SetActive(false);
                nothanksButton.gameObject.SetActive(false);
                all_key_parentGo.SetActive(true);
            }
        }
    }
    private void ResetKeys()
    {
        all_key_parentGo.SetActive(true);
        foreach (var key in all_keysGo)
            key.SetActive(true);
    }
    private bool CheckBoxAllOpen()
    {
        int closeCount = 0;
        foreach (var box in allBoxItems)
            if (!box.GetBoxOpenState())
                closeCount++;
        return closeCount == 1;
    }
    int sourcePanelIndex = -1;
    public void SendPanelShowArgs(params int[] args)
    {
        sourcePanelIndex = args[0];
        StopCoroutine("AutoMoveBestPrize");
        StartCoroutine("AutoMoveBestPrize");
        foreach (var box in allBoxItems)
            box.Init(this);
        ResetKeys();
        adButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        nothanksButton.gameObject.SetActive(false);
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel((int)PopPanel.DrawBoxPanel, sourcePanelIndex);
        if (UIManager.PanelShow(BasePanel.GamePanel))
            UIManager.AfterGetBingoWheelReward();
    }
    public int GetOpenBoxTime()
    {
        int times = 0;
        foreach (var box in allBoxItems)
            if (box.GetBoxOpenState())
                times++;
        return times;
    }
}
