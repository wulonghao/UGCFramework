using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;
using XLua;

[RequireComponent(typeof(AudioSource))]
[Hotfix]
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance = null;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject();
                instance = go.AddComponent<AudioManager>();
                go.name = instance.GetType().ToString();
                instance.Init();
            }
            return instance;
        }
    }
    /// <summary> 默认音效播放器 </summary>
    public AudioSource _soundSource;
    /// <summary> 默认音乐播放器 </summary>
    public AudioSource _musicSource;
    /// <summary> 临时声音播放器组父物体 </summary>
    public Transform tempAudioParent;
    Dictionary<AudioSoundType, AudioClip> sound_clipDict = new Dictionary<AudioSoundType, AudioClip>();
    Dictionary<string, AudioClip> music_clipDict = new Dictionary<string, AudioClip>();

    const string soundPrefKey = "sound";
    const string musicPrefKey = "music";

    int _music = 1;
    int _sound = 1;

    public int music
    {
        get
        {
            return _music;
        }
        set
        {
            _music = value;
            instance._musicSource.volume = _music;
        }
    }

    public int sound
    {
        get
        {
            return _sound;
        }
        set
        {
            _sound = value;
            instance._soundSource.volume = _sound;
        }
    }

    public bool IsPlayingSound
    {
        get
        {
            return _soundSource.isPlaying;
        }
    }

    void Init()
    {
        GameObject bm = new GameObject("BackgroundMusic");
        bm.transform.SetParent(transform);
        tempAudioParent = new GameObject("AudioParent").transform;
        tempAudioParent.SetParent(transform);

        _musicSource = bm.AddComponent<AudioSource>();
        if (PlayerPrefs.HasKey(musicPrefKey))
            _musicSource.mute = PlayerPrefs.GetFloat(musicPrefKey) == 0;
        else
        {
            _musicSource.mute = false;
            PlayerPrefs.SetFloat(musicPrefKey, 1);
        }
        _musicSource.playOnAwake = false;

        _soundSource = instance.GetComponent<AudioSource>();
        if (PlayerPrefs.HasKey(soundPrefKey))
            _soundSource.mute = PlayerPrefs.GetFloat(soundPrefKey) == 0;
        else
        {
            _soundSource.mute = false;
            PlayerPrefs.SetFloat(soundPrefKey, 1);
        }
        _soundSource.playOnAwake = false;
    }

    /// <summary>
    /// 创建一个音效源
    /// </summary>
    /// <param name="soundType">音效类型</param>
    /// <returns></returns>
    AudioSource CreateSoundSource(AudioSoundType soundType)
    {
        GameObject go = new GameObject(soundType.ToString());
        go.transform.SetParent(tempAudioParent);
        AudioSource ase = go.AddComponent<AudioSource>();
        ase.playOnAwake = false;
        return ase;
    }

    public AudioSource PlayTempSound(string soundName, string pageName = null, bool isLoop = false, float pitch = 1, float playTime = 0)
    {
        return PlayTempSound((AudioSoundType)Enum.Parse(typeof(AudioSoundType), soundName), pageName, isLoop, pitch, playTime);
    }

    /// <summary>
    /// 在指定物体上播放音效
    /// </summary>
    /// <param name="soundType"></param>
    /// <param name="pageName"></param>
    /// <param name="isLoop"></param>
    /// <param name="pitch">加速倍数</param>
    /// <param name="playTime">循环播放时播放的时长,isLoop为true时有效</param>
    public AudioSource PlayTempSound(AudioSoundType soundType, string pageName = null, bool isLoop = false, float pitch = 1, float playTime = 0)
    {
        if (string.IsNullOrEmpty(pageName))
            pageName = PageManager.Instance.CurrentPage.GetType().ToString();
        AudioSource ase;
        if (sound_clipDict.ContainsKey(soundType))
            ase = tempAudioParent.Find(soundType.ToString()).GetComponent<AudioSource>();
        else
            ase = CreateSoundSource(soundType);
        ase.clip = GetAudioByType(soundType, pageName);
        ase.loop = isLoop;
        ase.pitch = pitch;
        ase.volume = _sound;
        ase.mute = _soundSource.mute;
        ase.Play();
        if (isLoop && playTime != 0)
            StartCoroutine(DelayStop(ase, playTime));
        return ase;
    }

    IEnumerator DelayStop(AudioSource ase, float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);
        ase.Stop();
    }

    public void StopTargetTempSound(AudioSoundType soundType)
    {
        Transform tf = tempAudioParent.Find(soundType.ToString());
        if (tf)
        {
            AudioSource ase = tf.GetComponent<AudioSource>();
            ase.Stop();
        }

    }

    public void PlaySound(AudioSoundType soundType, string pageName = "Common", bool isLoop = false)
    {
        _soundSource.clip = GetAudioByType(soundType, pageName);
        _soundSource.loop = isLoop;
        _soundSource.Play();
        //return _soundSource;
    }

    public void PlayMusic(string musicName, string pageName = "Common")
    {
        _musicSource.clip = GetAudioByType(musicName, pageName);
        _musicSource.loop = true;
        _musicSource.Play();
    }

    /// <summary> 停止播放音乐 </summary>
    public void StopMusic()
    {
        _musicSource.Stop();
    }

    /// <summary> 暂停播放音乐 </summary>
    public void PauseMusic()
    {
        _musicSource.Pause();
    }

    /// <summary> 继续播放音乐 </summary>
    public void ResumeMusic()
    {
        _musicSource.UnPause();
    }

    /// <summary> 设置音效是否静音 </summary>
    public void SetSoundMute(bool isMute)
    {
        _soundSource.mute = isMute;
        foreach (AudioSource ase in tempAudioParent.GetComponentsInChildren<AudioSource>(true))
            ase.mute = isMute;

        PlayerPrefs.SetFloat(soundPrefKey, isMute ? 0 : 1);
    }

    /// <summary>
    /// 获取音效静音状态
    /// </summary>
    /// <returns></returns>
    public bool GetSoundMute()
    {
        return _soundSource.mute;
    }

    /// <summary> 设置音乐是否静音 </summary>
    public void SetMusicMute(bool isMute)
    {
        _musicSource.mute = isMute;
        PlayerPrefs.SetFloat(musicPrefKey, isMute ? 0 : 1);
    }

    /// <summary>
    /// 获取音乐静音状态
    /// </summary>
    public bool GetMusicMute()
    {
        return _musicSource.mute;
    }


    /// <summary> 设置是否静音 </summary>
    public void SetAudioMute(bool isMute)
    {
        _musicSource.mute = isMute;
        _soundSource.mute = isMute;
        foreach (AudioSource ase in tempAudioParent.GetComponentsInChildren<AudioSource>(true))
            ase.mute = isMute;

        PlayerPrefs.SetFloat(soundPrefKey, isMute ? 0 : 1);
        PlayerPrefs.SetFloat(musicPrefKey, isMute ? 0 : 1);
    }

    /// <summary> 设置音乐声音大小 </summary>
    public void SetMusic(int volume)
    {
        music = volume;
        PlayerPrefs.SetFloat(musicPrefKey, volume);
    }

    /// <summary> 设置音效声音大小 </summary>
    public void SetSound(int volume)
    {
        sound = volume;
        PlayerPrefs.SetFloat(soundPrefKey, volume);
    }

    AudioClip GetAudioByType(string musicName, string pageName)
    {
        AudioClip ac = null;
        if (music_clipDict.ContainsKey(musicName))
            ac = music_clipDict[musicName];
        else
        {
            ac = BundleManager.Instance.GetAudioClip(musicName, "Music", pageName);
            music_clipDict.Add(musicName, ac);
        }
        return ac;
    }

    AudioClip GetAudioByType(AudioSoundType soundType, string pageName)
    {
        AudioClip ac = null;
        if (sound_clipDict.ContainsKey(soundType))
            ac = sound_clipDict[soundType];
        else
        {
            ac = BundleManager.Instance.GetAudioClip(soundType.ToString(), "Sound", pageName);
            sound_clipDict.Add(soundType, ac);
        }
        return ac;
    }

    public void ClearAllTempAudio()
    {
        List<AudioSoundType> astTemp = new List<AudioSoundType>();
        foreach (AudioSoundType ast in sound_clipDict.Keys)
            foreach (Transform tf in tempAudioParent)
                if (tf.name == ast.ToString())
                {
                    astTemp.Add(ast);
                    Destroy(tf.gameObject);
                    break;
                }
        foreach (AudioSoundType ast in astTemp)
            sound_clipDict.Remove(ast);
    }

    void OnDestroy()
    {
        instance = null;
    }
}

public enum AudioSoundType
{
    None,
    BtnClick
}
