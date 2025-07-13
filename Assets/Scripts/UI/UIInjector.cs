using Reflex.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIInjector : MonoBehaviour, IInstaller
    {
        [SerializeField] private Slider interactionProgressSlider;
        
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(interactionProgressSlider);
        }
    }
}