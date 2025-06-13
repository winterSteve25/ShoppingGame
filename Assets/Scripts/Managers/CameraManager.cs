using Unity.Cinemachine;
using UnityEngine;

namespace Managers
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Current { get; private set; }

        public CinemachineCamera FPCam => fpCam;
    
        [SerializeField] private Camera cam;
        [SerializeField] private CinemachineCamera fpCam;

        private void Awake()
        {
            Current = this;
            fpCam.gameObject.SetActive(false);
        }

        public void SetFPTarget(Transform target)
        {
            fpCam.gameObject.SetActive(true);
            fpCam.Target = new CameraTarget()
            {
                TrackingTarget = target,
            };
        }
    }
}