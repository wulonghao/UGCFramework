using UnityEngine.UI;
using UnityEngine;

namespace UGCF.UGUIExtend
{
    [RequireComponent(typeof(Text))]
    public class TextNonBreakingSpace : MonoBehaviour
    {
        static readonly string no_breaking_space = "\u00A0";

        void Awake()
        {
            Text txt = GetComponent<Text>();
            txt.RegisterDirtyVerticesCallback(() =>
            {
                if (txt.text.Contains(" "))
                    txt.text = txt.text.Replace(" ", no_breaking_space);
            });
        }
    }
}