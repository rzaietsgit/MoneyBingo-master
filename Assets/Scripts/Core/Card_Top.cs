using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_Top : MonoBehaviour
{
    public Image bgImage;
    public Text characterText;
    private RectTransform rect;
    public RectTransform Rect { get { if (rect == null) rect = GetComponent<RectTransform>(); return rect; } }
    public void Init(string @char,Color color )
    {
        characterText.text = @char;
        bgImage.color = color;
    }
}
