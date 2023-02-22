using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PCInput : MonoBehaviour
{
    [SerializeField] InputActionProperty _move;
    [SerializeField] InputActionProperty _look;

    [SerializeField] Transform _rootTransform;
    [SerializeField] Transform _cameraTransform;

    [SerializeField] Vector2 _lookSpeed = new Vector2( 0.08f, -0.08f );
    [SerializeField] Vector2 _moveSpeed = new Vector2( 3f, 3f );
    [SerializeField] float _acceleration = 4f;

    Vector2 _currentMoveDelta;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        if( Application.isFocused == false )
        {
            return;
        }

        var moveDelta = _move.action.ReadValue<Vector2>();

        _currentMoveDelta = Vector2.MoveTowards( _currentMoveDelta, moveDelta, Time.deltaTime * _acceleration );

        _rootTransform.Translate( _currentMoveDelta.x * _moveSpeed.x * Time.deltaTime, 0f, _currentMoveDelta.y * _moveSpeed.y * Time.deltaTime, Space.Self );

        var lookDelta = _look.action.ReadValue<Vector2>();
        _rootTransform.Rotate( 0f, lookDelta.x * _lookSpeed.x, 0f, Space.Self );
        _cameraTransform.Rotate( lookDelta.y * _lookSpeed.y, 0f, 0f, Space.Self );
    }
}
