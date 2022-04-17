using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{
    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(() => 
            {
                if (FB.IsInitialized)
                {
                    Debug.LogError("|Facebook| Init success!");
                    FB.ActivateApp();
                }
                else
                {
                    Debug.LogError("|Facebook| Could not init!");
                }
            });
        }
    }
}
