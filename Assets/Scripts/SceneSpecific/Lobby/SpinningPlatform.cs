using Unity.Netcode;
using UnityEngine;

namespace SceneSpecific.Lobby
{
    public class SpinningPlatform : NetworkBehaviour
    {
        [SerializeField] private float degPerSec;
        
        private void Update()
        {
            if (!IsOwner) return;
            transform.rotation *= Quaternion.AngleAxis(degPerSec * Time.deltaTime, Vector3.up);
        }
    }
}