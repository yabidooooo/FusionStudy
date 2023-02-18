using Fusion;
using UnityEngine;

namespace Asteroids.SharedSimple
{
    // The SpaceshipSpawner, just like the AsteroidSpawner, only executes on the Host.
    // Therefore none of its parameters need to be [Networked].
    public class SpaceshipSpawner : SimulationBehaviour, ISpawned
    {
        // References to the NetworkObject prefab to be used for the players' spaceships.
        [SerializeField] private NetworkPrefabRef _spaceshipNetworkPrefab = NetworkPrefabRef.Empty;

        private GameStateController _gameStateController = null;

        private SpawnPoint[] _spawnPoints = null;

        public void Spawned()
        {
            // Collect all spawn points in the scene.
            _spawnPoints = FindObjectsOfType<SpawnPoint>();

            // Spawn the spaceship for the local player if GameStateController is initialized
            if (FindObjectOfType<GameStateController>().GameIsRunning)
                SpawnSpaceship(Runner.LocalPlayer);
        }

        // The spawner is started when the GameStateController switches to GameState.Running.
        public void StartSpaceshipSpawner(GameStateController gameStateController)
        {
            _gameStateController = gameStateController;
        }

        // Spawns a spaceship for a player.
        // The spawn point is chosen in the _spawnPoints array using the implicit playerRef to int conversion 
        public void SpawnSpaceship(PlayerRef player)
        {
            // Modulo is used in case there are more players than spawn points.
            int index = player % _spawnPoints.Length;
            var spawnPosition = _spawnPoints[index].transform.position;

            var playerObject = Runner.Spawn(_spaceshipNetworkPrefab, spawnPosition, Quaternion.identity, player);
            // Set Player Object to facilitate access across systems.
            Runner.SetPlayerObject(player, playerObject);

            //If the spaceship is being spawned, game state controller is ready
            if (!_gameStateController)
                _gameStateController = FindObjectOfType<GameStateController>();

            // Add the new spaceship to the players to be tracked for the game end check.
            _gameStateController.RPC_TrackNewPlayer(playerObject.GetComponent<PlayerDataNetworked>().Id);
        }
    }
}