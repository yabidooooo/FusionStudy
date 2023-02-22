using UnityEngine;
using Fusion;

[RequireComponent( typeof( LineRenderer ) )]
public class TeleportHandler : MonoBehaviour
{
    [SerializeField] NetworkTransform _root;
    [SerializeField] Transform _head;

    [SerializeField] Gradient _validGradient;
    [SerializeField] Gradient _invalidGradient;
    [SerializeField] float _range = 10f;

    Vector3? _lastTeleportPoint;
    LineRenderer _line;



    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
    }

    public void UpdateInput( InputDataController input )
    {
        if( input.GetState( InputState.TELEPORT_MODE ) )
        {
            _lastTeleportPoint = TeleportSurface.Instance.Raycast( transform.position, transform.forward, _range );
        }

        if( input.GetAction( InputAction.TELEPORT ) )
        {
            if( _lastTeleportPoint != null && _lastTeleportPoint.HasValue )
            {
                DoTeleport( _lastTeleportPoint.Value );
            }
        }

        UpdateLine( input );
    }

    void UpdateLine( InputDataController input )
    {
        if( _line == null )
        {
            return;
        }

        _line.enabled = input.GetState( InputState.TELEPORT_MODE );
        _line.SetPosition( 0, transform.position );

        if( _lastTeleportPoint.HasValue )
        {
            _line.SetPosition( 1, _lastTeleportPoint.Value );
            _line.colorGradient = _validGradient;
        }
        else
        {
            _line.SetPosition( 1, transform.position + transform.forward * _range );
            _line.colorGradient = _invalidGradient;
        }
    }

    void DoTeleport( Vector3 target )
    {
        Vector3 headDelta = _head.position - _root.transform.position;
        headDelta.y = 0f;
        _root.TeleportToPosition( target - headDelta );
    }
}
