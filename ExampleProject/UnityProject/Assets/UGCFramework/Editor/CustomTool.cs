using System.Collections;
using System.Collections.Generic;
using UGCF.UGUIExtend;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGCF.Editor
{
    public class CustomTool : UnityEditor.Editor
    {
        [MenuItem("自定义工具/清除缓存")]
        static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("自定义工具/其他工具/添加置灰材质球")]
        static void AddImageToGrey()
        {
            GameObject[] gos = Selection.gameObjects;
            for (int i = 0; i < gos.Length; i++)
                gos[i].GetComponent<Image>().material = Resources.Load<Material>("Shader/ImageToGrey");
        }

        [MenuItem("自定义工具/其他工具/设置选中物体所有的raycast为false")]
        static void ClearRaycast()
        {
            GameObject[] gos = Selection.gameObjects;
            for (int i = 0; i < gos.Length; i++)
            {
                Graphic[] gs = gos[i].GetComponentsInChildren<Graphic>(true);
                for (int j = 0; j < gs.Length; j++)
                    gs[j].raycastTarget = false;
            }
        }

        [MenuItem("自定义工具/其他工具/设置所有Button组件highlightedColor为纯白色")]
        static void ChangeButtonHigh()
        {
            GameObject[] gos = Selection.gameObjects;
            for (int i = 0; i < gos.Length; i++)
            {
                Selectable[] gs = gos[i].GetComponentsInChildren<Selectable>(true);
                for (int j = 0; j < gs.Length; j++)
                {
                    ColorBlock cbk = gs[j].colors;
                    ColorBlock cb = new ColorBlock();
                    cb.normalColor = cbk.normalColor;
                    cb.highlightedColor = Color.white;
                    cb.pressedColor = cbk.pressedColor;
                    cb.disabledColor = cbk.disabledColor;
                    cb.colorMultiplier = cbk.colorMultiplier;
                    cb.fadeDuration = cbk.fadeDuration;
                    gs[j].colors = cb;
                }
            }
        }

        static string fontName = "Arial";
        [MenuItem("自定义工具/文本组件相关/字体检查 %Q")]
        static void CheckFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Editor Default Resources/AssetBundle/Font/xxxx.ttf");
            if (!font)
                Debug.Log("字体 JDZYTJ.TTF 不存在");
            List<GameObject> gos = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < gos.Count; i++)
            {
                int wrongNum = 0;
                Text[] texts = gos[i].GetComponentsInChildren<Text>(true);
                for (int j = 0; j < texts.Length; j++)
                {
                    if (texts[j].font == null || texts[j].font.name == fontName)
                    {
                        wrongNum++;
                        texts[j].font = font;
                        //Debug.Log(gos[i].name + " : " + texts[j].name);
                    }
                }
                if (wrongNum > 0)
                    Debug.Log(gos[i].name);
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("自定义工具/UI/RawImage转换为Image")]
        static void ChangeRawImageToImage()
        {
            Color imgColor;
            bool isRaycast;
            Texture texture;
            Material material;
            List<GameObject> gos = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < gos.Count; i++)
            {
                RawImage[] images = gos[i].GetComponentsInChildren<RawImage>(true);
                for (int j = 0; j < images.Length; j++)
                {
                    RawImage image = images[j];
                    {
                        GameObject go = image.gameObject;
                        imgColor = image.color;
                        isRaycast = image.raycastTarget;
                        texture = image.texture;
                        material = image.material;
                        DestroyImmediate(image);

                        Image mt = go.AddComponent<Image>();
                        mt.color = imgColor;
                        //mt.sprite = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        if (material.name == "Default UI Material")
                            mt.material = null;
                        else
                            mt.material = material;
                        mt.raycastTarget = isRaycast;
                        Debug.Log(go.name);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("自定义工具/UI/Image转换为RawImage")]
        static void ChangeImageToRawImage()
        {
            Color imgColor;
            bool isRaycast;
            Sprite sprite;
            Material material;
            List<GameObject> gos = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < gos.Count; i++)
            {
                Image[] images = gos[i].GetComponentsInChildren<Image>(true);
                for (int j = 0; j < images.Length; j++)
                {
                    Image image = images[j];
                    if ((image.type == Image.Type.Simple || image.sprite == null) && !image.useSpriteMesh && !image.preserveAspect)
                    {
                        GameObject go = image.gameObject;
                        imgColor = image.color;
                        isRaycast = image.raycastTarget;
                        sprite = image.sprite;
                        material = image.material;
                        DestroyImmediate(image);

                        RawImage mt = go.AddComponent<RawImage>();
                        mt.color = imgColor;
                        mt.texture = sprite.texture;
                        if (material.name == "Default UI Material")
                            mt.material = null;
                        else
                            mt.material = material;
                        mt.raycastTarget = isRaycast;
                        Debug.Log(go.name);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("自定义工具/UI/Image转换为RawImage(不包含子节点)")]
        static void ChangeImageToRawImageSelf()
        {
            Color imgColor;
            bool isRaycast;
            Sprite sprite;
            Material material;
            List<GameObject> gos = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < gos.Count; i++)
            {
                Image image = gos[i].GetComponent<Image>();
                if (image != null && (image.type == Image.Type.Simple || image.sprite == null) && !image.useSpriteMesh && !image.preserveAspect)
                {
                    GameObject go = image.gameObject;
                    imgColor = image.color;
                    isRaycast = image.raycastTarget;
                    sprite = image.sprite;
                    material = image.material;
                    DestroyImmediate(image);

                    RawImage mt = go.AddComponent<RawImage>();
                    mt.color = imgColor;
                    mt.texture = sprite.texture;
                    if (material.name == "Default UI Material")
                        mt.material = null;
                    else
                        mt.material = material;
                    mt.raycastTarget = isRaycast;
                    Debug.Log(go.name);
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("自定义工具/UI/Image转换为RoundRectangleImage")]
        static void ChangeImageToRoundRectangleImage()
        {
            Color imgColor;
            bool isRaycast;
            Sprite sprite;
            Material material;
            List<GameObject> gos = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < gos.Count; i++)
            {
                Image[] images = gos[i].GetComponentsInChildren<Image>(true);
                for (int j = 0; j < images.Length; j++)
                {
                    Image image = images[j];
                    if ((image.type == Image.Type.Simple || image.sprite == null) && !image.useSpriteMesh && !image.preserveAspect)
                    {
                        GameObject go = image.gameObject;
                        imgColor = image.color;
                        isRaycast = image.raycastTarget;
                        sprite = image.sprite;
                        material = image.material;
                        DestroyImmediate(image);

                        RoundRectangleImage mt = go.AddComponent<RoundRectangleImage>();
                        mt.color = imgColor;
                        mt.sprite = sprite;
                        if (material.name == "Default UI Material")
                            mt.material = null;
                        else
                            mt.material = material;
                        mt.raycastTarget = isRaycast;
                        Debug.Log(go.name);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("自定义工具/UI/Image转换为RoundRectangleImage(不包含子节点)")]
        static void ChangeImageToRoundRectangleImageSelf()
        {
            Color imgColor;
            bool isRaycast;
            Sprite sprite;
            Material material;
            List<GameObject> gos = new List<GameObject>(Selection.gameObjects);
            for (int i = 0; i < gos.Count; i++)
            {
                Image image = gos[i].GetComponent<Image>();
                if (image != null && (image.type == Image.Type.Simple || image.sprite == null) && !image.useSpriteMesh && !image.preserveAspect)
                {
                    GameObject go = image.gameObject;
                    imgColor = image.color;
                    isRaycast = image.raycastTarget;
                    sprite = image.sprite;
                    material = image.material;
                    DestroyImmediate(image);

                    RoundRectangleImage mt = go.AddComponent<RoundRectangleImage>();
                    mt.color = imgColor;
                    mt.sprite = sprite;
                    if (material.name == "Default UI Material")
                        mt.material = null;
                    else
                        mt.material = material;
                    mt.raycastTarget = isRaycast;
                    Debug.Log(go.name);
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("自定义工具/其他工具/输出选择文件夹下所有文件名")]
        static void PrintSelectAllFileName()
        {
            string s = "";
            Object[] gos = Selection.objects;
            for (int i = 0; i < gos.Length; i++)
            {
                s += gos[i].name + ",\n";
            }
            Debug.Log(s);
        }

        [MenuItem("GameObject/UI/Tab")]
        static void AddTab()
        {
            GameObject[] gos = Selection.gameObjects;
            GameObject go = new GameObject("Tab", typeof(Tab));
            go.transform.parent = gos[0].transform;
        }
    }
}