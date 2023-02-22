using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Player : NetworkBehaviour, IPlayerLeft
{
    [SerializeField] Transform _head;
    [SerializeField] GameObject _headVisuals;
    public Hand _leftHand;
    public Hand _rightHand;

    [SerializeField] Renderer[] _renderers;
    [SerializeField] Material _localHandMaterial;

    public override void Spawned()
    {
        if( Object.HasInputAuthority )
        {
            _headVisuals.SetActive( false );

            foreach( var rend in _renderers )
            {
                rend.sharedMaterial = _localHandMaterial;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if( GetInput<InputData>( out var input ) )
        {
            _leftHand.UpdateInput( input.Left );
            _rightHand.UpdateInput( input.Right );
            _head.localPosition = input.HeadLocalPosition;
            _head.localRotation = input.HeadLocalRotation;
            
            Runner.AddPlayerAreaOfInterest( Runner.LocalPlayer, _head.position, 1f );
        }
    }

    public void PlayerLeft( PlayerRef player )
    {
        if( player == Object.InputAuthority )
        {
            Runner.Despawn( Object );
        }
    }
}
