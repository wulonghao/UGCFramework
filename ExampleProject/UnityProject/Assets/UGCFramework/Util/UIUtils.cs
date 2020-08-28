using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System;

namespace UGCF.Utils
{
    public static class UIUtils
    {
        /// <summary> 创建指定属性的物体 </summary>
        public static GameObject CreateGameObject(Transform parent, string name = null, Vector3 position = default, Vector3 angle = default)
        {
            GameObject go = new GameObject();
            if (name == null)
                go.name = typeof(GameObject).ToString();
            else
                go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localEulerAngles = angle;
            return go;
        }

        /// <summary> 创建指定属性和组件的物体 </summary>
        public static T CreateGameObject<T>(Transform parent, string name = null, Vector3 position = default, Vector3 angle = default) where T : MonoBehaviour
        {
            T t = new GameObject().AddComponent<T>();
            if (name == null)
                t.name = typeof(T).ToString();
            else
                t.name = name;
            t.transform.SetParent(parent, false);
            t.transform.localPosition = position;
            t.transform.localEulerAngles = angle;
            return t;
        }

        /// <summary> 初始化目标的UI属性 </summary>
        public static void AttachAndReset(GameObject go, Transform parent, GameObject prefab = null)
        {
            RectTransform rectTrans = go.transform as RectTransform;
            if (rectTrans)
            {
                rectTrans.SetParent(parent);
                rectTrans.localPosition = Vector3.zero;
                rectTrans.localScale = Vector3.one;
                if (prefab == null)
                {
                    rectTrans.sizeDelta = Vector2.zero;
                    rectTrans.localPosition = Vector2.zero;
                    rectTrans.offsetMax = Vector2.zero;
                    rectTrans.offsetMin = Vector2.zero;
                }
                else
                {
                    RectTransform prefabRectTrans = prefab.transform as RectTransform;
                    if (prefabRectTrans)
                    {
                        rectTrans.sizeDelta = prefabRectTrans.sizeDelta;
                        rectTrans.localPosition = prefabRectTrans.localPosition;
                        rectTrans.offsetMax = prefabRectTrans.offsetMax;
                        rectTrans.offsetMin = prefabRectTrans.offsetMin;
                    }
                }
            }
        }

        /// <summary> 双线性插值法缩放图片，等比缩放 </summary>
        public static Texture2D ScaleTextureBilinear(Texture2D originalTexture, float scaleFactor)
        {
            Texture2D newTexture = new Texture2D(Mathf.CeilToInt(originalTexture.width * scaleFactor), Mathf.CeilToInt(originalTexture.height * scaleFactor));
            float scale = 1.0f / scaleFactor;
            int maxX = originalTexture.width - 1;
            int maxY = originalTexture.height - 1;
            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    float targetX = x * scale;
                    float targetY = y * scale;
                    int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                    int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                    int x2 = Mathf.Min(maxX, x1 + 1);
                    int y2 = Mathf.Min(maxY, y1 + 1);

                    float u = targetX - x1;
                    float v = targetY - y1;
                    float w1 = (1 - u) * (1 - v);
                    float w2 = u * (1 - v);
                    float w3 = (1 - u) * v;
                    float w4 = u * v;
                    Color color1 = originalTexture.GetPixel(x1, y1);
                    Color color2 = originalTexture.GetPixel(x2, y1);
                    Color color3 = originalTexture.GetPixel(x1, y2);
                    Color color4 = originalTexture.GetPixel(x2, y2);
                    Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                        Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                        Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                        Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                    );
                    newTexture.SetPixel(x, y, color);

                }
            }
            newTexture.Apply();
            return newTexture;
        }

        /// <summary> 双线性插值法缩放图片为指定尺寸 </summary>
        public static Texture2D SizeTextureBilinear(Texture2D originalTexture, Vector2 size)
        {
            Texture2D newTexture = new Texture2D(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
            float scaleX = originalTexture.width / size.x;
            float scaleY = originalTexture.height / size.y;
            int maxX = originalTexture.width - 1;
            int maxY = originalTexture.height - 1;
            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    float targetX = x * scaleX;
                    float targetY = y * scaleY;
                    int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                    int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                    int x2 = Mathf.Min(maxX, x1 + 1);
                    int y2 = Mathf.Min(maxY, y1 + 1);

                    float u = targetX - x1;
                    float v = targetY - y1;
                    float w1 = (1 - u) * (1 - v);
                    float w2 = u * (1 - v);
                    float w3 = (1 - u) * v;
                    float w4 = u * v;
                    Color color1 = originalTexture.GetPixel(x1, y1);
                    Color color2 = originalTexture.GetPixel(x2, y1);
                    Color color3 = originalTexture.GetPixel(x1, y2);
                    Color color4 = originalTexture.GetPixel(x2, y2);
                    Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                        Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                        Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                        Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                    );
                    newTexture.SetPixel(x, y, color);

                }
            }
            newTexture.Apply();
            return newTexture;
        }

        /// <summary>获取摄像机视图，显示在指定RawImage组件中</summary>
        public static void GetCameraRawImage(ref RawImage img, Camera ca, int depthBuffer = 24, RenderTextureFormat format = RenderTextureFormat.ARGB32)
        {
            if (img && ca)
            {
                Rect rect = img.GetComponent<RectTransform>().rect;
                RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, depthBuffer, format);
                rt.useMipMap = false;
                rt.filterMode = FilterMode.Bilinear;
                rt.antiAliasing = 4;
                rt.Create();
                ca.targetTexture = rt;
                img.texture = rt;
            }
            else
                LogUtils.Log("RowImage或Camera不存在！");
        }

        /// <summary> 得到鼠标/触点相对Canvas中心的位置 </summary>
        public static Vector2 GetMouseCenterPosInCanvas()
        {
            Vector2 mousePosition = InputUtils.GetTouchPosition();

            Vector2 middlePos = new Vector2(Screen.width / 2, Screen.height / 2);

            Vector2 endPos = middlePos - mousePosition;//最终位置
            return endPos = new Vector2(-1 * endPos.x, -1 * endPos.y);
        }

        /// <summary> 获取当前Canvas的Rect值,通过屏幕坐标转化 </summary>
        public static Rect GetRectInCanvas(Canvas canvas, RectTransform rectTrans)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, rectTrans.position, canvas.worldCamera, out Vector2 pos))
            {
                var rect = new Rect(new Vector2(pos.x - rectTrans.pivot.x * rectTrans.rect.width, pos.y - rectTrans.pivot.y * rectTrans.rect.height), rectTrans.rect.size);
                return rect;
            }

            throw new Exception("Error! Get RectTransform rect in canvas fail.");
        }

        /// <summary> 销毁目标物体的所有子物体 </summary>
        public static void DestroyChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                GameObject.Destroy(parent.GetChild(i).gameObject);
        }

        /// <summary> 设置目标物体下所有子物体的显隐状态 </summary>
        public static void SetAllChildrenActive(Transform trans, bool active)
        {
            for (int i = 0; i < trans.childCount; ++i)
                trans.GetChild(i).gameObject.SetActive(active);
        }

        /// <summary> 设置目标物体和目标物体下所有子物体的layer </summary>
        public static void SetAllChildrenLayer(Transform tf, int layer)
        {
            tf.gameObject.layer = layer;
            for (int i = 0; i < tf.childCount; i++)
                tf.GetChild(i).gameObject.layer = layer;
        }

        /// <summary> Texture转Sprite</summary>
        public static Sprite TextureToSprite(Texture texture)
        {
            Sprite sprite = null;
            if (texture)
            {
                Texture2D t2d = (Texture2D)texture;
                sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), Vector2.zero);
            }
            return sprite;
        }

        /// <summary> Texture旋转</summary>
        public static Texture2D RotateTexture(Texture2D texture, float eulerAngles)
        {
            int x;
            int y;
            int i;
            int j;
            float phi = eulerAngles / (180 / Mathf.PI);
            float sn = Mathf.Sin(phi);
            float cs = Mathf.Cos(phi);
            Color32[] arr = texture.GetPixels32();
            Color32[] arr2 = new Color32[arr.Length];
            int W = texture.width;
            int H = texture.height;
            int xc = W / 2;
            int yc = H / 2;

            for (j = 0; j < H; j++)
            {
                for (i = 0; i < W; i++)
                {
                    arr2[j * W + i] = new Color32(0, 0, 0, 0);

                    x = (int)(cs * (i - xc) + sn * (j - yc) + xc);
                    y = (int)(-sn * (i - xc) + cs * (j - yc) + yc);

                    if ((x > -1) && (x < W) && (y > -1) && (y < H))
                    {
                        arr2[j * W + i] = arr[y * W + x];
                    }
                }
            }

            Texture2D newImg = new Texture2D(W, H);
            newImg.SetPixels32(arr2);
            newImg.Apply();

            return newImg;
        }

        /// <summary> 在指定物体上添加指定图片 </summary>
        public static Image AddImage(GameObject target, Sprite sprite)
        {
            target.SetActive(false);
            Image image = target.GetComponent<Image>();
            if (!image)
                image = target.AddComponent<Image>();
            image.sprite = sprite;
            image.SetNativeSize();
            target.SetActive(true);
            return image;
        }

        /// <summary> 角度转向量 </summary>
        public static Vector2 AngleToVector2D(float angle)
        {
            float radian = Mathf.Deg2Rad * angle;
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
        }

        /// <summary>
        /// 返回两个向量的夹角
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;
            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            return cross.z > 0 ? -angle : angle;
        }

        /// <summary>
        /// 截屏，指定位置、尺寸、类型
        /// </summary>
        /// <param name="ua">截图完毕后执行的函数</param>
        /// <param name="rect">截图的rect信息,不传则默认全屏</param>
        /// <param name="dest">截图的偏移量，不传则为(0,0)</param>
        public static void PrintScreenNextFrame(this MonoBehaviour mono, UnityAction<Texture2D> ua = null, Rect rect = default, Vector2 dest = default)
        {
            if (rect == default) rect = new Rect(0, 0, Screen.width, Screen.height);
            mono.StartCoroutine(PrintScreenAc(rect, ua, dest));
        }

        private static IEnumerator PrintScreenAc(Rect rect, UnityAction<Texture2D> ua, Vector2 dest)
        {
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            yield return WaitForUtils.WaitFrame;
            texture.ReadPixels(rect, (int)dest.x, (int)dest.y);
            texture.Apply();
            ua?.Invoke(texture);
        }

        /// <summary>
        /// 截屏，指定位置、尺寸、类型
        /// </summary>
        /// <param name="rect">截图的rect信息</param>
        /// <param name="dest">截图的偏移量</param>
        public static Texture2D PrintScreen(Rect rect, Vector2 dest = default)
        {
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            texture.ReadPixels(rect, (int)dest.x, (int)dest.y);
            texture.Apply();
            return texture;
        }
    }
}