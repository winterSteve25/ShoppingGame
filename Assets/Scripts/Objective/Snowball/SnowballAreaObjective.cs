using System.Collections.Generic;
using Items;
using Player;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Objective.Snowball
{
    public class SnowballAreaObjective : NetworkBehaviour, IInteractableArea
    {
        [SerializeField] private float timeNeededToConquer;
        [SerializeField] private Slider conquerMeter;
        [SerializeField] private Image meterColor;
        [SerializeField] private Image meterBackgroundColor;
        [SerializeField] private NetworkObject snowBallPrefab;

        private NetworkVariable<byte> _owningTeam = new();
        private NetworkVariable<float> _conquerTimer = new();
        private NetworkVariable<byte> _isConquering = new(byte.MaxValue);

        [ShowInInspector] public byte OwningTeam => _owningTeam.Value;
        [ShowInInspector] public float ConqueringTimer => _conquerTimer.Value;
        [ShowInInspector] public byte IsConquering => _isConquering.Value;

        // Server Only
        private List<PlayerIdentity> _playersInArea;

        public override void OnNetworkSpawn()
        {
            _owningTeam.OnValueChanged += (_, newValue) =>
            {
                Color.RGBToHSV(TeamUtils.GetTeamColor(newValue), out float h, out float s, out float v);
                meterBackgroundColor.color = Color.HSVToRGB(h, s, v * 0.8f);
            };
            _isConquering.OnValueChanged += (_, newValue) => { meterColor.color = TeamUtils.GetTeamColor(newValue); };
            _conquerTimer.OnValueChanged += (_, newValue) => { conquerMeter.value = newValue / timeNeededToConquer; };

            if (!IsServer) return;

            _owningTeam.Value = byte.MaxValue;
            _playersInArea = new List<PlayerIdentity>();
        }

        private void Update()
        {
            if (!IsServer) return;

            if (_playersInArea.Count <= 0)
            {
                if (_isConquering.Value != byte.MaxValue)
                {
                    _conquerTimer.Value -= UnityEngine.Time.deltaTime;
                    if (_conquerTimer.Value <= 0)
                    {
                        _conquerTimer.Value = 0;
                        _isConquering.Value = byte.MaxValue;
                    }
                }

                return;
            }

            var isOneTeam = true;
            var team = byte.MaxValue;

            for (var i = 0; i < _playersInArea.Count; i++)
            {
                var player = _playersInArea[i];
                if (i == 0)
                {
                    team = player.TeamId;
                    continue;
                }

                if (team != player.TeamId)
                {
                    isOneTeam = false;
                    break;
                }

                team = player.TeamId;
            }

            if (isOneTeam)
            {
                // was conquering
                if (_isConquering.Value != byte.MaxValue)
                {
                    // continue conquering
                    if (_isConquering.Value == team)
                    {
                        _conquerTimer.Value += UnityEngine.Time.deltaTime;

                        if (_conquerTimer.Value >= timeNeededToConquer)
                        {
                            _conquerTimer.Value = 0;
                            _owningTeam.Value = _isConquering.Value;
                            _isConquering.Value = byte.MaxValue;
                        }

                        return;
                    }
                }

                // start conquering
                if (_owningTeam.Value != team)
                {
                    _isConquering.Value = team;
                    _conquerTimer.Value = 0;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if (!other.CompareTag("Player")) return;
            if (!other.TryGetComponent(out PlayerIdentity playerIdentity)) return;
            _playersInArea.Add(playerIdentity);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;
            if (!other.CompareTag("Player")) return;
            if (!other.TryGetComponent(out PlayerIdentity playerIdentity)) return;
            _playersInArea.Remove(playerIdentity);
        }

        public bool Interact(NetworkObject player, bool left)
        {
            if (!player.TryGetComponent(out PlayerHandManager hand)) return false;
            if (!player.TryGetComponent(out PlayerIdentity playerIdentity)) return false;
            if (playerIdentity.TeamId != _owningTeam.Value) return false;

            var item = left ? hand.LeftHandItem : hand.RightHandItem;
            if (item != null) return false;

            InteractRpc(player, left);
            return true;
        }

        [Rpc(SendTo.Server)]
        private void InteractRpc(NetworkObjectReference player, bool left, RpcParams sender = default)
        {
            if (!player.TryGet(out var p)) return;
            var obj = NetworkManager.SpawnManager.InstantiateAndSpawn(snowBallPrefab, p.OwnerClientId, true);
            PlayerPickupRpc(player, obj, left, RpcTarget.Single(sender.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void PlayerPickupRpc(NetworkObjectReference player, NetworkObjectReference item, bool left, RpcParams _)
        {
            if (!player.TryGet(out var p)) return;
            if (!item.TryGet(out var obj)) return;

            p.GetComponent<PlayerHandManager>()
                .PickupItemToHand(obj.GetComponent<WorldItem>(), left);
        }
    }
}