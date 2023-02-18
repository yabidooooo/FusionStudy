using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Asteroids.HostSimple
{
    // Holds the player's information and ensures it is replicated to all clients.
    public class PlayerDataNetworked : NetworkBehaviour
    {
        // Global static setting
        private const int STARTING_LIVES = 3;

        // Local Runtime references
        private PlayerOverviewPanel _overviewPanel = null;

        // Game Session SPECIFIC Settings are used in the UI.
        // The method passed to the OnChanged attribute is called everytime the [Networked] parameter is changed.
        [HideInInspector]
        [Networked(OnChanged = nameof(OnNickNameChanged))]
        public NetworkString<_16> NickName { get; private set; }

        [HideInInspector]
        [Networked(OnChanged = nameof(OnLivesChanged))]
        public int Lives { get; private set; }

        [HideInInspector]
        [Networked(OnChanged = nameof(OnScoreChanged))]
        public int Score { get; private set; }

        public override void Spawned()
        {
            // --- Client
            // Find the local non-networked PlayerData to read the data and communicate it to the Host via a single RPC 
            if (Object.HasInputAuthority)
            {
                var nickName = FindObjectOfType<PlayerData>().GetNickName();
                RpcSetNickName(nickName);
            }

            // --- Host
            // Initialized game specific settings
            if (Object.HasStateAuthority)
            {
                Lives = STARTING_LIVES;
                Score = 0;
            }

            // --- Host & Client
            // Set the local runtime references.
            _overviewPanel = FindObjectOfType<PlayerOverviewPanel>();
            // Add an entry to the local Overview panel with the information of this spaceship
            _overviewPanel.AddEntry(Object.InputAuthority, this);
        }

        // Remove the entry in the local Overview panel for this spaceship
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _overviewPanel.RemoveEntry(Object.InputAuthority);
        }

        // Increase the score by X amount of points
        public void AddToScore(int points)
        {
            Score += points;
        }

        // Decrease the current Lives by 1
        public void SubtractLife()
        {
            Lives--;
        }

        // RPC used to send player information to the Host
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void RpcSetNickName(string nickName)
        {
            if (string.IsNullOrEmpty(nickName)) return;
            NickName = nickName;
        }

        // Updates the player's nickname displayed in the local Overview Panel entry.
        public static void OnNickNameChanged(Changed<PlayerDataNetworked> playerInfo)
        {
            playerInfo.Behaviour._overviewPanel.UpdateNickName(playerInfo.Behaviour.Object.InputAuthority,
                playerInfo.Behaviour.NickName.ToString());
        }

        // Updates the player's current Score displayed in the local Overview Panel entry.
        public static void OnScoreChanged(Changed<PlayerDataNetworked> playerInfo)
        {
            playerInfo.Behaviour._overviewPanel.UpdateScore(playerInfo.Behaviour.Object.InputAuthority,
                playerInfo.Behaviour.Score);
        }

        // Updates the player's current amount of Lives displayed in the local Overview Panel entry.
        public static void OnLivesChanged(Changed<PlayerDataNetworked> playerInfo)
        {
            playerInfo.Behaviour._overviewPanel.UpdateLives(playerInfo.Behaviour.Object.InputAuthority,
                playerInfo.Behaviour.Lives);
        }
    }
}