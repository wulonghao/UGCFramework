using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UGCF.UnityExtend
{
    public abstract class CommonAnimationBase
    {
        public bool pingPong, foward = true, autoPlay;
        public float playTime = 1;
        public float delayTime;
        [HideInInspector]
        public GameObject currentGameObject;
        [HideInInspector]
        public RectTransform rtf;
        [HideInInspector]
        public UnityAction endAction;

        public virtual void Init(GameObject _currentGameObject)
        {
            currentGameObject = _currentGameObject;
            rtf = currentGameObject.GetComponent<RectTransform>();
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
        public List<Vector3> pointList = new List<Vector3>();
        public CASpace caSpace = CASpace.TransformSelf;

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = pointList.Count - 1;

            if (caSpace == CASpace.TransformSelf)
                currentGameObject.transform.localPosition = pointList[initIndex];
            else if (caSpace == CASpace.TransformWorld)
                currentGameObject.transform.position = pointList[initIndex];
            else
                currentGameObject.GetComponent<RectTransform>().anchoredPosition3D = pointList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return pointList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (caSpace == CASpace.TransformSelf)
                currentGameObject.transform.localPosition = Vector3.Lerp(pointList[startIndex], pointList[startIndex + 1], progress);
            else if (caSpace == CASpace.TransformWorld)
                currentGameObject.transform.position = Vector3.Lerp(pointList[startIndex], pointList[startIndex + 1], progress);
            else
            {
                if (!rtf) rtf = currentGameObject.AddComponent<RectTransform>();
                rtf.anchoredPosition3D = Vector3.Lerp(pointList[startIndex], pointList[startIndex + 1], progress);
            }
        }

        public override float GetSpeed()
        {
            return (pointList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : pointList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (endAction != null)
                endAction();
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
        public List<Vector3> angleList = new List<Vector3>();

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = angleList.Count - 1;
            currentGameObject.transform.localEulerAngles = angleList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return angleList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            currentGameObject.transform.localEulerAngles = Vector3.Lerp(angleList[startIndex], angleList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (angleList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : angleList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (endAction != null)
                endAction();
        }
    }

    [Serializable]
    public class CommonAnimationScale : CommonAnimationBase
    {
        public List<Vector3> scaleList = new List<Vector3>();

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = scaleList.Count - 1;
            currentGameObject.transform.localScale = scaleList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return scaleList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            currentGameObject.transform.localScale = Vector3.Lerp(scaleList[startIndex], scaleList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (scaleList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : scaleList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (endAction != null)
                endAction();
        }
    }

    [Serializable]
    public class CommonAnimationSize : CommonAnimationBase
    {
        public List<Vector2> sizeList = new List<Vector2>();

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = sizeList.Count - 1;
            if (rtf) rtf.sizeDelta = sizeList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return sizeList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (rtf) rtf.sizeDelta = Vector2.Lerp(sizeList[startIndex], sizeList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (sizeList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : sizeList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (endAction != null)
                endAction();
        }
    }

    [Serializable]
    public class CommonAnimationAlpha : CommonAnimationBase
    {
        public List<float> alphaList = new List<float>();
        CanvasGroup cg;
        bool hadCanvas;

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = alphaList.Count - 1;
            cg = currentGameObject.GetComponent<CanvasGroup>();
            hadCanvas = cg;
            if (!cg)
            {
                cg = currentGameObject.AddComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            cg.alpha = alphaList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return alphaList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (!cg)
            {
                cg = currentGameObject.AddComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            cg.alpha = Mathf.Lerp(alphaList[startIndex], alphaList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (alphaList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : alphaList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (!hadCanvas)
                UnityEngine.Object.Destroy(cg);
            if (endAction != null)
                endAction();
        }
    }

    [Serializable]
    public class CommonAnimationColor : CommonAnimationBase
    {
        public List<Color> colorList = new List<Color>();
        Graphic graphic;

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = colorList.Count - 1;
            graphic = currentGameObject.GetComponent<Graphic>();
            if (graphic)
                graphic.color = colorList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return colorList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (graphic)
                graphic.color = Color.Lerp(colorList[startIndex], colorList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (colorList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : colorList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (endAction != null)
                endAction();
        }
    }

    [Serializable]
    public class CommonAnimationFillAmount : CommonAnimationBase
    {
        public List<float> fillAmountList = new List<float>();
        Image image;

        public override void Init(GameObject _currentGameObject)
        {
            base.Init(_currentGameObject);
            int initIndex = 0;
            if (!foward)
                initIndex = fillAmountList.Count - 1;
            image = currentGameObject.GetComponent<Image>();
            if (image)
                image.fillAmount = fillAmountList[initIndex];
        }

        public override int GetAnimationListCount()
        {
            return fillAmountList.Count;
        }

        public override void PlayAnimation(int startIndex, float progress)
        {
            if (image)
                image.fillAmount = Mathf.Lerp(fillAmountList[startIndex], fillAmountList[startIndex + 1], progress);
        }

        public override float GetSpeed()
        {
            return (fillAmountList.Count - 1) / playTime;
        }

        public override int GetCurrentStartIndex()
        {
            return foward ? 0 : fillAmountList.Count - 2;
        }

        public override void PlayEndAction()
        {
            if (endAction != null)
                endAction();
        }
    }
}