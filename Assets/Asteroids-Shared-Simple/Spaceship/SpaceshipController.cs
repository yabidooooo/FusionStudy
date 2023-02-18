using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Asteroids.SharedSimple
{
    // This class controls the lifecycle of the spaceship
    public class SpaceshipController : NetworkBehaviour
    {
        // Game Session AGNOSTIC Settings
        [SerializeField] private float _respawnDelay = 4.0f;
        [SerializeField] private float _spaceshipDamageRadius = 2.5f;
        [SerializeField] private LayerMask _asteroidCollisionLayer;

        // Local Runtime references
        private Rigidbody _rigidbody = null;
        private PlayerDataNetworked _playerDataNetworked = null;
        private SpaceshipVisualController _visualController = null;

        // Game Session SPECIFIC Settings
        public bool AcceptInput => _isAlive && Object.IsValid;

        [Networked(OnChanged = nameof(OnAliveStateChanged))]
        private NetworkBool _isAlive { get; set; }

        [Networked] private TickTimer _respawnTimer { get; set; }

        private Collider[] _hits = new Collider[1];

        private GameStateController _gameStateController;

        public override void Spawned()
        {
            _visualController = GetComponent<SpaceshipVisualController>();

            // --- State authority only
            if (Object.HasStateAuthority == false) return;

            // Set the local runtime references.
            _rigidbody = GetComponent<Rigidbody>();
            _playerDataNetworked = GetComponent<PlayerDataNetworked>();

            // The Game Session SPECIFIC settings are initialized
            _isAlive = true;
            _gameStateController = FindObjectOfType<GameStateController>();
        }

        // React to _isAlive changing
        private static void OnAliveStateChanged(Changed<SpaceshipController> spaceshipController)
        {
            // Read the previous value
            spaceshipController.LoadOld();
            var wasAlive = spaceshipController.Behaviour._isAlive;

            // Read the current value
            spaceshipController.LoadNew();
            var isAlive = spaceshipController.Behaviour._isAlive;

            spaceshipController.Behaviour.ToggleVisuals(wasAlive, isAlive);
        }

        private void ToggleVisuals(bool wasAlive, bool isAlive)
        {
            // Check if the spaceship was just brought to life
            if (wasAlive == false && isAlive == true)
            {
                _visualController.TriggerSpawn();
            }
            // or whether it just got destroyed.
            else if (wasAlive == true && isAlive == false)
            {
                _visualController.TriggerDestruction();
            }
        }

        public override void FixedUpdateNetwork()
        {
            // --- State authority only
            if (Object.HasStateAuthority == false) return;

            // Checks if the spaceship is ready to be respawned.
            if (_respawnTimer.Expired(Runner) && _gameStateController.GameIsRunning)
            {
                _isAlive = true;
                _respawnTimer = default;
            }

            // Checks if the spaceship got hit by an asteroid
            if (_isAlive && HasHitAsteroid())
            {
                ShipWasHit();
            }
        }

        // Check asteroid collision using a lag compensated OverlapSphere
        private bool HasHitAsteroid()
        {
            var count = Runner.GetPhysicsScene().OverlapSphere(_rigidbody.position, _spaceshipDamageRadius, _hits,
                _asteroidCollisionLayer.value, QueryTriggerInteraction.UseGlobal);

            if (count <= 0) return false;

            var asteroidBehaviour = _hits[0].GetComponent<AsteroidBehaviour>();

            if (asteroidBehaviour.IsAlive == false)
                return false;

            asteroidBehaviour.HitAsteroid(PlayerRef.None);

            return true;
        }

        // Toggle the _isAlive boolean if the spaceship was hit and check whether the player has any lives left.
        // If they do, then the _respawnTimer is activated.
        private void ShipWasHit()
        {
            _isAlive = false;

            ResetShip();

            if (_playerDataNetworked.Lives > 1)
            {
                _respawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnDelay);
            }
            else
            {
                _respawnTimer = default;
            }

            _playerDataNetworked.SubtractLife();

            // If testing the game with only one client, the game will end after the first hit.
            FindObjectOfType<GameStateController>().RPC_CheckIfGameHasEnded();
        }



        // Resets the spaceships movement velocity
        private void ResetShip()
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}