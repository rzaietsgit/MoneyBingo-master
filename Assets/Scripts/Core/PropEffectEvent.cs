using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropEffectEvent : MonoBehaviour
{
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
