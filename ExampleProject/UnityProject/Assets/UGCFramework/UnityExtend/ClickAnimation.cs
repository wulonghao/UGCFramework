using UnityEngine;

namespace UGCF.UnityExtend
{
    public class ClickAnimation : MonoBehaviour
    {
        [SerializeField] bool isZoom = true;
        [SerializeField] bool isGradient;
        [SerializeField] float zoomScale = 0.9f;
        [SerializeField] Vector3 defaultVt = Vector3.one;
        bool isDown, isPlaying;
        float lerpT;
        public bool IsZoom { get => isZoom; set => isZoom = value; }
        public bool IsGradient { get => isGradient; set => isGradient = value; }
        public float ZoomScale { get => zoomScale; set => zoomScale = value; }
        public Vector3 DefaultVt { get => defaultVt; set => defaultVt = value; }

        void Start()
        {
            UGUIEventListener.Get(gameObject).OnDown += delegate
            {
                if (IsZoom)
                {
                    if (IsGradient)
                    {
                        lerpT = 0;
                        isPlaying = true;
                        isDown = true;
                    }
                    else
                        transform.localScale = DefaultVt * ZoomScale;
                }
            };
            UGUIEventListener.Get(gameObject).OnUp += delegate
            {
                if (IsZoom)
                {
                    if (IsGradient)
                    {
                        lerpT = 1;
                        isPlaying = true;
                        isDown = false;
                    }
                    else
                        transform.localScale = DefaultVt;
                }
            };
        }

        void Update()
        {
            if (IsGradient && isPlaying)
            {
                if (isDown)
                    lerpT += 0.15f;
                else
                    lerpT -= 0.15f;
                transform.localScale = Vector3.Lerp(DefaultVt, DefaultVt * ZoomScale, lerpT);
                if ((isDown && lerpT >= 1) || (!isDown && lerpT <= 0))
                {
                    isPlaying = false;
                }
            }
        }
    }
}