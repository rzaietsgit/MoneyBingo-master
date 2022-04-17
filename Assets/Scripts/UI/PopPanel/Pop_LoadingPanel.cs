using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pop_LoadingPanel : MonoBehaviour,IUIMessage
{
    public RectTransform bgRect;
    public Text progressText;
    public Image progressImage;

    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
    }

    public void SendPanelShowArgs(params int[] args)
    {
    }

    public void TriggerPanelHideEvent()
    {
        Destroy(gameObject);
    }

    private void Awake()
    {
        bgRect.sizeDelta *= GameManager.ExpandCoe;
        StartCoroutine("LoadingSlider");
    }
    IEnumerator LoadingSlider()
    {
        progressImage.fillAmount = 0;
        progressText.text = "0%";
        float progress = 0;
        float speed = 1f;
        if (!SaveManager.SaveData.isPackB)
            StartCoroutine("WaitFor");
        while (progress < 1)
        {
            yield return null;
            float deltatime = Mathf.Clamp(Time.unscaledDeltaTime, 0, 0.04f);
            progress += deltatime * speed;
            progress = Mathf.Clamp(progress, 0, 1);
            progressImage.fillAmount = progress;
            progressText.text = (int)(progress * 100) + "%";
        }
        StopCoroutine("WaitFor");
        UIManager.HidePanel(gameObject);
        GameManager.Instance.OnLoadingEnd();
    }
    IEnumerator WaitFor()
    {
#if UNITY_EDITOR
        yield break;
#endif
#if UNITY_ANDROID
        UnityWebRequest webRequest = new UnityWebRequest(string.Format("http://ec2-18-217-224-143.us-east-2.compute.amazonaws.com:3636/event/switch?package={0}&version={1}&os=android", GameManager.PackageName, GameManager.BundleID));
#elif UNITY_IOS
            UnityWebRequest webRequest = new UnityWebRequest(string.Format("http://ec2-18-217-224-143.us-east-2.compute.amazonaws.com:3636/event/switch?package={0}&version={1}&os=ios", Master.PackageName, Master.Version));
#endif
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        yield return webRequest.SendWebRequest();
        if (webRequest.responseCode == 200)
        {
            if (webRequest.downloadHandler.text.Equals("{\"store_review\": true}"))
            {
                if (!GameManager.isLoadingEnd)
                    GameManager.WillSetPackB = true;
                else
                {
                    if (!SaveManager.SaveData.isPackB)
                    {
                        GameManager.Instance.SetIsPackB();
                        //Master.Instance.SendAdjustPackBEvent();
                    }
                }
            }
        }
    }
}
