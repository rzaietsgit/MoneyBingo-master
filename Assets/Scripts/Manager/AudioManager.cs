using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    private static GameObject Root;
    private const string LoadAudioFrontPath = "AudioClips/";
    private static AudioSource bgmAs;
    public AudioManager(GameObject audioRoot)
    {
        Root = audioRoot;
        PlayBgm();
    }
    private static readonly Dictionary<AudioPlayArea, string> audioClipPathDic = new Dictionary<AudioPlayArea, string>()
    {
        {AudioPlayArea.BGM,"MainBGM" },
        {AudioPlayArea.Bingo,"Bingo" },
        {AudioPlayArea.Button,"button" },
        {AudioPlayArea.WindowShow,"WindowShow" },
        {AudioPlayArea.ReadyGo,"ReadyGO" },
        {AudioPlayArea.EnterGamePage,"EnterGamePage" },
        {AudioPlayArea.ExtraRewardFill,"ExtraRewardFill" },
        {AudioPlayArea.ExtraGoldReach,"TriggerCoinItem" },
        {AudioPlayArea.StarReach,"TriggerMustHitItem" },
        {AudioPlayArea.TripleReach,"TriggerTripleItem" },
        {AudioPlayArea.ClickCardBingo,"SquClick" },
        {AudioPlayArea.DrawPhone_getFragment,"DrawPhone_getFragment" },
        {AudioPlayArea.GameOver,"GameOver" },
    };
    private static readonly Dictionary<AudioPlayArea, AudioClip> loadedAudioclipDic = new Dictionary<AudioPlayArea, AudioClip>();
    private static readonly Dictionary<int, AudioClip> loadedBallNumClipDic = new Dictionary<int, AudioClip>();
    private static readonly List<AudioSource> allPlayer = new List<AudioSource>();
    private static readonly List<AudioPlayArea> allPlayerType = new List<AudioPlayArea>();
    public static AudioSource PlayOneShot(AudioPlayArea playArea, bool loop = false)
    {
        if (loadedAudioclipDic.TryGetValue(playArea, out AudioClip tempClip))
        {
            if (tempClip == null)
            {
                loadedAudioclipDic.Remove(playArea);
                Debug.LogError("音频路径配置错误，类型:" + playArea);
                return null;
            }
            int asCount = allPlayer.Count;
            AudioSource tempAs;
            for (int i = 0; i < asCount; i++)
            {
                if (allPlayerType[i] == playArea)
                {
                    tempAs = allPlayer[i];
                    tempAs.clip = tempClip;
                    tempAs.loop = loop;
                    tempAs.mute = !SaveManager.SaveData.sound_on;
                    tempAs.Stop();
                    tempAs.Play();
                    return tempAs;
                }
            }
            tempAs = Root.AddComponent<AudioSource>();
            allPlayer.Add(tempAs);
            allPlayerType.Add(playArea);
            tempAs.clip = tempClip;
            tempAs.loop = loop;
            tempAs.mute = !SaveManager.SaveData.sound_on;
            tempAs.Play();
            return tempAs;
        }
        else
        {
            if (audioClipPathDic.TryGetValue(playArea, out string tempClipFileName))
            {
                tempClip = Resources.Load<AudioClip>(LoadAudioFrontPath + tempClipFileName);
                if (tempClip == null)
                {
                    Debug.LogError("配置的音频文件路径错误，类型:" + playArea);
                    return null;
                }
                loadedAudioclipDic.Add(playArea, tempClip);
                int asCount = allPlayer.Count;
                AudioSource tempAs;
                for (int i = 0; i < asCount; i++)
                {
                    if (allPlayerType[i] == playArea)
                    {
                        tempAs = allPlayer[i];
                        tempAs.clip = tempClip;
                        tempAs.loop = loop;
                        tempAs.mute = !SaveManager.SaveData.sound_on;
                        tempAs.Stop();
                        tempAs.Play();
                        return tempAs;
                    }
                }
                tempAs = Root.AddComponent<AudioSource>();
                allPlayer.Add(tempAs);
                allPlayerType.Add(playArea);
                tempAs.clip = tempClip;
                tempAs.loop = loop;
                tempAs.mute = !SaveManager.SaveData.sound_on;
                tempAs.Play();
                return tempAs;
            }
            else
            {
                Debug.LogError("没有配置音频文件路径，类型:" + playArea);
                return null;
            }
        }
    }
    public static AudioSource PlayLoop(AudioPlayArea playArea)
    {
        return PlayOneShot(playArea, true);
    }
    private void PlayBgm()
    {
        if (audioClipPathDic.TryGetValue(AudioPlayArea.BGM, out string bgmFileName))
        {
            AudioClip tempClip = Resources.Load<AudioClip>(LoadAudioFrontPath + bgmFileName);
            if (tempClip == null)
            {
                Debug.LogError("背景音乐文件路径配置错误");
                return;
            }
            bgmAs = Root.AddComponent<AudioSource>();
            bgmAs.clip = tempClip;
            bgmAs.loop = true;
            bgmAs.mute = !SaveManager.SaveData.music_on;
            bgmAs.Play();
        }
        else
        {
            Debug.LogError("背景音乐没有配置文件路径");
        }
    }
    public static void SetMusicState(bool isOn)
    {
        bgmAs.mute = !isOn;
    }
    public static void SetSoundState(bool isOn)
    {
        int count = allPlayer.Count;
        for (int i = 0; i < count; i++)
        {
            allPlayer[i].mute = !isOn;
        }
    }
    public static void PauseBgm(bool pause)
    {
        if (pause)
            bgmAs.Pause();
        else
            bgmAs.UnPause();
    }
    public static AudioSource PlayBallNum(int num)
    {
        if (loadedBallNumClipDic.TryGetValue(num, out AudioClip tempClip))
        {
            if (tempClip == null)
            {
                loadedBallNumClipDic.Remove(num);
                Debug.LogError("音频路径配置错误，类型:" + num);
                return null;
            }
            int asCount = allPlayer.Count;
            AudioSource tempAs;
            for (int i = 0; i < asCount; i++)
            {
                tempAs = allPlayer[i];
                if (!tempAs.isPlaying)
                {
                    tempAs.clip = tempClip;
                    tempAs.loop = false;
                    tempAs.mute = !SaveManager.SaveData.sound_on;
                    tempAs.Play();
                    return tempAs;
                }
            }
            tempAs = Root.AddComponent<AudioSource>();
            allPlayer.Add(tempAs);
            allPlayerType.Add(AudioPlayArea.BallNum);
            tempAs.clip = tempClip;
            tempAs.loop = false;
            tempAs.mute = !SaveManager.SaveData.sound_on;
            tempAs.Play();
            return tempAs;
        }
        else
        {
            tempClip = Resources.Load<AudioClip>(LoadAudioFrontPath + "BallNum/" + num);
            if (tempClip == null)
            {
                Debug.LogError("配置的音频文件路径错误，类型:" + num);
                return null;
            }
            loadedBallNumClipDic.Add(num, tempClip);
            int asCount = allPlayer.Count;
            AudioSource tempAs;
            for (int i = 0; i < asCount; i++)
            {
                tempAs = allPlayer[i];
                if (!tempAs.isPlaying)
                {
                    tempAs.clip = tempClip;
                    tempAs.loop = false;
                    tempAs.mute = !SaveManager.SaveData.sound_on;
                    tempAs.Play();
                    return tempAs;
                }
            }
            tempAs = Root.AddComponent<AudioSource>();
            allPlayer.Add(tempAs);
            allPlayerType.Add(AudioPlayArea.BallNum);
            tempAs.clip = tempClip;
            tempAs.loop = false;
            tempAs.mute = !SaveManager.SaveData.sound_on;
            tempAs.Play();
            return tempAs;
        }
    }
}
public enum AudioPlayArea
{
    BallNum,
    BGM,
    Bingo,
    Button,
    WindowShow,
    ReadyGo,
    EnterGamePage,
    ExtraRewardFill,
    ExtraGoldReach,
    StarReach,
    TripleReach,
    ClickCardBingo,
    DrawPhone_getFragment,
    GameOver,
}
