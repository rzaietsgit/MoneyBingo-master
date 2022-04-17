using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InviteOk : MonoBehaviour,IUIMessage
{
    public Image reward_iconImage;
    public Text reward_numText;
    public Button double_rewardButton;
    public Button single_rewardButton;
    public GameObject adGo;
    private void Awake()
    {
        double_rewardButton.AddClickEvent(OnDoubleClick);
        single_rewardButton.AddClickEvent(OnSingleClick);
    }
    private void OnDoubleClick()
    {
        GetReward();
    }
    private void GetReward()
    {
        Server.Instance.ConnectToGetInviteReward(OnGetRewardCallback, OnGetRewardErrorCallback, null, true);
    }
    private void OnGetRewardErrorCallback()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnGetRewardCallback()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnSingleClick()
    {
        GetReward();
    }
    Reward invite_ok_reward_type;
    int invite_ok_reward_num;
    public void BeforeShowAnimation(params int[] args)
    {
        invite_ok_reward_type = (Reward)args[0];
        invite_ok_reward_num = args[1];
        if (invite_ok_reward_type == Reward.Pt)
            reward_iconImage.gameObject.SetActive(false);
        else
        {
            reward_iconImage.gameObject.SetActive(true);
            reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Menu, invite_ok_reward_type.ToString().ToLower());
        }
        single_rewardButton.gameObject.SetActive(false);
        adGo.SetActive(false);
        switch (invite_ok_reward_type)
        {
            case Reward.Cash:
                reward_numText.text = "x " + invite_ok_reward_num.GetCashShowString();
                break;
            case Reward.Pt:
                reward_numText.text = "x " + invite_ok_reward_num + "PT";
                break;
            default:
                reward_numText.text = "x " + invite_ok_reward_num.GetTokenShowString();
                break;
        }
    }
    public void SendPanelShowArgs(params int[] args)
    {
        BeforeShowAnimation(args);
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.FriendInviteOk, PopPanel.Friend);
        if (UIManager.GetInviteOkCount() > 0)
        {
            int receiveTime = SaveManager.SaveData.friendData.reward_conf.invite_receive + 1;
            int not_received_invite_reward = SaveManager.SaveData.friendData.invite_num - SaveManager.SaveData.friendData.reward_conf.invite_receive;
            UIManager.SetInviteOkCount(not_received_invite_reward - 1);
            if (receiveTime <= SaveManager.SaveData.friendData.reward_conf.invite_flag)
            {
                if (SaveManager.SaveData.friendData.reward_conf.lt_flag_type != Reward.Empty)
                    UIManager.ShowPanel(PopPanel.FriendInviteOk, (int)SaveManager.SaveData.friendData.reward_conf.lt_flag_type, SaveManager.SaveData.friendData.reward_conf.lt_flag_num);
            }
            else
            {
                if (SaveManager.SaveData.friendData.reward_conf.gt_flag_type != Reward.Empty)
                    UIManager.ShowPanel(PopPanel.FriendInviteOk, (int)SaveManager.SaveData.friendData.reward_conf.gt_flag_type, SaveManager.SaveData.friendData.reward_conf.gt_flag_num);
            }
        }
    }
}
