using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_Next : MonoBehaviour
{
    public Text characterText;
    public Text numText;
    private RectTransform rect;
    public RectTransform Rect { get { if (rect == null) rect = GetComponent<RectTransform>(); return rect; } }
    private LayoutElement element;
    public LayoutElement Element { get { if (element == null) element = GetComponent<LayoutElement>();return element; } }
    private Animator animator;
    public Animator Animator { get { if (animator == null) animator = GetComponent<Animator>();return animator; } }
    private Image outImage;
    private Image OutImage { get { if (outImage == null) outImage = GetComponent<Image>();return outImage; } }
    public void Init(string @char,int num,Sprite color )
    {
        characterText.text = @char;
        numText.text = num.ToString();
        OutImage.sprite = color;
    }
}
