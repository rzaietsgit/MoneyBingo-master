using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIMessage
{
    void SendPanelShowArgs(params int[] args);
    void SendArgs(int sourcePanelIndex, params int[] args);
    void TriggerPanelHideEvent();
    //void SendArgs(params string[] args);
}
