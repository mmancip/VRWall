using UnityEngine;

namespace Network
{
    public class AppState : MonoBehaviour
    {
        [HideInInspector]
        public static AppState Instance;

        public Transform vrwallTransform;
        public Transform user;
        
        void Awake()
        {
            Instance = this;
        }

    }
}
