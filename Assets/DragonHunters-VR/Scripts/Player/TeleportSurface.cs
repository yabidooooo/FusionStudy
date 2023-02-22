using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSurface : MonoBehaviour
{
    public static TeleportSurface Instance;
    [SerializeField] List<Collider> _colliders;
    [SerializeField] LayerMask _mask;

    PhysicsScene _scene;
    private void Awake()
    {
        Instance = this;
        _scene = gameObject.scene.GetPhysicsScene();
    }

    public Vector3? Raycast( Vector3 position, Vector3 direction, float range )
    {
        if( _scene.Raycast( position, direction, out var hitInfo, range, _mask.value ) )
        {
            return _colliders.Contains( hitInfo.collider ) ? new Vector3?( hitInfo.point ) : null;
        }

        return null;
    }

}
