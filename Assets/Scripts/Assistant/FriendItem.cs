using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FriendItem : MonoBehaviour
{
    public Image head_iconImage;
    public Image starImage;
    public Text nameText;
    public Text dateText;
    public Text levelText;
    public void Init(int head_icon_id, int distance, string name, string date, int level, int sumpt)
    {
        head_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, "head_" + head_icon_id);
        starImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, distance == 1 ? "direct_friend" : "indirect_friend");
        nameText.text = name;
        dateText.text = "Lv." + level;
        levelText.text = sumpt + " PT";
        starImage.SetNativeSize();
    }
}
