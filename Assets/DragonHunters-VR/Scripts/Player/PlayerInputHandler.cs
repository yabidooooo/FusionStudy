using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    InputData _data;

    [SerializeField] bool _onlySendInputWhenFocused;
    [SerializeField] Transform _relativeTo;
    [SerializeField] Transform _head;
    public LocalController LeftController;
    public LocalController RightController;

    private void Awake()
    {
        if( _relativeTo == null )
        {
            _relativeTo = transform.parent;
        }
    }

    void Start()
    {
        var networkedParent = GetComponentInParent<NetworkObject>();
        if( networkedParent == null || networkedParent.Runner == null )
        {
            return;
        }

        var runner = networkedParent.Runner;
        var events = runner.GetComponent<NetworkEvents>();

        events.OnInput.AddListener( OnInput );

        var player = networkedParent.GetComponent<Player>();
        if( player != null )
        {
            player._leftHand.SetLocalController( LeftController );
            player._rightHand.SetLocalController( RightController );
        }
    }

    private void Update()
    {
        LeftController?.UpdateInput( ref _data.Left );
        RightController?.UpdateInput( ref _data.Right );
    }

    void OnInput( NetworkRunner runner, NetworkInput inputContainer )
    {
        if( _onlySendInputWhenFocused && Application.isFocused == false )
        {
            return;
        }
        _data.HeadLocalPosition = _relativeTo.InverseTransformPoint( _head.position );
        _data.HeadLocalRotation = Quaternion.Inverse( _relativeTo.rotation ) * _head.rotation;

        LeftController?.UpdateInputFixed( ref _data.Left );
        RightController?.UpdateInputFixed( ref _data.Right );

        inputContainer.Set( _data );

        _data.ResetStates();
    }
}
