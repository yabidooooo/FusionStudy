using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSpawner : Fusion.Behaviour
{
    [SerializeField] Player _playerPrefab;

    private void Awake()
    {
        var events = GetComponent<NetworkEvents>();
        events.PlayerJoined.AddListener( PlayerJoined );
        events.OnConnectedToServer.AddListener( OnServerJoined );
    }

    public void OnServerJoined( NetworkRunner runner )
    {
        if( runner.GameMode == GameMode.Shared || runner.GameMode == GameMode.Host )
        {
            SpawnPlayer( runner, runner.LocalPlayer );
        }
    }

    public void PlayerJoined( NetworkRunner runner, PlayerRef player )
    {
        SpawnPlayer( runner, player );
    }

    void SpawnPlayer( NetworkRunner runner, PlayerRef playerref )
    {
        runner.Spawn( _playerPrefab, null, null, playerref );
    }
}
