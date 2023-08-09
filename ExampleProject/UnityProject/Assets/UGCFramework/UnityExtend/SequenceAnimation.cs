using System.Collections;
using System.Collections.Generic;
using UGCF.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGCF.UnityExtend
{
    //[RequireComponent(typeof(Image))]
    public class SequenceAnimation : MonoBehaviour
    {
        private Image ImageSource;
        private SpriteRenderer SRSource;
        private int mCurFrame = 0;
        private float mDelta = 0;
        private Dictionary<int, UnityAction> diu = new Dictionary<int, UnityAction>();

        [SerializeField] float fPS = 5;
        [SerializeField] List<Sprite> spriteFrames = new List<Sprite>();
        [SerializeField] bool isPlaying;
        [SerializeField] bool foward = true;
        [SerializeField] bool autoPlay;
        [SerializeField] bool pingPong;
        [SerializeField] bool loop;
        [SerializeField] bool isAutoDisable;
        [SerializeField] bool isSetNativeSize = true;
        [SerializeField] float delayTime;

        public int FrameCount { get => SpriteFrames.Count; }
        public float FPS { get => fPS; set => fPS = value; }
        public List<Sprite> SpriteFrames { get => spriteFrames; set => spriteFrames = value; }
        public bool IsPlaying { get => isPlaying; set => isPlaying = value; }
        public bool Foward { get => foward; set => foward = value; }
        public bool AutoPlay { get => autoPlay; set => autoPlay = value; }
        public bool PingPong { get => pingPong; set => pingPong = value; }
        public bool Loop { get => loop; set => loop = value; }
        public bool IsAutoDisable { get => isAutoDisable; set => isAutoDisable = value; }
        public bool IsSetNativeSize { get => isSetNativeSize; set => isSetNativeSize = value; }
        public float DelayTime { get => delayTime; set => delayTime = value; }
        public UnityAction OnPlayEndCall { get; set; }
        public bool IsStart { get; set; }

        void Awake()
        {
            ImageSource = GetComponent<Image>();
            SRSource = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            if (Foward)
                mCurFrame = 0;
            else
                mCurFrame = FrameCount - 1;
            if (AutoPlay)
                Play();
        }

        void OnDisable()
        {
            IsStart = false;
        }

        private void SetSprite(int idx)
        {
            if (ImageSource != null || SRSource != null)
            {
                if (SpriteFrames != null && idx < SpriteFrames.Count)
                {
                    if (ImageSource)
                        ImageSource.sprite = SpriteFrames[idx];
                    if (SRSource)
                        SRSource.sprite = SpriteFrames[idx];
                }
                if (IsSetNativeSize)
                {
                    if (ImageSource)
                        ImageSource.SetNativeSize();
                }
            }
        }

        public void Play()
        {
            IsPlaying = true;
            Foward = true;
            if (gameObject.activeSelf)
                StartCoroutine(PlayAnimation());
        }

        public void PlayReverse()
        {
            IsPlaying = true;
            Foward = false;
        }

        IEnumerator PlayAnimation()
        {
            IsStart = true;
            yield return WaitForUtils.WaitForSecond(DelayTime);
            while (true)
            {
                yield return WaitForUtils.WaitFrame;
                if (!IsPlaying) continue;
                mDelta += Time.deltaTime;
                if (mDelta > 1 / FPS)
                {
                    mDelta = 0;
                    if (mCurFrame >= FrameCount)
                    {
                        if (Loop)
                        {
                            if (PingPong)
                            {
                                Foward = false;
                                mCurFrame = FrameCount - 1;
                            }
                            else
                                mCurFrame = 0;
                        }
                        else
                        {
                            IsPlaying = false;
                            if (IsAutoDisable)
                                gameObject.SetActive(false);
                            if (OnPlayEndCall != null)
                            {
                                OnPlayEndCall();
                                OnPlayEndCall = null;
                            }
                            break;
                        }
                    }
                    else if (mCurFrame < 0)
                    {
                        if (Loop)
                        {
                            if (PingPong)
                            {
                                Foward = true;
                                mCurFrame = 0;
                            }
                            else
                                mCurFrame = FrameCount - 1;
                        }
                        else
                        {
                            IsPlaying = false;
                            if (IsAutoDisable)
                                gameObject.SetActive(false);
                            break;
                        }
                    }
                    else
                    {
                        SetSprite(mCurFrame);
                        if (diu.ContainsKey(mCurFrame))
                            diu[mCurFrame]();
                        if (Foward)
                            mCurFrame++;
                        else
                            mCurFrame--;
                    }
                }
            }
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Continue()
        {
            if (!IsPlaying)
                IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
            mCurFrame = 0;
            SetSprite(mCurFrame);
        }

        public void Rewind()
        {
            mCurFrame = 0;
            SetSprite(mCurFrame);
            Play();
        }

        public void AddActionAtFrame(int frame, UnityAction ua)
        {
            diu.Add(frame, ua);
        }
    }
}