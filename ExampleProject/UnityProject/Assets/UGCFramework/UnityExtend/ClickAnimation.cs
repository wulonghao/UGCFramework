using System.Collections;
using System.Collections.Generic;
using UGCF.UGUIExtend;
using UnityEngine;

namespace UGCF.UnityExtend
{
    public class ClickAnimation : MonoBehaviour
    {
        public bool isZoom = true;
        public bool isGradient;
        public float zoomScale = 0.9f;
        public Vector3 defaultVt = Vector3.one;
        bool isDown, isPlaying;

        void Start()
        {
            UGUIEventListener.Get(gameObject).onDown += delegate
            {
                if (isZoom)
                {
                    if (isGradient)
                    {
                        lerpT = 0;
                        isPlaying = true;
                        isDown = true;
                    }
                    else
                        transform.localScale = defaultVt * zoomScale;
                }
            };
            UGUIEventListener.Get(gameObject).onUp += delegate
            {
                if (isZoom)
                {
                    if (isGradient)
                    {
                        lerpT = 1;
                        isPlaying = true;
                        isDown = false;
                    }
                    else
                        transform.localScale = defaultVt;
                }
            };
        }

        float lerpT;
        void Update()
        {
            if (isGradient && isPlaying)
            {
                if (isDown)
                    lerpT += 0.15f;
                else
                    lerpT -= 0.15f;
                transform.localScale = Vector3.Lerp(defaultVt, defaultVt * zoomScale, lerpT);
                if ((isDown && lerpT >= 1) || (!isDown && lerpT <= 0))
                {
                    isPlaying = false;
                }
            }
        }
    }
}