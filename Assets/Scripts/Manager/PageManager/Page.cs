using UnityEngine;
using System.Collections;
using XLua;

[Hotfix]
public class Page : MonoBehaviour
{
    protected AssetBundle spriteAB;

    public void InitData(AssetBundle ab)
    {
        spriteAB = ab;
        gameObject.name = gameObject.name.Replace("(Clone)", "");
    }

    public virtual void Init() { gameObject.SetActive(false); }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    public AssetBundle GetSpriteAB()
    {
        return spriteAB;
    }
}
