using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_TaskPanel : MonoBehaviour, IUIMessage
{
    public Button closeButton;
    public TaskItem single_taskItem;
    private readonly List<TaskItem> all_taskItems = new List<TaskItem>();
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        all_taskItems.Add(single_taskItem);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void SendPanelShowArgs(params int[] args)
    {
        AudioManager.PlayOneShot(AudioPlayArea.WindowShow);
        foreach (var item in all_taskItems)
            item.gameObject.SetActive(false);
        List<string> descriptions = ConfigManager.GetTaskShowContent(out List<Data7Int> otherData);
        int showTaskCount = descriptions.Count;
        for(int i = 0; i < showTaskCount; i++)
        {
            if (i > all_taskItems.Count - 1)
                all_taskItems.Add(Instantiate(single_taskItem.gameObject, single_taskItem.transform.parent).GetComponent<TaskItem>());
            Data7Int otherThing = otherData[i];
            all_taskItems[i].gameObject.SetActive(true);
            all_taskItems[i].Init(descriptions[i], otherThing.data1, (Reward)otherThing.data2, otherThing.data3, otherThing.data4, otherThing.data5, otherThing.data6, otherThing.data7 == 1);
        }
    }

    public void TriggerPanelHideEvent()
    {
        UIManager.SendMessageToPanel(PopPanel.TaskPanel, BasePanel.Menu);
    }
}
