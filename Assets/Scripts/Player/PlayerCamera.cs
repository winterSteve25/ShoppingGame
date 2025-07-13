using Reflex.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerCamera : MonoBehaviour, IInstaller
    {
        [SerializeField] private Camera cam;
        [SerializeField] private CinemachineCamera fpCam;

        // ReSharper disable once InconsistentNaming
        public new Transform transform => fpCam.transform;

        private void Awake()
        {
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

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(this, typeof(PlayerCamera));
        }
    }
}