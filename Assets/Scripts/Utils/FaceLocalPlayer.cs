using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    public class FaceLocalPlayer : MonoBehaviour
    {
        private Transform _transform;

        private void Update()
        {
            if (NetworkManager.Singleton == null) return;
            if (!NetworkManager.Singleton.IsConnectedClient) return;

            if (_transform == null)
            {
                _transform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
            }

            transform.LookAt(_transform);
        }
    }
}