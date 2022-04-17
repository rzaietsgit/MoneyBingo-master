using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyReward : MonoBehaviour
{
    public static FlyReward Instance;
    private void Awake()
    {
        Instance = this;
    }
    public List<Image> all_iconImages = new List<Image>();
    private Vector2 size = new Vector2(180, 110);
    private Reward rewardType;
    public void FlyTo(Reward reward,int rewardNum,Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        rewardType = reward;
        List<Sprite> sprites = new List<Sprite>();
        bool isBall = false;
        if (reward == Reward.Ball)
        {
            isBall = true;
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, "ball_1"));
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, "ball_2"));
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, "ball_3"));
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, "ball_4"));
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, "ball_5"));
        }
        else if (reward == Reward.Cash_MoneyBox)
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, "cash"));
        else
            sprites.Add(SpriteManager.GetSprite(SpriteAtlas_Name.Menu, reward.ToString().ToLower()));
        List<Vector3> targetPosList = new List<Vector3>();
        int flyCount = Mathf.Min(rewardNum, all_iconImages.Count);
        for(int i = 0; i < flyCount; i++)
        {
            Image icon = all_iconImages[i];
            if (isBall)
                icon.sprite = sprites[Random.Range(0, sprites.Count)];
            else
                icon.sprite = sprites[0];
            icon.SetNativeSize();
            icon.transform.position = startWorldPos;
            Vector3 localStart = icon.transform.localPosition;
            targetPosList.Add(new Vector3(localStart.x + Random.Range(-size.x, size.x), localStart.y + Random.Range(-size.y, size.y)));
            icon.color = Color.white;
        }
        StartCoroutine(Fly(targetPosList, targetWorldPos));
    }
    private IEnumerator Fly(List<Vector3> spreadTargetLocalPosList, Vector3 targetWorldPos)
    {
        float spreadTime = 0.2f;
        float timer = 0;
        Vector3 startLocalPos = all_iconImages[0].transform.localPosition;
        int imageCount = spreadTargetLocalPosList.Count;
        while (timer < spreadTime)
        {
            yield return null;
            timer += Time.deltaTime;
            timer = Mathf.Clamp(timer, 0, spreadTime);
            for(int i = 0; i < imageCount; i++)
            {
                all_iconImages[i].transform.localPosition = Vector3.Lerp(startLocalPos, spreadTargetLocalPosList[i], timer / spreadTime);
            }
        }
        for (int i = 0; i < imageCount; i++)
        {
            spreadTargetLocalPosList[i] = all_iconImages[i].transform.position;
        }
        float flyTime = 0.3f;
        float flyInterval = 0.05f;
        int startIndex = 0;
        int endIndex = 0;
        timer = 0;
        bool hasRefresh = false;
        bool isKey = rewardType == Reward.Key;
        while (startIndex <= imageCount - 1)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer >= (startIndex + 1) * flyInterval)
                endIndex++;
            endIndex = Mathf.Clamp(endIndex, 0, imageCount - 1);
            for(int i = startIndex; i <= endIndex; i++)
            {
                float progress = (timer - flyInterval * i) / flyTime;
                progress = Mathf.Clamp(progress, 0, 1);
                if (i == startIndex && progress >= 1)
                {
                    if(isKey&&!hasRefresh)
                        UIManager.SendMessageToPanel(BasePanel.System, BasePanel.GamePanel);
                    startIndex++;
                    progress = 1;
                    all_iconImages[i].color = Color.clear;
                }
                all_iconImages[i].transform.position = Vector3.Lerp(spreadTargetLocalPosList[i], targetWorldPos, progress);
            }
        }
    }
}
