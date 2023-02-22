using System.Collections;
using UnityEngine;
using Fusion;

[RequireComponent( typeof( Highlightable ) )]
[OrderAfter( typeof( Hand ), typeof( NetworkTransform ), typeof( NetworkRigidbody ), typeof( HighlightCollector ) )]
public class GrabbableObject : NetworkBehaviour
{
    [Networked] protected Hand _holdingHand { get; set; }
    [Networked] protected Vector3 _positionOffset { get; set; }
    [Networked] protected Quaternion _rotationOffset { get; set; }

    protected Rigidbody _body;
    protected NetworkRigidbody _networkBody;

    [SerializeField] float _throwForce = 2f;

    protected Highlightable _highlight { get; private set; }

    [SerializeField] bool _keepRotationOffsetOnGrab = true;
    [SerializeField] bool _keepPositionOffsetOnGrab = true;

    protected virtual void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _networkBody = GetComponent<NetworkRigidbody>();
        _body.maxAngularVelocity = Mathf.Infinity;

        _highlight = GetComponent<Highlightable>();
        _highlight.GrabCallback += OnGrab;
        _highlight.DropCallback += OnDrop;
    }

    protected virtual void OnDestroy()
    {
        if( _highlight != null )
        {
            _highlight.GrabCallback -= OnGrab;
            _highlight.DropCallback -= OnDrop;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if( _holdingHand != null )
        {
            Vector3 targetPosition = _holdingHand.transform.position + _holdingHand.transform.TransformDirection( _positionOffset );
            _body.velocity = ( targetPosition - _body.position ) / Runner.DeltaTime;

            Quaternion targetRotation = _holdingHand.transform.rotation * _rotationOffset;
            Quaternion rotationDelta = targetRotation * Quaternion.Inverse( _body.rotation );
            rotationDelta.ToAngleAxis( out var angleInDegrees, out var rotationAxis );
            if( angleInDegrees > 180f )
                angleInDegrees -= 360f;

            var angularVelocity = ( rotationAxis * angleInDegrees * Mathf.Deg2Rad ) / Runner.DeltaTime;
            if( float.IsNaN( angularVelocity.x ) == false )
            {
                _body.angularVelocity = angularVelocity;
            }
        }
    }

    void OnGrab( Hand other )
    {
        if( _holdingHand != null )
        {
            _holdingHand.Drop();
        }
        _holdingHand = other;

        if( _keepRotationOffsetOnGrab )
        {
            _rotationOffset = Quaternion.Inverse( _holdingHand.transform.rotation ) * _body.rotation;
        }
        else
        {
            _rotationOffset = Quaternion.identity;
        }

        if( _keepPositionOffsetOnGrab )
        {
            _positionOffset = _holdingHand.transform.InverseTransformDirection( _body.position - _holdingHand.transform.position );
        }
        else
        {
            _positionOffset = Vector3.zero;
        }
    }


    void OnDrop()
    {
        if( _holdingHand != null && _holdingHand.VelocityBuffer != null )
        {
            _body.velocity = _holdingHand.VelocityBuffer.GetAverageVelocity() * _throwForce;
        }
        else
        {
            _body.velocity = _body.velocity * _throwForce;
        }

        _holdingHand = null;
    }

}
