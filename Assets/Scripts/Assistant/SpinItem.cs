using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinItem : MonoBehaviour
{
    public Image bgImage;
    public Image iconImage;
    public Text nameText;
    public GameObject claimGo;
    public Animator claimAniamtor;
    public void Init(Reward reward, bool hasClaim)
    {
        switch (reward)
        {
            case Reward.ManyCash:
            case Reward.Cash:
                iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.DrawPhone, "Cash");
                nameText.text = "Lucky Money";
                break;
            case Reward.PlayBingoTicket:
                iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.DrawPhone, "PlayBingoTicket");
                nameText.text = "Card*" + ConfigManager.GetDrawPhoneSpinItemReawrdNum(Reward.PlayBingoTicket);
                break;
            default:
                iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.DrawPhone, "Fragment_" + reward);
                nameText.text = ConfigManager.GetRewardDescription(reward);
                break;
        }
        claimGo.SetActive(hasClaim);
        bgImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.DrawPhone, hasClaim ? "spin_item_base_select" : "spin_item_base");
    }
    public void SetBg(bool isSelect)
    {
        bgImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.DrawPhone, isSelect ? "spin_item_base_select" : "spin_item_base");
    }
    public void Claim()
    {
        claimGo.SetActive(true);
        claimAniamtor.Play("Claim");
    }
}
