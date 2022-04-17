using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Friends : MonoBehaviour,IUIMessage
{
    public RectTransform topRect;
    [Space(15)]
    public Button backButton;
    public Button helpButton;
    public Button cashoutButton;
    [Space(15)]
    public RectTransform midRect;
    public Button bannerButton;
    public Button myfriendsButton;
    public Image friend_headImage1;
    public Image friend_headImage2;
    public Image friend_headImage3;
    [Space(15)]
    public Button inviteButton;
    public Image invite_reward_iconImage;
    [Space(15)]
    public GameObject lastdayGo;
    public GameObject nofriend_tipGo;
    public FriendInviteRecordItem single_invite_record_item;
    private List<FriendInviteRecordItem> all_invite_friend_items = new List<FriendInviteRecordItem>();
    [Space(15)]
    public RectTransform viewport;
    private void Awake()
    {
        backButton.AddClickEvent(OnBackButtonClick);
        helpButton.AddClickEvent(OnHelpButtonClick);
        cashoutButton.AddClickEvent(OnCashoutButtonClick);
        inviteButton.AddClickEvent(OnInviteButtonClick);
        myfriendsButton.AddClickEvent(OnMyfriendsButtonClick);
        bannerButton.AddClickEvent(OnBannerClick);
        all_invite_friend_items.Add(single_invite_record_item);
        if (GameManager.IsBigScreen)
        {
            topRect.sizeDelta = new Vector2(topRect.sizeDelta.x, topRect.sizeDelta.y + GameManager.TopMoveDownOffset);
            viewport.sizeDelta += new Vector2(0, 1920 * (GameManager.ExpandCoe - 1) - GameManager.TopMoveDownOffset);
        }
        bannerButton.gameObject.SetActive(false);
        float bannerHeight = bannerButton.GetComponent<RectTransform>().rect.height;
        midRect.sizeDelta -= new Vector2(0, bannerHeight);
        viewport.sizeDelta += new Vector2(0, bannerHeight);
        cashoutButton.gameObject.SetActive(true);
#if UNITY_IOS
        helpButton.gameObject.SetActive(true);
#endif
    }
    #region ios share
    public void Init()
    {
        GJCNativeShare.Instance.onShareSuccess = OnShareSuccess;
        GJCNativeShare.Instance.onShareCancel = OnShareCancel;
    }
    void OnShareSuccess(string platform)
    {
        //...your code
    }
    void OnShareCancel(string platform)
    {
        //...your code
    }
    #endregion
    private void OnBackButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnHelpButtonClick()
    {
        UIManager.ShowPanel(PopPanel.FriendHelpPanel);
    }
    private void OnCashoutButtonClick()
    {
        UIManager.ShowPanel(PopPanel.CashoutPanel, (int)PopPanel.Friend, 0);
    }
    private AndroidJavaClass _aj;
    private AndroidJavaClass _AJ
    {
        get
        {
            if (_aj == null)
                _aj = new AndroidJavaClass("com.wyx.shareandcopy.Share_Copy");
            return _aj;

        }
    }
    private void OnInviteButtonClick()
    {
#if UNITY_EDITOR
        return;
#endif
        //GameManager.Instance.SendAdjustClickInviteButtonEvent(false);
#if UNITY_ANDROID
        _AJ.CallStatic("ShareString", "I earned $5 in the game, you can try it. http://aff.luckyclub.vip:8000/MoneyBingo/" + SaveManager.SaveData.inviteCode);
        return;
#endif
        GJCNativeShare.Instance.NativeShare("I earned $5 in the game, you can try it. http://aff.luckyclub.vip:8000/MoneyBingo/" + SaveManager.SaveData.inviteCode);

    }
    private void OnBannerClick()
    {

    }
    private void OnMyfriendsButtonClick()
    {
        UIManager.ShowPanel(PopPanel.FriendList);
    }
    private void BeforeShowAnimation(params int[] args)
    {
        Server.Instance.ConnectToGetFriendData(RefreshFriendList, null, null, true);
    }
    public void RefreshFriendList()
    {
        pt_numText.text = ((int)SaveManager.SaveData.friendData.user_total).GetTokenShowString() + " <size=70>PT</size>";
        int invite_people_num = SaveManager.SaveData.friendData.invite_num;
        int invite_people_receive = SaveManager.SaveData.friendData.reward_conf.invite_receive;
        myfriends_numText.text = string.Format("My friends: <color=#0596E4>{0}</color>", invite_people_num.GetTokenShowString());
        yesterday_pt_numText.text = ((int)SaveManager.SaveData.friendData.yestday_team_all).GetTokenShowString() + " <size=55>PT</size>";
        total_pt_numText.text = ((int)SaveManager.SaveData.friendData.user_total).GetTokenShowString() + " <size=55>PT</size>";

        foreach (var friend in all_invite_friend_items)
            friend.gameObject.SetActive(false);

        List<AllData_FriendData_Friend> friend_Infos = SaveManager.SaveData.friendData.two_user_list;
        int count = friend_Infos.Count;
        List<AllData_FriendData_Friend> friend_Infos_order = new List<AllData_FriendData_Friend>();
        for (int i = 0; i < count; i++)
        {
            AllData_FriendData_Friend unorder_friend_info = friend_Infos[i];
            int orderCount = friend_Infos_order.Count;
            bool hasAdd = false;
            for (int j = 0; j < orderCount; j++)
            {
                if (unorder_friend_info.yestday_doller < friend_Infos_order[j].yestday_doller)
                    continue;
                else
                {
                    friend_Infos_order.Insert(j, unorder_friend_info);
                    hasAdd = true;
                    break;
                }
            }
            if (!hasAdd)
                friend_Infos_order.Add(unorder_friend_info);
        }
        int hasPtFriendCount = 0;
        for (int i = 0; i < count; i++)
        {
            AllData_FriendData_Friend friendInfo = friend_Infos_order[i];
            if ((int)friendInfo.yestday_doller == 0)
                continue;
            hasPtFriendCount++;
            if (i > all_invite_friend_items.Count - 1)
            {
                FriendInviteRecordItem newRecordItem = Instantiate(single_invite_record_item, single_invite_record_item.transform.parent).GetComponent<FriendInviteRecordItem>();
                all_invite_friend_items.Add(newRecordItem);
            }
            all_invite_friend_items[i].gameObject.SetActive(true);
            all_invite_friend_items[i].Init(friendInfo.user_img, friendInfo.user_name, (int)friendInfo.yestday_doller, friendInfo.distance);
        }
        bool noFriend = count == 0;
        bool noPtFriend = hasPtFriendCount == 0;
        lastdayGo.SetActive(!noPtFriend);
        nofriend_tipGo.SetActive(noFriend);
        myfriendsButton.gameObject.SetActive(!noFriend);
        if (count > 0)
            friend_headImage1.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, "head_" + friend_Infos[0].user_img);
        if (count > 1)
            friend_headImage2.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, "head_" + friend_Infos[1].user_img);
        if (count > 2)
            friend_headImage3.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, "head_" + friend_Infos[2].user_img);

        int receiveTime = invite_people_receive + 1;
        if (receiveTime <= SaveManager.SaveData.friendData.reward_conf.invite_flag)
        {
            Reward lt = SaveManager.SaveData.friendData.reward_conf.lt_flag_type;
            if (lt == Reward.Empty)
                invite_reward_numText.gameObject.SetActive(false);
            else
            {
                invite_reward_numText.gameObject.SetActive(true);
                string numStr;
                switch (lt)
                {
                    case Reward.Cash:
                        invite_reward_iconImage.gameObject.SetActive(true);
                        invite_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Menu, SaveManager.SaveData.friendData.reward_conf.lt_flag_type.ToString().ToLower());
                        if (SaveManager.SaveData.isPackB)
                            numStr = "$" + SaveManager.SaveData.friendData.reward_conf.lt_flag_num.GetCashShowString();
                        else
                            numStr =SaveManager.SaveData.friendData.reward_conf.lt_flag_num.GetCashShowString();
                        break;
                    case Reward.Pt:
                        invite_reward_iconImage.gameObject.SetActive(false);
                        numStr = SaveManager.SaveData.friendData.reward_conf.lt_flag_num + "PT";
                        break;
                    default:
                        numStr = SaveManager.SaveData.friendData.reward_conf.lt_flag_num.GetTokenShowString();
                        invite_reward_iconImage.gameObject.SetActive(true);
                        invite_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Menu, SaveManager.SaveData.friendData.reward_conf.lt_flag_type.ToString().ToLower());
                        break;
                }
                invite_reward_numText.text = string.Format("Invite friends to get <color=#FF9732>{0}</color>", numStr);
            }
        }
        else
        {
            Reward gt = SaveManager.SaveData.friendData.reward_conf.gt_flag_type;
            if(gt==Reward.Empty)
                invite_reward_numText.gameObject.SetActive(false);
            else
            {
                invite_reward_numText.gameObject.SetActive(true);
                string numStr;
                switch (gt)
                {
                    case Reward.Cash:
                        invite_reward_iconImage.gameObject.SetActive(true);
                        invite_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Menu, SaveManager.SaveData.friendData.reward_conf.gt_flag_type.ToString().ToLower());
                        if (SaveManager.SaveData.isPackB)
                            numStr = "$" + SaveManager.SaveData.friendData.reward_conf.gt_flag_num.GetCashShowString();
                        else
                            numStr = SaveManager.SaveData.friendData.reward_conf.gt_flag_num.GetCashShowString();
                        break;
                    case Reward.Pt:
                        invite_reward_iconImage.gameObject.SetActive(false);
                        numStr = SaveManager.SaveData.friendData.reward_conf.gt_flag_num + "PT";
                        break;
                    default:
                        numStr = SaveManager.SaveData.friendData.reward_conf.gt_flag_num.GetTokenShowString();
                        invite_reward_iconImage.gameObject.SetActive(true);
                        invite_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Menu, SaveManager.SaveData.friendData.reward_conf.gt_flag_type.ToString().ToLower());
                        break;
                }
                invite_reward_numText.text = string.Format("Invite friends to get <color=#FF9732>{0}</color>", numStr);
            }
        }

        int not_received_invite_reward = invite_people_num - invite_people_receive;
        UIManager.SetInviteOkCount(not_received_invite_reward - 1);
        if (not_received_invite_reward > 0)
        {
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
    public void OnChangePackB()
    {
        cashoutButton.gameObject.SetActive(true);
    }

    public void SendPanelShowArgs(params int[] args)
    {
        BeforeShowAnimation(args);
    }

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
        if (sourcePanelIndex == (int)PopPanel.FriendInviteOk)
            RefreshFriendList();
    }

    public void TriggerPanelHideEvent()
    {
    }

    [Space(15)]
    public Text pt_numText;
    public Text myfriends_numText;
    public Text invite_reward_numText;
    public Text yesterday_pt_numText;
    public Text total_pt_numText;
}
