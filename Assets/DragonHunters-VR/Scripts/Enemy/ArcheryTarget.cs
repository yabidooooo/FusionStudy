using UnityEngine;
using Fusion;

public class ArcheryTarget : MonoBehaviour, ITarget
{
    [System.Serializable]
    struct Rings
    {
        public float Radius;
        public int Score;
    }

    [SerializeField] Transform _centerTransform;
    [SerializeField] Rings[] _rings;

    public void OnHit( Vector3 position, Vector3 hitDirection, int hitboxIndex, PlayerRef player )
    {
        Plane plane = new Plane( _centerTransform.forward, _centerTransform.position );
        Ray ray = new Ray( position, hitDirection );
        plane.Raycast( ray, out var rayDist );

        position = ray.GetPoint( rayDist );

        float Distance = Vector3.Distance( position, _centerTransform.position );
        int score = 0;
        for( int i = 0; i < _rings.Length; ++i )
        {
            if( Distance < _rings[ i ].Radius )
            {
                score = _rings[ i ].Score;
                break;
            }
        }

        ScoreManager.AddScoreForPlayer( score, position, player );
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if( _rings == null || _centerTransform == null )
        {
            return;
        }

        Gizmos.matrix = Matrix4x4.TRS( _centerTransform.position, _centerTransform.rotation, Vector3.up + Vector3.right );
        Gizmos.color = Color.magenta;
        for( int i = 0; i < _rings.Length; ++i )
        {
            Gizmos.DrawWireSphere( Vector3.zero, _rings[ i ].Radius );
        }
    }
#endif
}
