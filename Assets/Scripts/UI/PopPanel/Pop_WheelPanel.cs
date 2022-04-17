using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pop_WheelPanel : MonoBehaviour,IUIMessage
{
    public WheelOne wheelOne;
    public RectTransform wheelRect;
    public Animator animator;
    const int WheelMustAllocationNum = 12;
    private List<Vector2Int> wheelAllocation;
    private void Awake()
    {
        wheelAllocation = ConfigManager.GetWheelRewardAllocation();
        AutoGenerateWheelIcon();
    }
    private void AutoGenerateWheelIcon()
    {
        int count = wheelAllocation.Count;
        float anglePerReward = 360f / count;
        if (count != WheelMustAllocationNum)
            Debug.LogError("wheel config error");
        else
        {
            for (int i = 0; i < count; i++)
            {
                WheelOne one;
                if (i == 0)
                    one = wheelOne;
                else
                    one = Instantiate(wheelOne.gameObject, wheelOne.transform.parent).GetComponent<WheelOne>();
                one.Init((Reward)wheelAllocation[i].x, wheelAllocation[i].y);
                one.transform.eulerAngles = new Vector3(0, 0, -i * anglePerReward);
            }
        }
    }
    int willGetRewardType = 0;
    int willGetRewardNum = 0;
    int willGetRewardTypeMultiple = 1;
    bool willGetRewardNeedRandomMultiple = false;
    public void ChangeWheelBase()
    {
        GameManager.Instance.TriggerTask(TaskType.PlayWheel, 1);
        GameManager.Instance.AddPlayWheelTime();
        int panelIndex = ConfigManager.GetRandomWheelReward(out willGetRewardTypeMultiple, out willGetRewardNeedRandomMultiple);
        willGetRewardType =wheelAllocation[panelIndex].x;
        willGetRewardNum = wheelAllocation[panelIndex].y;
        wheelRect.localEulerAngles = new Vector3(0, 0, (360f / wheelAllocation.Count) * panelIndex);
        animator.SetBool("Spin", false);
    }
    public void AfterRotateAnimation()
    {
        if (willGetRewardType != (int)Reward.Empty)
        {
            if (willGetRewardNeedRandomMultiple)
                UIManager.ShowPanel(PopPanel.SlotsPanel, (int)PopPanel.WheelPanel, willGetRewardType, willGetRewardNum, willGetRewardTypeMultiple);
            else
                UIManager.ShowPanel(PopPanel.GetSingleRewardPanel, (int)PopPanel.WheelPanel, willGetRewardType, willGetRewardNum);
        }
        else
        {
            UIManager.ShowPanel(PopPanel.MoreSpinPanel);
        }
    }
    private void PlayHideAnimation()
    {
        CardGenerate.BingoCount--;
        if (CardGenerate.BingoCount > 0)
        {
            animator.Play(isZero ? "WheelRotate" : "WheelRotate 0");
            isZero = !isZero;
        }
        else
            animator.SetBool("Show", false);
    }
    public void AfterHideAnimation()
    {
        UIManager.HidePanel(gameObject);
        if (SaveManager.SaveData.key >= 3)
            UIManager.ShowPanel(PopPanel.DrawBoxPanel, (int)BasePanel.GamePanel);
        else
        {
            if (UIManager.PanelShow(BasePanel.GamePanel))
                UIManager.AfterGetBingoWheelReward();
        }
    }
    bool isZero = false;
    public void SendPanelShowArgs(int[] args)
    {
        animator.SetBool("Show", true);
        isZero = false;
    }

    public void SendArgs(int sourcePanelIndex, int[] args)
    {
        if (sourcePanelIndex == (int)PopPanel.SlotsPanel)
            PlayHideAnimation();
        if (sourcePanelIndex == (int)PopPanel.GetSingleRewardPanel)
            PlayHideAnimation();
        if (sourcePanelIndex == (int)PopPanel.DrawBoxPanel)
            PlayHideAnimation();
        if (sourcePanelIndex == (int)PopPanel.MoreSpinPanel)
        {
            if (args[0] == 1)
                CardGenerate.BingoCount++;
            PlayHideAnimation();
        }
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.WheelPanel, BasePanel.Menu);
        UIManager.SendMessageToPanel(PopPanel.WheelPanel, BasePanel.GamePanel);
    }
}
