using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
public class Server : MonoBehaviour
{
#if UNITY_ANDROID
    public const string Platform = "android";
#elif UNITY_IOS
        public const string Platform = "ios";
#endif
    public const string Bi_name = "Bingo";
    public static Server Instance;
    public CanvasGroup canvasGroup;
    public Image state_iconImage;
    public Text titleText;
    public Text tipText;
    public Button retryButton;
    public static string localCountry = "";
    static string adID = "";
    private void Awake()
    {
        Instance = this;
        GetAdID();
        retryButton.AddClickEvent(OnRetryButtonClick);
    }
    private void OnRetryButtonClick()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        ConnectToServer(RequestType, ServerResponseOkCallback, ServerResponseErrorCallback, NetworkErrorCallback, ShowConnectingWindow, Args);
    }
    static bool isConnecting = false;
    private IEnumerator WaitConnecting()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        titleText.text = "";
        tipText.text = "Connecting...";
        state_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, "loading");
        retryButton.gameObject.SetActive(false);
        while (isConnecting)
        {
            yield return null;
            state_iconImage.transform.Rotate(new Vector3(0, 0, -Time.deltaTime * 300));
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string Getidfa();
#endif
    private void GetAdID()
    {
#if UNITY_EDITOR
        adID = SystemInfo.deviceUniqueIdentifier;
#elif UNITY_ANDROID
        Application.RequestAdvertisingIdentifierAsync(
           (string advertisingId, bool trackingEnabled, string error) =>
           {
               adID = advertisingId;
           }
       );
#elif UNITY_IOS && !UNITY_EDITOR
         adID = Getidfa();
#endif
    }
    public enum Server_RequestType
    {
        FriendData,
        GetLocalCountry,
        GetUUID,
        GetInviteReward
    }
    static readonly Dictionary<Server_RequestType, string> getdata_uri_dic = new Dictionary<Server_RequestType, string>()
    {
        {Server_RequestType.FriendData,"http://aff.luckyclub.vip:8000/public_start/" },
        {Server_RequestType.GetLocalCountry,"https://a.mafiagameglobal.com/event/country/" },
        {Server_RequestType.GetUUID,"http://aff.luckyclub.vip:8000/get_random_id/" },
        {Server_RequestType.GetInviteReward,"http://aff.luckyclub.vip:8000/public_invite/" },
    };
    Server_RequestType RequestType;
    Action ServerResponseOkCallback;
    Action ServerResponseErrorCallback;
    Action NetworkErrorCallback;
    bool ShowConnectingWindow;
    string[] Args;
    private void ConnectToServer(Server_RequestType _RequestType, Action _ServerResponseOkCallback, Action _ServerResponseErrorCallback, Action _NetworkErrorCallback, bool _ShowConnectingWindow, params string[] _Args)
    {
        RequestType = _RequestType;
        ServerResponseOkCallback = _ServerResponseOkCallback;
        ServerResponseErrorCallback = _ServerResponseErrorCallback;
        NetworkErrorCallback = _NetworkErrorCallback;
        ShowConnectingWindow = _ShowConnectingWindow;
        Args = _Args;
        StartCoroutine(ConnectToServerThread(_RequestType, _ServerResponseOkCallback, _ServerResponseErrorCallback, _NetworkErrorCallback, _ShowConnectingWindow, Args));
    }
    private IEnumerator ConnectToServerThread(Server_RequestType _RequestType, Action _ServerResponseOkCallback, Action _ServerResponseErrorCallback, Action _NetworkErrorCallback, bool _ShowConnectingWindow, params string[] _Args)
    {
        List<IMultipartFormSection> iparams = new List<IMultipartFormSection>();
        if (_ShowConnectingWindow)
            OnConnectingServer();
        #region request uuid
        if (string.IsNullOrEmpty(SaveManager.SaveData.uuid))
        {
            UnityWebRequest requestUUID = UnityWebRequest.Get(getdata_uri_dic[Server_RequestType.GetUUID]);
            yield return requestUUID.SendWebRequest();
            if (requestUUID.isNetworkError || requestUUID.isHttpError)
            {
                OnConnectServerFail();
                _NetworkErrorCallback?.Invoke();
                yield break;
            }
            else
            {
                if (requestUUID.responseCode.Equals(200))
                {
                    string downText = requestUUID.downloadHandler.text;
                    GameManager.Instance.ChangeUUID(downText);
                }
                else
                {
                    OnConnectServerFail();
                    _NetworkErrorCallback?.Invoke();
                    yield break;
                }
            }
            requestUUID.Dispose();
        }
        #endregion
        iparams.Add(new MultipartFormDataSection("uuid", SaveManager.SaveData.uuid));
        if (!string.IsNullOrEmpty(adID))
        {
            GameManager.Instance.ChangeADID(adID);
            iparams.Add(new MultipartFormDataSection("device_id", adID));
        }
        iparams.Add(new MultipartFormDataSection("app_name", Bi_name));
        switch (_RequestType)
        {
            case Server_RequestType.FriendData:
                #region request country
                if (string.IsNullOrEmpty(localCountry))
                {
                    UnityWebRequest requestCountry = UnityWebRequest.Get(getdata_uri_dic[Server_RequestType.GetLocalCountry]);
                    yield return requestCountry.SendWebRequest();
                    if (requestCountry.isNetworkError || requestCountry.isHttpError)
                    {
                        OnConnectServerFail();
                        _NetworkErrorCallback?.Invoke();
                        yield break;
                    }
                    else
                    {
                        string downText = requestCountry.downloadHandler.text;
                        LocalCountryData countryData = JsonMapper.ToObject<LocalCountryData>(downText);
                        localCountry = countryData.country.ToLower();
                    }
                    requestCountry.Dispose();
                }
                #endregion
                iparams.Add(new MultipartFormDataSection("country", localCountry));
                iparams.Add(new MultipartFormDataSection("ad_ios", Platform));
                break;
        }
        UnityWebRequest www = UnityWebRequest.Post(getdata_uri_dic[_RequestType], iparams);
        yield return www.SendWebRequest();
        isConnecting = false;
        if (www.isNetworkError || www.isHttpError)
        {
            OnConnectServerFail();
            _NetworkErrorCallback?.Invoke();
            yield break;
        }
        else
        {
            OnConnectServerSuccess();
            string downText = www.downloadHandler.text;
            www.Dispose();
            if (int.TryParse(downText, out int errorcode) && errorcode < 0)
            {
                ShowConnectErrorTip(downText);
                _ServerResponseErrorCallback?.Invoke();
            }
            else
            {
                switch (RequestType)
                {
                    case Server_RequestType.FriendData:
                        ReceiveFriendData friendData = JsonMapper.ToObject<ReceiveFriendData>(downText);
                        GameManager.Instance.SaveFriendData(friendData.invite_data, friendData.self_code, friendData.uuid);
                        break;
                    case Server_RequestType.GetInviteReward:
                        ReceiveGetInviteRewardData inviteRewardData = JsonMapper.ToObject<ReceiveGetInviteRewardData>(downText);
                        GameManager.Instance.SaveInviteReward(inviteRewardData.user_total, inviteRewardData.invite_reward_num);
                        break;
                }
                _ServerResponseOkCallback?.Invoke();
            }
        }
    }
    public void ConnectToGetFriendData(Action _ServerResponseOkCallback, Action _ServerResponseErrorCallback, Action _NetworkErrorCallback, bool _ShowConnectingWindow)
    {
        ConnectToServer(Server_RequestType.FriendData, _ServerResponseOkCallback, _ServerResponseErrorCallback, _NetworkErrorCallback, _ShowConnectingWindow);
    }
    public void ConnectToGetInviteReward(Action _ServerResponseOkCallback, Action _ServerResponseErrorCallback, Action _NetworkErrorCallback, bool _ShowConnectingWindow)
    {
        ConnectToServer(Server_RequestType.GetInviteReward, _ServerResponseOkCallback, _ServerResponseErrorCallback, _NetworkErrorCallback, _ShowConnectingWindow);
    }
    private void ShowConnectErrorTip(string errorCode)
    {
        string errorString;
        errorString = "Error code :" + errorCode;
        GameManager.Instance.ShowTip(errorString, 3);
    }
    #region connecting state
    public void OnConnectServerFail()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        titleText.text = "ERROR";
        tipText.text = "Network connection is unavailable, please check network settings.";
        state_iconImage.transform.rotation = Quaternion.identity;
        state_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Friend, "nonet");
        retryButton.GetComponentInChildren<Text>().text = "RETRY";
        retryButton.gameObject.SetActive(true);
        StopCoroutine("WaitConnecting");
    }
    public void OnConnectServerSuccess()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        StopCoroutine("WaitConnecting");
    }
    public void OnConnectingServer()
    {
        isConnecting = true;
        StartCoroutine("WaitConnecting");
    }
    #endregion
}
public struct LocalCountryData
{
    public string ip;
    public string country;
}
public struct ReceiveFriendData
{
    public AllData_FriendData invite_data;
    public string self_code;
    public string uuid;
}
[Serializable]
public struct AllData_FriendData
{
    public int yestday_team_all;
    public int user_title;
    public int invite_num;
    public int invite_reward_num;
    public bool up_user;
    public string up_user_id;
    public double user_total;
    public AllData_FriendData_InviteRewardConfig reward_conf;
    public List<AllData_FriendData_Friend> two_user_list;
}
[Serializable]
public struct AllData_FriendData_InviteRewardConfig
{
    public int invite_receive;//邀请奖励领取的次数
    public int invite_flag;//邀请奖励分界人数, <=为小于部分，>为大于部分
    public Reward lt_flag_type;//小于部分
    public int lt_flag_num;
    public Reward gt_flag_type;//大于部分
    public int gt_flag_num;
}
[Serializable]
public struct AllData_FriendData_Friend
{
    public int user_img;
    public double yestday_doller;
    public int distance;//1直接好友，0间接好友
    public string user_name;
    public int user_level;
    public string user_time;
    public double sum_coin;//累计收益
}
public struct ReceiveGetInviteRewardData
{
    public double user_total;
    public int invite_reward_num;
}
