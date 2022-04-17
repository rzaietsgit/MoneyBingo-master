using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskItem : MonoBehaviour
{
    public Text descriptionText;
    public Image rewardImage;
    public Text reward_numText;
    public Button completeButton;
    public Text progressText;
    private int task_id = -1;
    private Reward rewardType = Reward.Empty;
    private int rewardNum = 0;
    private int rewardMultiple = 0;
    private void Awake()
    {
        completeButton.AddClickEvent(OnCompleteButtonClick);
    }
    private void OnCompleteButtonClick()
    {
        completeButton.interactable = false;
        progressText.text = "CLAIMED";
        switch (rewardType)
        {
            case Reward.PlayBingoTicket:
            case Reward.Cash:
            case Reward.Gold:
                UIManager.ShowPanel(PopPanel.SlotsPanel, (int)PopPanel.TaskPanel, (int)rewardType, rewardNum, rewardMultiple);
                break;
            default:
                UIManager.ShowPanel(PopPanel.GetSingleRewardPanel, (int)PopPanel.TaskPanel, (int)rewardType, rewardNum);
                break;
        }
        GameManager.Instance.FinishTask(task_id, rewardType);
    }
    public void Init(string description, int taskid, Reward reward, int rewardnum,int rewardmultiple, int current, int target, bool hasGetReward)
    {
        task_id = taskid;
        descriptionText.text = description;
        rewardType = reward;
        rewardNum = rewardnum;
        rewardMultiple = rewardmultiple;
        string rewardSpriteName;
        if (reward == Reward.Gold)
        {
            string frontString = reward + "_";
            string behindString;
            if (rewardnum < 2000)
                behindString = "1";
            else if (rewardnum < 3000)
                behindString = "2";
            else
                behindString = "3";
            rewardSpriteName = frontString + behindString;
        }
        else
            rewardSpriteName = reward.ToString();
        rewardImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Task, rewardSpriteName);
        if (reward == Reward.Cash)
            reward_numText.text = "+$" + rewardnum.GetCashShowString();
        else
            reward_numText.text = "+" + rewardnum;
        completeButton.interactable = current >= target && !hasGetReward;
        if (!hasGetReward)
        {
            if (current < target)
                progressText.text = current + "/" + target;
            else
                progressText.text = "Claim";
        }
        else
            progressText.text = "CLAIMED";
    }
}
