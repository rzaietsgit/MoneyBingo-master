using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiftItem : MonoBehaviour
{
    public Image iconImage;
    public Text nameText;
    public Slider progressSlider;
    public Text progressText;
    private int target = 0;
    public void Init(Reward gift,int target)
    {
        iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.DrawPhone, "Gift_" + gift);
        nameText.text = ConfigManager.GetRewardDescription(gift);
        this.target = target;
    }
    public void SetProgress(int current)
    {
        if (target == 0)
            Debug.LogError("target is 0");
        else
        {
            progressSlider.value = (float)current / target;
            progressText.text = current + "/" + target;
        }
    }
}
