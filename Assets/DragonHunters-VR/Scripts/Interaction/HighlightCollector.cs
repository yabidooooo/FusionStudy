using UnityEngine;
using System.Collections.Generic;
using Fusion;

[OrderAfter( typeof( NetworkRigidbody ), typeof( NetworkTransform ), typeof( Hand ) )]
public class HighlightCollector : NetworkBehaviour
{
    bool _isLocal;

    [SerializeField] float _radius = 0.1f;
    [SerializeField] LayerMask _layerMask;
    [Networked] public Highlightable CurrentHighlight { get; private set; }

    Collider[] _colliderResults = new Collider[ 8 ];
    public override void Spawned()
    {
        _isLocal = Object.InputAuthority == Runner.LocalPlayer;
    }

    public override void FixedUpdateNetwork()
    {

        var count = Runner.GetPhysicsScene().OverlapSphere( transform.position, _radius, _colliderResults, _layerMask, QueryTriggerInteraction.Collide );
        Highlightable closest = null;
        float distance = Mathf.Infinity;

        for( int i = 0; i < count; ++i )
        {
            var col = _colliderResults[ i ];

            Highlightable highlight = null;
            Vector3 position = default;
            if( col.attachedRigidbody != null )
            {
                col.attachedRigidbody.TryGetComponent<Highlightable>( out highlight );
                position = col.attachedRigidbody.position;
            }
            else
            {
                highlight = col.GetComponentInParent<Highlightable>();
                position = col.transform.position;
            }
            if( highlight == null )
            {
                continue;
            }

            float newDistance = Vector3.SqrMagnitude( position - transform.position );
            if( newDistance < distance )
            {
                closest = highlight;
                distance = newDistance;
            }
        }

        if( _isLocal )
        {
            if( CurrentHighlight != null )
            {
                CurrentHighlight.StopHighlight();
            }
            if( closest != null )
            {
                closest.StartHighlight();
            }
        }
        CurrentHighlight = closest;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere( transform.position, _radius );
    }

}
