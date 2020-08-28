using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UGCF.Utils;

namespace UGCF.Manager
{
    [RequireComponent(typeof(AudioSource))]
    public partial class AudioManager : MonoBehaviour
    {
        private static AudioManager instance = null;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<AudioManager>();
                    instance.name = instance.GetType().ToString();
                    DontDestroyOnLoad(instance);
                    instance.Init();
                }
                return instance;
            }
        }
        const string soundVolumePrefKey = "soundVolume";
        const string musicVolumePrefKey = "musicVolume";
        const string soundMutePrefKey = "soundMute";
        const string musicMutePrefKey = "musicMute";
        #region ...属性
        public float MusicVolume
        {
            get
            {
                if (PlayerPrefs.HasKey(musicVolumePrefKey))
                    return PlayerPrefs.GetFloat(musicVolumePrefKey);
                else
                    return 1;
            }
            set
            {
                PlayerPrefs.SetFloat(musicVolumePrefKey, value);
                instance.musicSource.volume = value;
            }
        }
        public float SoundVolume
        {
            get
            {
                if (PlayerPrefs.HasKey(soundVolumePrefKey))
                    return PlayerPrefs.GetFloat(soundVolumePrefKey);
                else
                    return 1;
            }
            set
            {
                PlayerPrefs.SetFloat(soundVolumePrefKey, value);
                instance.soundSource.volume = value;
            }
        }
        public bool SoundMute
        {
            get
            {
                if (PlayerPrefs.HasKey(soundMutePrefKey))
                    return PlayerPrefs.GetInt(soundMutePrefKey) == 1;
                else
                    return false;
            }
            set
            {
                PlayerPrefs.SetInt(soundMutePrefKey, value ? 1 : 0);
                instance.soundSource.mute = value;
                foreach (AudioSource ase in tempAudioParent.GetComponentsInChildren<AudioSource>(true))
                    ase.mute = value;
            }
        }
        public bool MusicMute
        {
            get
            {
                if (PlayerPrefs.HasKey(musicMutePrefKey))
                    return PlayerPrefs.GetInt(musicMutePrefKey) == 1;
                else
                    return true;
            }
            set
            {
                PlayerPrefs.SetInt(musicMutePrefKey, value ? 1 : 0);
                instance.musicSource.mute = value;
            }
        }
        public bool IsPlayingSound { get { return soundSource.isPlaying; } }
        public bool IsPlayingMusic { get { return musicSource.isPlaying; } }
        #endregion
        /// <summary> 默认音效播放器 </summary>
        AudioSource soundSource;
        /// <summary> 默认音乐播放器 </summary>
        AudioSource musicSource;
        /// <summary> 临时声音播放器组父物体 </summary>
        Transform tempAudioParent;
        Dictionary<string, AudioClip> soundClipDict = new Dictionary<string, AudioClip>();
        Dictionary<string, AudioSource> soundASDict = new Dictionary<string, AudioSource>();//所有音频的AudioSource
        Dictionary<string, AudioClip> musicClipDict = new Dictionary<string, AudioClip>();

        void Init()
        {
            GameObject bm = new GameObject("BackgroundMusic");
            bm.transform.SetParent(transform);
            tempAudioParent = new GameObject("AudioParent").transform;
            tempAudioParent.SetParent(transform);

            musicSource = bm.AddComponent<AudioSource>();
            musicSource.mute = MusicMute;
            musicSource.volume = MusicVolume;
            musicSource.playOnAwake = false;

            soundSource = instance.GetComponent<AudioSource>();
            soundSource.mute = SoundMute;
            soundSource.volume = SoundVolume;
            soundSource.playOnAwake = false;
        }

        /// <summary>
        /// 创建一个音效源
        /// </summary>
        /// <param name="soundType">音效类型</param>
        /// <returns></returns>
        AudioSource CreateSoundSource(string soundType)
        {
            GameObject go = new GameObject(soundType.ToString());
            go.transform.SetParent(tempAudioParent);
            AudioSource ase = go.AddComponent<AudioSource>();
            ase.playOnAwake = false;
            return ase;
        }

        /// <summary>
        /// 在指定物体上播放音效
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="directoryPath"></param>
        /// <param name="isLoop"></param>
        /// <param name="pitch">加速倍数</param>
        /// <param name="playTime">循环播放时播放的时长,isLoop为true时有效</param>
        public AudioSource PlayTempSound(string soundName, string directoryPath = null, bool isLoop = false, float pitch = 1, float playTime = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryPath))
                    directoryPath = PageManager.Instance.CurrentPage.GetType().ToString();
                AudioSource ase;
                if (soundClipDict.ContainsKey(soundName))
                    ase = soundASDict[soundName];
                else
                {
                    ase = CreateSoundSource(soundName);
                    soundASDict.Add(soundName, ase);
                }
                ase.clip = GetAudioByType(soundName, directoryPath, SoundType.Sound);
                ase.loop = isLoop;
                ase.pitch = pitch;
                ase.volume = SoundVolume;
                ase.mute = SoundMute;
                ase.Play();
                if (isLoop && playTime != 0)
                    StartCoroutine(DelayStop(ase, playTime));
                return ase;
            }
            catch (Exception e)
            {
                LogUtils.LogError("播放音效失败：" + soundName + "\n"+ e.ToString());
                return null;
            }
        }

        IEnumerator DelayStop(AudioSource ase, float delayTime)
        {
            yield return WaitForUtils.WaitForSecondsRealtime(delayTime);
            ase.Stop();
        }

        public void StopTargetTempSound(string soundType)
        {
            Transform tf = tempAudioParent.Find(soundType.ToString());
            if (tf)
            {
                AudioSource ase = tf.GetComponent<AudioSource>();
                ase.Stop();
            }
        }

        public void PlaySound(string soundName, string directoryPath = ConstantUtils.CommonResourcesFolderName, bool isLoop = false)
        {
            try
            {
                AudioClip ac = GetAudioByType(soundName, directoryPath, SoundType.Sound);
                if (ac)
                {
                    soundSource.clip = ac;
                    soundSource.loop = isLoop;
                    soundSource.Play();
                }
                else
                    LogUtils.Log("要播放的音效文件不存在：" + soundName);
            }
            catch (Exception e)
            {
                LogUtils.LogError("播放音效失败：" + soundName + "\n" + e.ToString());
                return;
            }
        }

        public AudioClip PlayMusic(string musicName, string directoryPath = ConstantUtils.CommonResourcesFolderName)
        {
            try
            {
                AudioClip ac = GetAudioByType(musicName, directoryPath, SoundType.Music);
                if (ac)
                {
                    musicSource.clip = ac;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                else
                    LogUtils.Log("要播放的音乐文件不存在：" + musicName);
                return ac;
            }
            catch (Exception e)
            {
                LogUtils.LogError("播放音乐失败：" + musicName + "\n" + e.ToString());
                return null;
            }
        }

        public void PlayMusicList(List<string> allMusicName, string directoryPath = ConstantUtils.CommonResourcesFolderName)
        {
            StartCoroutine(PlayMusicListAc(allMusicName, directoryPath));
        }

        IEnumerator PlayMusicListAc(List<string> allMusicName, string directoryPath)
        {
            for (int i = 0; i < allMusicName.Count; i++)
            {
                AudioClip ac = GetAudioByType(allMusicName[i], directoryPath, SoundType.Music);
                if (ac)
                {
                    musicSource.clip = ac;
                    musicSource.loop = false;
                    musicSource.Play();
                }
                yield return WaitForUtils.WaitForSecondsRealtime(ac.length);
                if (i == allMusicName.Count - 1)
                    i = -1;
            }
        }

        /// <summary> 停止播放音乐 </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary> 暂停播放音乐 </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary> 继续播放音乐 </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        /// <summary> 设置是否静音 </summary>
        public void SetAudioMute(bool isMute)
        {
            MusicMute = isMute;
            SoundMute = isMute;
        }

        /// <summary>
        /// 根据类型获取对应音频数据
        /// </summary>
        /// <param name="soundName">音频文件名</param>
        /// <param name="directoryPath">所属文件夹相对path</param>
        /// <param name="soundType"></param>
        /// <returns></returns>
        public AudioClip GetAudioByType(string soundName, string directoryPath, SoundType soundType)
        {
            Dictionary<string, AudioClip> tempClipDict = soundType == SoundType.Music ? musicClipDict : soundClipDict;
            AudioClip ac = null;
            if (tempClipDict.ContainsKey(soundName))
            {
                ac = tempClipDict[soundName];
            }
            else
            {
                ac = BundleManager.Instance.GetAudioClip(soundName, soundType.ToString(), directoryPath);
                tempClipDict.Add(soundName, ac);
            }
            return ac;
        }

        public void ClearAllTempAudio()
        {
            List<string> astTemp = new List<string>();
            foreach (string ast in soundClipDict.Keys)
                foreach (Transform tf in tempAudioParent)
                    if (tf.name == ast.ToString())
                    {
                        astTemp.Add(ast);
                        Destroy(tf.gameObject);
                        break;
                    }
            foreach (string ast in astTemp)
            {
                soundClipDict.Remove(ast);
                soundASDict.Remove(ast);
            }
            Resources.UnloadUnusedAssets();
        }

        void OnDestroy()
        {
            instance = null;
        }

        public enum SoundType
        {
            Music,
            Sound
        }

        public enum MusicSourceMode
        {
            Stream,
            LocalFile
        }
    }
}
