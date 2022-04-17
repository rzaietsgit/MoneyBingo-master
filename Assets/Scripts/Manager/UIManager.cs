using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;

public class UIManager
{
    private static Transform MenuRoot;
    private static Transform BasePanelRoot;
    private static Transform PopPanelRoot;
    public UIManager(Transform menuRoot,Transform basePanelRoot, Transform popPanelRoot)
    {
        MenuRoot = menuRoot;
        BasePanelRoot = basePanelRoot;
        PopPanelRoot = popPanelRoot;
    }
    private static readonly Dictionary<int, string> panelPath = new Dictionary<int, string>()
    {
        {(int)BasePanel.Menu,"Prefabs/Menu" },

        {(int)BasePanel.MainPanel,"Prefabs/BasePanel/MainPanel" },
        {(int)BasePanel.GamePanel,"Prefabs/BasePanel/GamePanel" },

        {(int)PopPanel.DrawPhonePanel,"Prefabs/PopPanel/DrawPhonePanel" },
        {(int)PopPanel.WheelPanel,"Prefabs/PopPanel/WheelPanel" },
        {(int)PopPanel.SlotsPanel,"Prefabs/PopPanel/SlotsPanel" },
        {(int)PopPanel.GetSingleRewardPanel,"Prefabs/PopPanel/GetSingleRewardPanel" },
        {(int)PopPanel.FirstGetExtraRewardPanel,"Prefabs/PopPanel/FirstGetExtraRewardPanel" },
        {(int)PopPanel.LoadingPanel,"Prefabs/PopPanel/LoadingPanel" },
        {(int)PopPanel.BackToMainPanel,"Prefabs/PopPanel/BackMainPanel" },
        {(int)PopPanel.GameOverPanel,"Prefabs/PopPanel/GameOverPanel" },
        {(int)PopPanel.MoreSpinPanel,"Prefabs/PopPanel/MoreSpinPanel" },
        {(int)PopPanel.SettingPanel,"Prefabs/PopPanel/SettingPanel" },
        {(int)PopPanel.HelpPanel,"Prefabs/PopPanel/HelpPanel" },
        {(int)PopPanel.TaskPanel,"Prefabs/PopPanel/TaskPanel" },
        {(int)PopPanel.DrawPhoneHelpPanel,"Prefabs/PopPanel/DrawPhoneHelpPanel" },
        {(int)PopPanel.CashoutPanel,"Prefabs/PopPanel/CashoutPanel" },
        {(int)PopPanel.CashoutHelpPanel,"Prefabs/PopPanel/CashoutHelpPanel" },
        {(int)PopPanel.Friend,"Prefabs/PopPanel/Friend" },
        {(int)PopPanel.FriendList,"Prefabs/PopPanel/FriendList" },
        {(int)PopPanel.FriendInviteOk,"Prefabs/PopPanel/FriendInviteOk" },
        {(int)PopPanel.FriendHelpPanel,"Prefabs/PopPanel/FriendHelpPanel" },
        {(int)PopPanel.GetMoreBingoCardsPanel,"Prefabs/PopPanel/GetMoreBingoCardsPanel" },
        {(int)PopPanel.GetNewPlayerRewardPanel,"Prefabs/PopPanel/GetNewPlayerRewardPanel" },
        {(int)PopPanel.GuidePanel,"Prefabs/PopPanel/GuidePanel" },
        {(int)PopPanel.DrawBoxPanel,"Prefabs/PopPanel/DrawBoxPanel" },
        {(int)PopPanel.EnterCashoutTaskPanel,"Prefabs/PopPanel/EnterCashoutTaskPanel" },
        {(int)PopPanel.MoneyBoxPanel,"Prefabs/PopPanel/MoneyBoxPanel" },
        {(int)PopPanel.PigHelpPanel,"Prefabs/PopPanel/PigHelpPanel" },
        {(int)PopPanel.GetMoreKeyPanel,"Prefabs/PopPanel/GetMoreKeyPanel" },
        {(int)PopPanel.LockGiveawaysPanel,"Prefabs/PopPanel/LockGiveawaysPanel" },
    };
    private static readonly Dictionary<int, GameObject> panelLoadedResource = new Dictionary<int, GameObject>();
    private static readonly Dictionary<int, GameObject> panelGo = new Dictionary<int, GameObject>();
    public static void ShowPanel(PopPanel popPanel,params int[] args)
    {
        int panelIndex = (int)popPanel;
        ShowPanel(panelIndex, args, true);
    }
    private static void ShowMenuPanel(int[] args)
    {
        int panelIndex = (int)BasePanel.Menu;
        if (panelGo.ContainsKey(panelIndex))
        {
            panelGo[panelIndex].transform.SetAsLastSibling();
            panelGo[panelIndex].SetActive(true);
            IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
            if (messageAgent != null)
                messageAgent.SendPanelShowArgs(args);
        }
        else
        {
            if (panelLoadedResource.ContainsKey(panelIndex))
            {
                panelGo.Add(panelIndex, GameObject.Instantiate(panelLoadedResource[panelIndex],MenuRoot));
                panelGo[panelIndex].transform.SetAsLastSibling();
                IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
                if (messageAgent != null)
                    messageAgent.SendPanelShowArgs(args);
            }
            else
            {
                if (panelPath.ContainsKey(panelIndex))
                {
                    panelLoadedResource.Add(panelIndex, Resources.Load<GameObject>(panelPath[panelIndex]));
                    panelGo.Add(panelIndex, GameObject.Instantiate(panelLoadedResource[panelIndex], MenuRoot));
                    panelGo[panelIndex].transform.SetAsLastSibling();
                    IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
                    if (messageAgent != null)
                        messageAgent.SendPanelShowArgs(args);
                }
                else
                    Debug.LogError("no config panel " + panelIndex);
            }
        }
    }
    public static void ShowPanel(BasePanel basePanel,params int[] args)
    {
        if (basePanel == BasePanel.Menu)
            ShowMenuPanel(args);
        else
        {
            int panelIndex = (int)basePanel;
            ShowPanel(panelIndex, args, false);
        }
    }
    private static void ShowPanel(int panelIndex, int[] args,bool isPop)
    {
        if (panelIndex == (int)PopPanel.CashoutPanel)
        {
            if (args[1] == 1 && !SaveManager.SaveData.hasUnlockCashout)
            {
                panelIndex = (int)PopPanel.EnterCashoutTaskPanel;
                args = new int[1] { (int)BasePanel.System };
            }
        }
        if (panelGo.ContainsKey(panelIndex))
        {
            panelGo[panelIndex].transform.SetAsLastSibling();
            panelGo[panelIndex].SetActive(true);
            IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
            if (messageAgent != null)
                messageAgent.SendPanelShowArgs(args);
        }
        else
        {
            if (panelLoadedResource.ContainsKey(panelIndex))
            {
                panelGo.Add(panelIndex, GameObject.Instantiate(panelLoadedResource[panelIndex], isPop ? PopPanelRoot : BasePanelRoot));
                panelGo[panelIndex].transform.SetAsLastSibling();
                IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
                if (messageAgent != null)
                    messageAgent.SendPanelShowArgs(args);
            }
            else
            {
                if (panelPath.ContainsKey(panelIndex))
                {
                    panelLoadedResource.Add(panelIndex, Resources.Load<GameObject>(panelPath[panelIndex]));
                    panelGo.Add(panelIndex, GameObject.Instantiate(panelLoadedResource[panelIndex], isPop ? PopPanelRoot : BasePanelRoot));
                    panelGo[panelIndex].transform.SetAsLastSibling();
                    IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
                    if (messageAgent != null)
                        messageAgent.SendPanelShowArgs(args);
                }
                else
                    Debug.LogError("no config panel " + panelIndex);
            }
        }
    }
    public static void HidePanel(PopPanel popPanel)
    {
        int panelIndex = (int)popPanel;
        if (panelGo.ContainsKey(panelIndex))
        {
            panelGo[panelIndex].SetActive(false);
            IUIMessage messageAgent = panelGo[panelIndex].GetComponent<IUIMessage>();
            if (messageAgent != null)
                messageAgent.TriggerPanelHideEvent();
        }
        else
            Debug.LogError("no instantiate panel");
    }
    public static void HidePanel(GameObject panel)
    {
        panel.SetActive(false);
        IUIMessage messageAgent = panel.GetComponent<IUIMessage>();
        if (messageAgent != null)
            messageAgent.TriggerPanelHideEvent();
    }
    public static void AfterGetBingoWheelReward()
    {
        CardGenerate.Instance.AfterGetBingoWheelReward();
    }
    public static void AfterFirstGetExtraReward()
    {
        CardGenerate.Instance.AfterCloseFirstGetExtraReward();
    }
    public static void SendMessageToPanel(PopPanel fromPopPanel, PopPanel targetPopPanel,params int[] args)
    {
        SendMessageToPanel((int)fromPopPanel, (int)targetPopPanel, args);
    }
    public static void SendMessageToPanel(BasePanel fromBasePanel,BasePanel targetBasePanel,params int[] args)
    {
        SendMessageToPanel((int)fromBasePanel, (int)targetBasePanel, args);
    }
    public static void SendMessageToPanel(PopPanel fromPopPanel, BasePanel targetBasePanel, params int[] args)
    {
        SendMessageToPanel((int)fromPopPanel, (int)targetBasePanel, args);
    }
    public static void SendMessageToPanel(BasePanel fromBasePanel, PopPanel targetPopPanel, params int[] args)
    {
        SendMessageToPanel((int)fromBasePanel, (int)targetPopPanel, args);
    }
    public static void SendMessageToPanel(int fromPanelIndex,int targetPanelIndex,params int[] args)
    {
        if (panelGo.ContainsKey(targetPanelIndex))
        {
            IUIMessage messageAgent = panelGo[targetPanelIndex].GetComponent<IUIMessage>();
            if (messageAgent != null)
                messageAgent.SendArgs(fromPanelIndex, args);
            else
                Debug.LogWarning("target panel " + targetPanelIndex + " have't add listener.");
        }
        else
            Debug.LogWarning("target panel have'n existed.");
    }
    public static bool PanelExist(BasePanel basePanel)
    {
        return panelGo.ContainsKey((int)basePanel);
    }
    public static bool PanelExist(PopPanel popPanel)
    {
        return panelGo.ContainsKey((int)popPanel);
    }
    public static bool PanelShow(PopPanel popPanel)
    {
        int panelIndex = (int)popPanel;
        if (panelGo.ContainsKey(panelIndex))
            return panelGo[panelIndex].activeSelf;
        else
            return false;
    }
    public static bool PanelShow(BasePanel basePanel)
    {
        int panelIndex = (int)basePanel;
        if (panelGo.ContainsKey(panelIndex))
            return panelGo[panelIndex].activeSelf;
        else
            return false;
    }
    static Transform goldTrans = null;
    static Transform cashTrans = null;
    static Transform pigTrans = null;
    static Transform ballTextTrans = null;
    static Transform[] keyTrans = new Transform[3];
    public static void SetGoldTransAndCashTrans(Transform goldTrans,Transform cashTrans)
    {
        UIManager.goldTrans = goldTrans;
        UIManager.cashTrans = cashTrans;
    }
    public static void SetPigCashFlyTarget(Transform pigCashTrans)
    {
        pigTrans = pigCashTrans;
    }
    public static void SetBallTextTransAndKeyTrans(Transform ballTrans,params Transform[] keyTrans)
    {
        ballTextTrans = ballTrans;
        UIManager.keyTrans = keyTrans;
    }
    public static void Fly_Reward(Reward reward,int rewardNum, Vector3 startWorldPos)
    {
        Vector3 targetPos;
        switch (reward)
        {
            case Reward.Gold:
                targetPos = goldTrans.position;
                break;
            case Reward.Cash:
                targetPos = cashTrans.position;
                break;
            case Reward.Ball:
                targetPos = ballTextTrans.position;
                break;
            case Reward.Cash_MoneyBox:
                targetPos = pigTrans.position;
                break;
            case Reward.Key:
                targetPos = keyTrans[SaveManager.SaveData.key - 1].position;
                break;
            default:
                return;
        }
        FlyReward.Instance.FlyTo(reward, rewardNum, startWorldPos, targetPos);
    }
    static Vector3 guideMaskWorldPos;
    static Vector2 guideMaskSize;
    public static void ShowGuidePanel(int sourcePanelIndex,Vector3 maskWorldPos,Vector2 maskSize, GuideType guideType)
    {
        guideMaskWorldPos = maskWorldPos;
        guideMaskSize = maskSize;
        ShowPanel(PopPanel.GuidePanel, sourcePanelIndex, (int)guideType);
    }
    public static Vector3 GetGuideMaskArgs(out Vector2 size)
    {
        size = guideMaskSize;
        return guideMaskWorldPos;
    }
    static int inviteOkCount = 0;
    public static void SetInviteOkCount(int count)
    {
        inviteOkCount = count;
    }
    public static int GetInviteOkCount()
    {
        return inviteOkCount;
    }
    public static bool GetWillShowWheelPanel()
    {
        if (PanelShow(BasePanel.GamePanel))
            return CardGenerate.Instance.GetWillShowWheelPanel();
        else
            return false;
    }
}
public enum BasePanel
{
    System = -1,
    Menu = 0,
    MainPanel = 1,
    GamePanel = 2,
}
public enum PopPanel
{
    DrawPhonePanel = 3,
    WheelPanel = 4,
    SlotsPanel = 5,
    GetSingleRewardPanel = 6,
    FirstGetExtraRewardPanel = 7,
    LoadingPanel = 8,
    BackToMainPanel = 9,
    GameOverPanel = 10,
    MoreSpinPanel = 11,
    SettingPanel = 12,
    HelpPanel = 13,
    TaskPanel = 14,
    DrawPhoneHelpPanel = 15,
    CashoutPanel = 16,
    CashoutHelpPanel = 17,
    Friend = 18,
    FriendList = 19,
    FriendInviteOk = 20,
    FriendHelpPanel = 21,
    GetMoreBingoCardsPanel = 22,
    GetNewPlayerRewardPanel = 23,
    GuidePanel = 24,
    DrawBoxPanel = 25,
    EnterCashoutTaskPanel = 26,
    MoneyBoxPanel = 27,
    PigHelpPanel = 28,
    GetMoreKeyPanel = 29,
    LockGiveawaysPanel = 30,
}
public enum GuideType
{
    Cashout_Cash,
    Cashout_Amazon,
    GameGuide_ClickCard,
    GameGuide_ExtraProp,
    Unlock_Pig,
    Unlock_Task,
    Unlock_Giveaways,
    Pt_Earn,
}
