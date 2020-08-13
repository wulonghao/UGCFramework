using UnityEngine;

public class MessageListener : MonoBehaviour
{
    void OnDestroy()
    {
        gameObject.RemoveAllListener();
    }
}
