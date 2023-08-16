using UnityEngine;

namespace UGCF.Network
{
    public class MessageListener : MonoBehaviour
    {
        void OnDestroy()
        {
            gameObject.RemoveAllListener();
        }
    }
}