using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WheelOne : MonoBehaviour
{
    public Image iconImage;
    public Text numText;
    private RectTransform rect;
    public RectTransform Rect { get { if (rect == null) rect = GetComponent<RectTransform>();return rect; } }
    public void Init(Reward reward,int num)
    {
        iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Wheel, reward.ToString());
        if (reward == Reward.Cash)
            numText.text = "x" + num.GetWheelCashShowString();
        else if (reward == Reward.Empty)
            numText.text = "sorry";
        else
            numText.text = "x" + num;
    }
}
