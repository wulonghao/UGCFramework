using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UGCF.UnityExtend
{
    public abstract class CommonAnimationBase
    {
        [SerializeField] bool autoPlay;
        public bool AutoPlay { get => autoPlay; set => autoPlay = value; }

        [SerializeField] bool pingPong;
        public bool PingPong { get => pingPong; set => pingPong = value; }

        [SerializeField] bool foward;
        public bool Foward { get => foward; set => foward = value; }

        [SerializeField] float playTime = 1;
        public float PlayTime { get => playTime; set => playTime = value; }

        [SerializeField] float delayTime;
        public float DelayTime { get => delayTime; set => delayTime = value; }

        public GameObject CurrentGameObject { get; set; }

        public RectTransform Rtf { get; set; }

        public UnityAction EndAction { get; set; }

        public virtual void Init(GameObject _currentGameObject)
        {
            CurrentGameObject = _currentGameObject;
            Rtf = CurrentGameObject.GetComponent<RectTransform>();
        }

        public abstract float GetSpeed();

        public abstract int GetCurrentStartIndex();

        public abstract int GetAnimationListCount();

        public abstract void PlayAnimation(int startIndex, float progress);

        public abstract void PlayEndAction();
    }

    [Serializable]
    public class CommonAnimationPoint : CommonAnimationBase
    {
        [SerializeField] List<Vector3> pointList = new List<Vector3>();
        public List<Vector3> PointList { get => pointList; set => pointList = value; }

        [SerializeField] CASpace caSpace = CASpace.TransformSelf;

        public CASpace CaSpace { get => caSpace; set => caSpace = value; }

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = PointList.Count - 1;

            if (CaSpace == CASpace.TransformSelf)
                CurrentGameObject.transform.localPosition = PointList[initIndex];
            else if (CaSpace == CASpace.TransformWorld)
                CurrentGameObject.transform.position = PointList[initIndex];
            else
                CurrentGameObject.GetComponent<RectTransform>().anchoredPosition3D = PointList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return PointList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (CaSpace == CASpace.TransformSelf)
                CurrentGameObject.transform.localPosition = Vector3.Lerp(PointList[startIndex], PointList[startIndex + 1], progress);
            else if (CaSpace == CASpace.TransformWorld)
                CurrentGameObject.transform.position = Vector3.Lerp(PointList[startIndex], PointList[startIndex + 1], progress);
            else
            {
                if (!Rtf) Rtf = CurrentGameObject.AddComponent<RectTransform>();
                Rtf.anchoredPosition3D = Vector3.Lerp(PointList[startIndex], PointList[startIndex + 1], progress);
            }
        }

        public override float GetSpeed()
        {
            return (PointList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : PointList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (EndAction != null)
                EndAction();
        }

        public enum CASpace
        {
            TransformSelf,
            TransformWorld,
            RectTransformSelf
        }
    }

    [Serializable]
    public class CommonAnimationAngle : CommonAnimationBase
    {
        [SerializeField] List<Vector3> angleList = new List<Vector3>();
        public List<Vector3> AngleList { get => angleList; set => angleList = value; }

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = AngleList.Count - 1;
            CurrentGameObject.transform.localEulerAngles = AngleList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return AngleList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            CurrentGameObject.transform.localEulerAngles = Vector3.Lerp(AngleList[startIndex], AngleList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (AngleList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : AngleList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (EndAction != null)
                EndAction();
        }
    }

    [Serializable]
    public class CommonAnimationScale : CommonAnimationBase
    {
        [SerializeField] List<Vector3> scaleList = new List<Vector3>();
        public List<Vector3> ScaleList { get => scaleList; set => scaleList = value; }

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = ScaleList.Count - 1;
            CurrentGameObject.transform.localScale = ScaleList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return ScaleList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            CurrentGameObject.transform.localScale = Vector3.Lerp(ScaleList[startIndex], ScaleList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (ScaleList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : ScaleList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (EndAction != null)
                EndAction();
        }
    }

    [Serializable]
    public class CommonAnimationSize : CommonAnimationBase
    {
        [SerializeField] List<Vector2> sizeList = new List<Vector2>();
        public List<Vector2> SizeList { get => sizeList; set => sizeList = value; }

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = SizeList.Count - 1;
            if (Rtf) Rtf.sizeDelta = SizeList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return SizeList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (Rtf) Rtf.sizeDelta = Vector2.Lerp(SizeList[startIndex], SizeList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (SizeList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : SizeList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (EndAction != null)
                EndAction();
        }
    }

    [Serializable]
    public class CommonAnimationAlpha : CommonAnimationBase
    {
        [SerializeField] List<float> alphaList = new List<float>();
        public List<float> AlphaList { get => alphaList; set => alphaList = value; }

        CanvasGroup cg;
        bool hadCanvas;


        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = AlphaList.Count - 1;
            cg = CurrentGameObject.GetComponent<CanvasGroup>();
            hadCanvas = cg;
            if (!cg)
            {
                cg = CurrentGameObject.AddComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            cg.alpha = AlphaList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return AlphaList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (!cg)
            {
                cg = CurrentGameObject.AddComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            cg.alpha = Mathf.Lerp(AlphaList[startIndex], AlphaList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (AlphaList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : AlphaList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (!hadCanvas)
                UnityEngine.Object.Destroy(cg);
            if (EndAction != null)
                EndAction();
        }
    }

    [Serializable]
    public class CommonAnimationColor : CommonAnimationBase
    {
        [SerializeField] List<Color> colorList = new List<Color>();
        public List<Color> ColorList { get => colorList; set => colorList = value; }
        Graphic graphic;

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = ColorList.Count - 1;
            graphic = CurrentGameObject.GetComponent<Graphic>();
            if (graphic)
                graphic.color = ColorList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return ColorList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (graphic)
                graphic.color = Color.Lerp(ColorList[startIndex], ColorList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (ColorList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : ColorList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (EndAction != null)
                EndAction();
        }
    }

    [Serializable]
    public class CommonAnimationFillAmount : CommonAnimationBase
    {
        [SerializeField] List<float> fillAmountList = new List<float>();
        public List<float> FillAmountList { get => fillAmountList; set => fillAmountList = value; }

        Image image;

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!Foward)
                initIndex = FillAmountList.Count - 1;
            image = CurrentGameObject.GetComponent<Image>();
            if (image)
                image.fillAmount = FillAmountList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return FillAmountList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (image)
                image.fillAmount = Mathf.Lerp(FillAmountList[startIndex], FillAmountList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (FillAmountList.Count - 1) / PlayTime;
        }

        public override int GetCurrentStartIndex()
        {
            return Foward ? 0 : FillAmountList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (EndAction != null)
                EndAction();
        }
    }
}