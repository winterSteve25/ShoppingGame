using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Player;
using PrimeTween;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSpecific.Lobby
{
    public class StartGameManager : NetworkBehaviour
    {
        private Dictionary<byte, List<ulong>> _teams;
        private NetworkVariable<int> _startCountDown = new();
        [NonSerialized] public NetworkVariable<int> SelectedMap = new();

        [SerializeField] private TMP_Text countDownText;

        private void Start()
        {
            countDownText.GetComponent<CanvasGroup>().alpha = 0;
        }

        public override void OnNetworkSpawn()
        {
            _startCountDown.OnValueChanged += (_, newValue) =>
            {
                countDownText.text = newValue.ToString();
                if (newValue == 0)
                {
                    countDownText.text = "Go!";
                }
            };
            
            if (!IsServer) return;
            _teams = new Dictionary<byte, List<ulong>>();
        }

        public void AddToTeam(ulong player, byte team)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Can not team modify on client");
                return;
            }
            
            if (!_teams.ContainsKey(team))
            {
                _teams.Add(team, new List<ulong>());
            }
            
            _teams[team].Add(player);
            NetworkManager
                .ConnectedClients[player]
                .PlayerObject
                .GetComponent<PlayerIdentity>()
                .SetTeamId(team);
            
            CheckStart();
        }

        public void RemoveFromTeam(ulong player, byte team)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Can not modify team on client");
                return;
            }
            
            if (!_teams.ContainsKey(team))
            {
                Debug.LogWarning($"Tried to remove {player} from team {team} but team {team} doesnt exist");
                return;
            }
            
            _teams[team].Remove(player);
        }

        private void CheckStart()
        {
            if (!CanStartGame())
            {
                return;
            }

            StartCoroutine(StartGame());
        }

        private bool CanStartGame()
        {
            var numPlayersReady = _teams.Aggregate(0, (acc, team) => acc + team.Value.Count);

            return NetworkManager.ConnectedClients.Count == numPlayersReady &&
                   LobbyManager.Singleton.Lobby != null &&
                   numPlayersReady == LobbyManager.Singleton.Lobby.Value.MemberCount;
        }

        private IEnumerator StartGame()
        {
            _startCountDown.Value = 3;
            
            var canvasGroup = countDownText.GetComponent<CanvasGroup>();
            Tween.Alpha(canvasGroup, 1, 0.2f);
            
            yield return new WaitForSeconds(1f);

            if (!CanStartGame())
            {
                Tween.Alpha(canvasGroup, 0, 0.2f);
                yield break;
            }

            _startCountDown.Value = 2;

            yield return new WaitForSeconds(1f);

            if (!CanStartGame())
            {
                Tween.Alpha(canvasGroup, 0, 0.2f);
                yield break;
            }

            _startCountDown.Value = 1;

            yield return new WaitForSeconds(1f);

            if (!CanStartGame())
            {
                Tween.Alpha(canvasGroup, 0, 0.2f);
                yield break;
            }

            _startCountDown.Value = 0;
            yield return new WaitForSeconds(0.5f);

            NetworkManager.SceneManager.LoadScene("Scenes/Game Scene", LoadSceneMode.Single);
        }
    }
}
