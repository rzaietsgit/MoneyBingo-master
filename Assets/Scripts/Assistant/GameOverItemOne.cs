using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverItemOne : MonoBehaviour
{
    public Image iconImage;
    public Text numText;
    public void Init(Reward reward,int num)
    {
        iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.BackToMain, reward.ToString());
        switch (reward)
        {
            case Reward.Cash:
                numText.text = "$" + num.GetCashShowString();
                break;
            default:
                numText.text = num.GetTokenShowString();
                break;
        }
    }
}
