using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Asteroids.SharedSimple
{
    // Defines how the bullet behaves
    public class BulletBehaviour : NetworkBehaviour
    {
        // The settings
        [SerializeField] private float _maxLifetime = 3.0f;
        [SerializeField] private float _speed = 200.0f;
        [SerializeField] private LayerMask _asteroidLayer;

        // The direction in which the bullet travels.
        [Networked] private Vector3 _direction { get; set; }

        // The countdown for a bullet lifetime.
        // Only the state authority need to manage the timer.
        private TickTimer _currentLifetime { get; set; }

        private bool _hitAsteroid;
        
        private NetworkTransform _nt;

        public override void Spawned()
        {
            if (!_nt)
                _nt = GetComponent<NetworkTransform>();
            
            //Teleport to avoid interpolation between the last position of the bullet when it get recycled from the pool
            _nt.TeleportToPositionRotation(transform.position, transform.rotation);
            
            if (Object.HasStateAuthority == false) return;

            _currentLifetime = TickTimer.CreateFromSeconds(Runner, _maxLifetime);
            // The network parameters get initializes by the state authority. These will be propagated to the other clients since the
            // variables are [Networked]
            _direction = transform.forward;
        }

        public override void FixedUpdateNetwork()
        {
            // Only move or despawns bullets that the local player owns
            if (Object.HasStateAuthority == false) return;

            _hitAsteroid = HasHitAsteroid();

            // If the bullet has not hit an asteroid, moves forward.
            if (_hitAsteroid == false)
            {
                transform.Translate(_direction * _speed * Runner.DeltaTime, Space.World);
            }
            else
            {
                Runner.Despawn(Object);
                return;
            }

            CheckLifetime();
        }

        // If the bullet has exceeded its lifetime, it gets destroyed
        private void CheckLifetime()
        {
            if (_currentLifetime.Expired(Runner) == false) return;

            Runner.Despawn(Object);
        }

        // Check if the bullet will hit an asteroid in the next tick.
        private bool HasHitAsteroid()
        {
            var hitAsteroid = Runner.GetPhysicsScene().Raycast(transform.position, _direction, out var hit,
                _speed * Runner.DeltaTime, _asteroidLayer);

            if (hitAsteroid == false) return false;

            var asteroidBehaviour = hit.collider.GetComponent<AsteroidBehaviour>();

            if (asteroidBehaviour.IsAlive == false)
                return false;

            asteroidBehaviour.HitAsteroid(Object.StateAuthority);

            return true;
        }
    }
}