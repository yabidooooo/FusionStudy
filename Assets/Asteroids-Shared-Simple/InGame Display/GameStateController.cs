using System;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

namespace Asteroids.SharedSimple
{
    public class GameStateController : NetworkBehaviour
    {
        enum GameState
        {
            Starting,
            Running,
            Ending
        }

        [SerializeField] private float _startDelay = 4.0f;
        [SerializeField] private float _endDelay = 4.0f;
        [SerializeField] private float _gameSessionLength = 180.0f;

        [SerializeField] private TextMeshProUGUI _startEndDisplay = null;
        [SerializeField] private TextMeshProUGUI _ingameTimerDisplay = null;

        [Networked] private TickTimer _timer { get; set; }
        [Networked] private GameState _gameState { get; set; }
        [Networked] private NetworkBehaviourId _winner { get; set; }

        public bool GameIsRunning => _gameState == GameState.Running;

        //TODO: Need to guarantee that the game only support 4 playrs
        [Networked, Capacity(4)] private NetworkLinkedList<NetworkBehaviourId> _playerDataNetworkedIds => default;

        public override void Spawned()
        {
            // --- This section is for all information which has to be locally initialized based on the networked game state
            // --- when a CLIENT joins a game

            _startEndDisplay.gameObject.SetActive(true);
            _ingameTimerDisplay.gameObject.SetActive(false);

            if (Object.HasStateAuthority == false) return;

            // --- This section is for all networked information that has to be initialized by the master client
            // If the game has already started, find all currently active players' PlayerDataNetworked component Ids
            if (_gameState != GameState.Starting)
            {
                foreach (var player in Runner.ActivePlayers)
                {
                    if (Runner.TryGetPlayerObject(player, out var playerObject) == false) continue;
                    RPC_TrackNewPlayer(playerObject.GetComponent<PlayerDataNetworked>().Id);
                }
            }

            // Initialize the game state on the master client
            _gameState = GameState.Starting;
            _timer = TickTimer.CreateFromSeconds(Runner, _startDelay);
        }


        public override void FixedUpdateNetwork()
        {
            // Update the game display with the information relevant to the current game state
            switch (_gameState)
            {
                case GameState.Starting:
                    UpdateStartingDisplay();
                    break;
                case GameState.Running:
                    UpdateRunningDisplay();
                    // Ends the game if the game session length has been exceeded
                    if (_timer.ExpiredOrNotRunning(Runner) && Object.HasStateAuthority)
                    {
                        GameHasEnded();
                    }

                    break;
                case GameState.Ending:
                    UpdateEndingDisplay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateStartingDisplay()
        {
            // Display the remaining time until the game starts in seconds (rounded down to the closest full second)

            _startEndDisplay.text = $"Game Starts In {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";

            // --- Master client
            if (Object.HasStateAuthority == false) return;
            if (_timer.ExpiredOrNotRunning(Runner) == false) return;

            // Starts the Spaceship and Asteroids spawners once the game start delay has expired
            FindObjectOfType<SpaceshipSpawner>().StartSpaceshipSpawner(this);
            FindObjectOfType<AsteroidSpawner>().StartAsteroidSpawner();
            // RPC to spawn already joined players
            RPC_SpawnReadyPlayers();


            // Switches to the Running GameState and sets the time to the length of a game session
            _gameState = GameState.Running;
            _timer = TickTimer.CreateFromSeconds(Runner, _gameSessionLength);
        }

        // Triggers to all player to spawn their avatars.
        // This will only be called for players that joins before the game starts.
        // Late joiners will spawn their avatars from the OnPlayerJoined callback.
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        private void RPC_SpawnReadyPlayers()
        {
            FindObjectOfType<SpaceshipSpawner>().SpawnSpaceship(Runner.LocalPlayer);
        }

        private void UpdateRunningDisplay()
        {
            // --- All clients
            // Display the remaining time until the game ends in seconds (rounded down to the closest full second)
            _startEndDisplay.gameObject.SetActive(false);
            _ingameTimerDisplay.gameObject.SetActive(true);
            _ingameTimerDisplay.text =
                $"{Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0).ToString("000")} seconds left";
        }

        private void UpdateEndingDisplay()
        {
            // --- All clients
            // Display the results and
            // the remaining time until the current game session is shutdown

            if (Runner.TryFindBehaviour(_winner, out PlayerDataNetworked playerData) == false) return;

            _startEndDisplay.gameObject.SetActive(true);
            _ingameTimerDisplay.gameObject.SetActive(false);
            _startEndDisplay.text =
                $"{playerData.NickName} won with {playerData.Score} points. Disconnecting in {Mathf.RoundToInt(_timer.RemainingTime(Runner) ?? 0)}";
            _startEndDisplay.color = SpaceshipVisualController.GetColor(playerData.Object.InputAuthority);

            // Shutdowns the current game session.
            // The disconnection behaviour is found in the OnServerDisconnect.cs script
            if (_timer.ExpiredOrNotRunning(Runner) == false) return;

            Runner.Shutdown();
        }

        // Called when a spaceship hits an asteroid.
        // As any ship should be able to call and only the master client should process, it uses all for source and state authority for target.

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_CheckIfGameHasEnded()
        {
            CheckIfGameHasEnded();
        }

        // Called from the ShipController when it hits an asteroid
        public void CheckIfGameHasEnded()
        {
            if (Object.HasStateAuthority == false) return;

            int playersAlive = 0;

            for (int i = 0; i < _playerDataNetworkedIds.Count; i++)
            {
                if (Runner.TryFindBehaviour(_playerDataNetworkedIds[i],
                        out PlayerDataNetworked playerDataNetworkedComponent) == false)
                {
                    _playerDataNetworkedIds.Remove(_playerDataNetworkedIds[i]);
                    i--;
                    continue;
                }

                if (playerDataNetworkedComponent.Lives > 0) playersAlive++;
            }

            // If more than 1 player is left alive, the game continues.
            // If only 1 player is left, the game ends immediately.
            if (playersAlive > 1) return;

            foreach (var playerDataNetworkedId in _playerDataNetworkedIds)
            {
                if (Runner.TryFindBehaviour(playerDataNetworkedId,
                        out PlayerDataNetworked playerDataNetworkedComponent) ==
                    false) continue;

                if (playerDataNetworkedComponent.Lives > 0 == false) continue;

                _winner = playerDataNetworkedId;
            }

            GameHasEnded();
        }

        private void GameHasEnded()
        {
            _timer = TickTimer.CreateFromSeconds(Runner, _endDelay);
            _gameState = GameState.Ending;
        }

        // This RPC is needed because when a shared client joins when the game is running, it will create it own avatar.
        // That way, it needs a RPC to tell the master client to add on the list.
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_TrackNewPlayer(NetworkBehaviourId playerDataNetworkedId)
        {
            _playerDataNetworkedIds.Add(playerDataNetworkedId);
        }
    }
}