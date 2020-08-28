using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UGCF.UGCFEditor
{
    public class MakeTheFont : ScriptableWizard
    {
        [Tooltip("字体包含文本")]
        public string FntTxt = "0123456789";
        [Tooltip("字体偏移 x ")]
        public float OffsetX = 0;
        [Tooltip("字体偏移 y ")]
        public float OffsetY = 0;
        [Tooltip("字体间距,默认为字体宽度")]
        public int Space = -1;

        private int a;
        [MenuItem("自定义工具/自定义字体工具")]//创建界面按钮
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Make The Font", typeof(MakeTheFont), "GO"); //创建设置弹框
            Debug.Log("test1");
        }

        private void OnSelectionChange()
        {
            //Debug.Log(" Select "+ Selection.objects[Selection.objects.Length-1].name);
            errorString = "";
            if (Selection.objects.Length > 0)
            {
                var msg = "select : ";
                foreach (Object o in Selection.objects)
                {
                    string name = o.name;
                    string path = AssetDatabase.GetAssetPath(o.GetInstanceID());
                    //Debug.Log(path.Remove(0, path.IndexOf(name)+name.Length));
                    if (path.Remove(0, path.IndexOf(name) + name.Length) != ".png")
                    {
                        errorString = "Select file type error, please select PNG";
                    }
                    else
                    {
                        msg += name + ".";
                    }
                }
                helpString = msg;
            }
        }

        private void OnWizardCreate()
        {
            //Selection 选中对象
            foreach (Object o in Selection.objects)
            {
                //取得图片
                string name = o.name;
                string path = AssetDatabase.GetAssetPath(o.GetInstanceID());
                if (path.Remove(0, path.IndexOf(name) + name.Length) != ".png")
                {
                    Debug.Log(" Err File is : " + path + ",please select PNG");
                    continue;
                    //errorString = "Select file type error, please select PNG";
                }
                int index = path.IndexOf(name);
                //Debug.Log(index);
                path = path.Remove(index);
                //Debug.Log(path);

                //创建字体文件
                Font CustomFont = new Font();
                {
                    AssetDatabase.CreateAsset(CustomFont, path + "" + name + ".fontsettings");
                    AssetDatabase.SaveAssets();
                }

                //设置字体文件
                Texture2D tex = AssetDatabase.LoadAssetAtPath(path + "" + name + ".png", typeof(Texture2D)) as Texture2D;
                TextureImporter texImp = AssetImporter.GetAtPath(path + "" + name + ".png") as TextureImporter;
                CharacterInfo[] characterInfo2 = new CharacterInfo[texImp.spritesheet.Length];
                for (int i = 0; i < texImp.spritesheet.Length; i++)
                {
                    Rect rect = texImp.spritesheet[i].rect;
                    Vector2 size = rect.size;
                    CharacterInfo info = new CharacterInfo();
                    info.index = (int)FntTxt[i];
                    info.uv.x = rect.x / tex.width;
                    info.uv.y = rect.y / tex.height;
                    info.uv.width = size.x / tex.width;
                    info.uv.height = size.y / tex.height;
                    info.glyphWidth = (int)size.x;
                    info.glyphHeight = (int)size.y;
                    info.vert.x = 0;
                    info.vert.y = 0;
                    info.advance = Space <= 0 ? (int)size.x : Space;
                    characterInfo2[i] = info;
                }
                CustomFont.characterInfo = characterInfo2;

                //字体图片要在一行并且等分 没写别的处理

                //float width = (float)tex.width / (float)FntTxt.Length;
                //float diffX = 1.0f / (float)FntTxt.Length;
                //CharacterInfo[] characterInfo = new CharacterInfo[FntTxt.Length];
                //for (int i = 0; i < FntTxt.Length; i++)
                //{
                //    CharacterInfo info = new CharacterInfo();
                //    info.index = (int)FntTxt[i];
                //    info.uv.x = diffX * i;
                //    info.uv.y = 1;
                //    info.uv.width = diffX;
                //    info.uv.height = -1;
                //    info.vert.x = OffsetX;
                //    info.vert.y = OffsetY;
                //    info.vert.width = width;
                //    info.vert.height = tex.height;
                //    info.advance = Space <= 0 ? (int)width : Space;
                //    characterInfo[i] = info;
                //}
                //CustomFont.characterInfo = characterInfo;


                Material mat = null;
                {
                    Shader shader = Shader.Find("Transparent/Diffuse");
                    mat = new Material(shader);
                    mat.SetTexture("_MainTex", tex);
                    AssetDatabase.CreateAsset(mat, path + name + ".mat");
                    AssetDatabase.SaveAssets();
                }
                CustomFont.material = mat;
            }

        }
    }
}