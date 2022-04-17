using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendList : MonoBehaviour,IUIMessage
{
    public Button backButton;
    public Button direct_friendButton;
    public GameObject direct_friend_underline;
    public Button indirect_friendButton;
    public GameObject indirect_friend_underline;
    public Text list_titleText;
    public FriendItem single_friend_item;
    private List<FriendItem> all_friends = new List<FriendItem>();
    public RectTransform allRect;
    public RectTransform bottomRect;
    public RectTransform viewport;
    private void Awake()
    {
        backButton.AddClickEvent(OnBackButtonClick);
        direct_friendButton.AddClickEvent(OnDirectFriendButtonClick);
        indirect_friendButton.AddClickEvent(OnIndirectFriendButtonClick);
        if (GameManager.IsBigScreen)
        {
            allRect.sizeDelta = new Vector2(allRect.sizeDelta.x, allRect.sizeDelta.y + GameManager.TopMoveDownOffset);
            bottomRect.sizeDelta += new Vector2(0, 1920 * (GameManager.ExpandCoe - 1) - GameManager.TopMoveDownOffset);
            viewport.sizeDelta += new Vector2(0, 1920 * (GameManager.ExpandCoe - 1) - GameManager.TopMoveDownOffset);
        }
        all_friends.Add(single_friend_item);
    }
    private void OnBackButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnDirectFriendButtonClick()
    {
        SetFriendListShow(true);
    }
    private void OnIndirectFriendButtonClick()
    {
        SetFriendListShow(false);
    }
    static readonly List<AllData_FriendData_Friend> direct_friend_list = new List<AllData_FriendData_Friend>();
    static readonly List<AllData_FriendData_Friend> indirect_friend_list = new List<AllData_FriendData_Friend>();
    private void BeforeShowAnimation(params int[] args)
    {
        direct_friend_list.Clear();
        indirect_friend_list.Clear();
        List<AllData_FriendData_Friend> all_friend_list = SaveManager.SaveData.friendData.two_user_list;
        int friendCount = all_friend_list.Count;
        for (int i = 0; i < friendCount; i++)
        {
            if (all_friend_list[i].distance == 1)
                direct_friend_list.Add(all_friend_list[i]);
            else
                indirect_friend_list.Add(all_friend_list[i]);
        }
        direct_friend_list.Sort(SortFunc);
        indirect_friend_list.Sort(SortFunc);
        SetFriendListShow(true);
    }
    private int SortFunc(AllData_FriendData_Friend a, AllData_FriendData_Friend b)
    {
        if (a.sum_coin > b.sum_coin) return -1;
        if (a.sum_coin == b.sum_coin) return 0;
        return 1;
    }
    private void SetFriendListShow(bool isDirect)
    {
        direct_friend_underline.SetActive(isDirect);
        indirect_friend_underline.SetActive(!isDirect);
        List<AllData_FriendData_Friend> willBeShow = isDirect ? direct_friend_list : indirect_friend_list;
        list_titleText.text = isDirect ? "Friends you invited" : "Friends invited by your friends";
        foreach (var friend in all_friends)
            friend.gameObject.SetActive(false);
        int willbeShowCount = willBeShow.Count;
        for (int i = 0; i < willbeShowCount; i++)
        {
            if (i > all_friends.Count - 1)
            {
                FriendItem newFriend = Instantiate(single_friend_item, single_friend_item.transform.parent).GetComponent<FriendItem>();
                all_friends.Add(newFriend);
            }
            AllData_FriendData_Friend frinedInfo = willBeShow[i];
            all_friends[i].gameObject.SetActive(true);
            all_friends[i].Init(frinedInfo.user_img, frinedInfo.distance, frinedInfo.user_name, frinedInfo.user_time, frinedInfo.user_level, (int)frinedInfo.sum_coin);
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
    }
}