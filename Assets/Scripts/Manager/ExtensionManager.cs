using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public static class ExtensionManager
{
    public static void AddClickEvent(this Button button, UnityAction call)
    {
        button.onClick.AddListener(call);
        button.onClick.AddListener(PlayButtonClickAudio);
    }
    private static void PlayButtonClickAudio()
    {
        AudioManager.PlayOneShot(AudioPlayArea.Button);
    }
    public static void RemoveAllClickEvent(this Button button)
    {
        button.onClick.RemoveAllListeners();
    }
    public static void RemoveClickEvent(this Button button, UnityAction call)
    {
        button.onClick.RemoveListener(call);
    }
    public static string GetTokenShowString(this int tokenNum)
    {
        string str = tokenNum.ToString();
        int length = str.Length;
        int pos = 0;
        for (int i = length - 1; i > 0; i--)
        {
            pos++;
            if (pos % 3 == 0)
                str = str.Insert(i, ",");
        }
        return str;
    }
    public static string GetCashShowString(this int cashNum)
    {
        string str = cashNum.ToString();
        if (str.Length == 1)
            return "0.0" + str+ "0";
        else if (str.Length == 2)
            return "0." + str + "0";
        else
        {
            str = str.Insert(str.Length - 2, ".");
            int length = str.Length;
            int pos = 0;
            for (int i = length - 4; i > 0; i--)
            {
                pos++;
                if (pos % 3 == 0)
                    str = str.Insert(i, ",");
            }
            return str + "0";
        }
    }
    public static string GetWheelCashShowString(this int cashNum)
    {
        string str = cashNum.ToString();
        string result;
        if (str.Length == 1)
            result= "0.0" + str;
        else if (str.Length == 2)
            result= "0." + str;
        else
        {
            str = str.Insert(str.Length - 2, ".");
            int length = str.Length;
            int pos = 0;
            for (int i = length - 4; i > 0; i--)
            {
                pos++;
                if (pos % 3 == 0)
                    str = str.Insert(i, ",");
            }
            result = str;
        }
        while (result.Length > 1)
        {
            char lastChar = result[result.Length - 1];
            if (lastChar.Equals('0'))
                result = result.Substring(0, result.Length - 1);
            else if (lastChar.Equals('.'))
            {
                result = result.Substring(0, result.Length - 1);
                break;
            }
            else
                break;
        }
        return result;
    }
}
