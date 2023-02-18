using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Asteroids.SharedSimple
{
    // The AsteroidBehaviour holds in the information about the asteroid
    public class AsteroidBehaviour : NetworkBehaviour
    {

        // The _points variable can be a local private variable as it will only be used to add points to the score
        // The score itself is networked and any increase or decrease will be propagated automatically.
        [SerializeField] private int _points = 1;

        // The IsBig variable is Networked as it can be used to evaluate and derive visual information for an asteroid locally.
        [HideInInspector] [Networked] public NetworkBool IsBig { get; set; }

        private TickTimer _despawnTimer;
        private bool _wasHit;
        private NetworkTransform _nt;

        public bool IsAlive => !_wasHit;

        public override void Spawned()
        {
            _wasHit = false;
            
            if (!_nt)
                _nt = GetComponent<NetworkTransform>();
            
            _nt.InterpolationTarget.localScale = Vector3.one;
            _nt.TeleportToPositionRotation(transform.position, transform.rotation);
        }

        // When the asteroid gets hit by another object, this method is called to decide what to do next.
        public void HitAsteroid(PlayerRef player)
        {
            // For instant feedback on the player who shot.
            _wasHit = true;
            _despawnTimer = TickTimer.CreateFromSeconds(Runner, .2f);

            // All players detect the collision, but only the owner of the bullet should process the following
            if (Object == null || (player != PlayerRef.None && player != Runner.LocalPlayer)) return;

            // If this hit was triggered by a projectile, the player who shot it gets points
            // The player object is retrieved via the Runner.
            if (Runner.TryGetPlayerObject(player, out var playerNetworkObject))
            {
                playerNetworkObject.GetComponent<PlayerDataNetworked>().AddToScore(_points);
            }

            // The asteroid hit call a RPC to all player to proper deal with it.
            RPC_HitAsteroid();
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority && _wasHit && _despawnTimer.Expired(Runner))
            {
                _wasHit = false;
                if (Object.HasStateAuthority)
                {
                    // Big asteroids tell the AsteroidSpawner to spawn multiple small asteroids as it breaks up.

                    if (IsBig)
                    {
                        FindObjectOfType<AsteroidSpawner>().BreakUpBigAsteroid(transform.position);
                    }

                    Runner.Despawn(Object);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public override void Render()
        {
            // This shrink animation is used to give an instant feedback on the local side.
            if (_wasHit && _despawnTimer.IsRunning)
            {
                _nt.InterpolationTarget.localScale *= .95f;
            }
        }

        //Targets all players to make sure they detect the hit and react to it.
        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_HitAsteroid()
        {
            _wasHit = true;
            // No problem on reseting the ticktimer for the bullet state authority how has already started it.
            _despawnTimer = TickTimer.CreateFromSeconds(Runner, .2f);
        }
    }
}