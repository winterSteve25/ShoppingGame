using System;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Objective.Time
{
    public class TimeObjective : NetworkBehaviour
    {
        [SerializeField] private float availableTime;
        [SerializeField] private TMP_Text timeText;

        private NetworkVariable<float> _timeRemaining = new();
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _timeRemaining.Value = availableTime;
            }
            
            _timeRemaining.OnValueChanged += (_, newValue) =>
            {
                var ts = TimeSpan.FromSeconds(newValue);
                timeText.text = $"{(int)ts.TotalMinutes}:{ts.Seconds:00}";
            };
        }

        private void Update()
        {
            if (!IsServer) return;
            _timeRemaining.Value -= UnityEngine.Time.unscaledDeltaTime;

            if (_timeRemaining.Value <= 0)
            {
                // todo
            }
        }
    }
}