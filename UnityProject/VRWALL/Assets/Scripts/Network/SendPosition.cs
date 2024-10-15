using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class SendPosition : NetworkBehaviour
    {
        /// <summary>
        /// This script is used to send position of this transform trough the server and share it to different users
        /// </summary>

        public NetworkVariable<Vector3> userPosition;
        public Transform transformTarget;
        private AppState appstate;
       

        public override void OnNetworkSpawn()
        {
            appstate = AppState.Instance;
           Debug.Log(appstate); 
            if (IsOwner) Move();
       }

        [ServerRpc]
        void SubmitServerRpc(Vector3 position, ServerRpcParams param = default)
        {
            userPosition.Value = position;
        }
    
        private void Move()
        {
            
            if (IsOwner)
            {
                transformTarget.position = appstate.user.transform.position;
                Vector3 pos = appstate.vrwallTransform.InverseTransformPoint(transform.position);
                SubmitServerRpc(pos);
            }

            if (!IsOwner)
            {
                Vector3 pos = userPosition.Value;
                transformTarget.position = appstate.vrwallTransform.TransformPoint( pos );
            }

        }
        private void Update()  
        {
            if(appstate != null){
                Move();
            }
        }
   
   
    }
}
