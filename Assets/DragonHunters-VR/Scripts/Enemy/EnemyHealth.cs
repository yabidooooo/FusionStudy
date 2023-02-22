using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Fusion;

public class EnemyHealth : SimulationBehaviour, ITarget
{
    [SerializeField] NetworkedAudioSource _hitAudio;
    [SerializeField] GameObject _hitVfxPrefab;

    [SerializeField] int[] _hitboxDamage;

    HitboxRoot _hitboxRoot;

    private void Awake()
    {
        _hitboxRoot = GetComponent<HitboxRoot>();
        Debug.Assert( _hitboxRoot != null, "Could not find hitbox root on EnemyHealth.", gameObject );
        Debug.Assert( _hitboxRoot.Hitboxes.Length == _hitboxDamage.Length, "Hitbox damage needs to be same length as hitboxes in root", gameObject );
    }

    public void OnHit( Vector3 position, Vector3 hitDirection, int hitboxIndex, PlayerRef player )
    {
        ScoreManager.AddScoreForPlayer( _hitboxDamage[ hitboxIndex ], position, player );;

        if( _hitAudio != null )
        {
            _hitAudio.PlayOneShot();
        }

        if( Runner.IsForward && _hitVfxPrefab != null )
        {
            var hitbox = _hitboxRoot.Hitboxes[ hitboxIndex ];
            var go = Instantiate( _hitVfxPrefab, position, Quaternion.LookRotation( hitDirection ) );
            go.transform.SetParent( hitbox.transform );
        }
    }
}
