using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_FriendHelpPanel : MonoBehaviour
{
    public Button closeButton;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
}
