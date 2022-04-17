using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pop_DrawPhoneHelpPanel : MonoBehaviour
{
    public RectTransform titleRect;
    public Button closeButton;
    public RectTransform maskRect;
    public Button contractButton;
    private void Awake()
    {
        closeButton.AddClickEvent(OnCloseButtonClick);
        contractButton.AddClickEvent(OnContractButtonClick);
        if (GameManager.IsBigScreen)
        {
            titleRect.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
            closeButton.transform.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
            maskRect.localPosition -= new Vector3(0, GameManager.TopMoveDownOffset);
            maskRect.sizeDelta += new Vector2(0, 1920 * (GameManager.ExpandCoe - 1) - GameManager.TopMoveDownOffset);
        }
    }
    private void OnCloseButtonClick()
    {
        UIManager.HidePanel(gameObject);
    }
    private void OnContractButtonClick()
    {

    }
}
