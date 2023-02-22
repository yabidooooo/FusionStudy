using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SpatialTracking;
using System;

public class LocalController : MonoBehaviour
{
    [SerializeField] bool _showDebugValues = false;
    [SerializeField] Transform _relativeTo;

    [SerializeField] InputActionAsset _actionMap;

    [Header( "Input Actions:" )]
    [Header( "Events" )]
    [SerializeField] InputActionProperty _grab;
    [SerializeField] InputActionProperty _drop;
    [SerializeField] InputActionProperty _teleport;
    [Header( "States" )]
    [SerializeField] InputActionProperty _teleportMode;

    private void Awake()
    {
        if( _relativeTo == null )
        {
            _relativeTo = transform.parent;
        }
    }
    protected void OnEnable()
    {
        _actionMap.Enable();
    }

    protected void OnDisable()
    {
        _actionMap.Disable();
    }

    public Vector3 GetLocalPosition()
    {
        return _relativeTo.InverseTransformPoint( transform.position );
    }
    public Quaternion GetLocalRotation()
    {
        return Quaternion.Inverse( _relativeTo.rotation ) * transform.rotation;
    }

    public void UpdateInput( ref InputDataController container )
    {
        container.Actions ^= _grab.action.triggered ? InputAction.GRAB : 0; // xor to flip the corresponding bit
        container.Actions ^= _drop.action.triggered ? InputAction.DROP : 0;
        container.Actions ^= _teleport.action.triggered ? InputAction.TELEPORT : 0;
    }

    public void UpdateInputFixed( ref InputDataController container )
    {
        container.LocalPosition = GetLocalPosition();
        container.LocalRotation = GetLocalRotation();

        container.States |= _teleportMode.action.ReadValue<float>() > InputSystem.settings.defaultButtonPressPoint ? InputState.TELEPORT_MODE : 0;

        if( _showDebugValues )
        {
            Debug.Log( gameObject.name + "= State: " + Convert.ToString( (uint)container.States, 2 ) + "\t  Events:" + Convert.ToString( (uint)container.Actions, 2 ) );
            if( _grab.action.triggered ) Debug.Log( gameObject.name + "Grab was pressed " + Time.frameCount );
            if( _drop.action.triggered ) Debug.Log( gameObject.name + "Drop was pressed " + Time.frameCount );
            if( _teleportMode.action.triggered ) Debug.Log( gameObject.name + "TeleportMode active" + Time.frameCount );
            if( _teleport.action.triggered ) Debug.Log( gameObject.name + "Teleport performed" + Time.frameCount );
        }

    }
}
