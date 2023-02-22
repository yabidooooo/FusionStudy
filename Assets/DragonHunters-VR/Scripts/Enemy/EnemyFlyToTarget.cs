using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class EnemyFlyToTarget : NetworkBehaviour
{
    DragonBlendTree _animBlendTree;
    int _flyPoseId;

    [Networked] Vector3 _velocity { get; set; }
    [Networked] Vector3 _upVector { get; set; }

    [SerializeField] Transform _target;
    [SerializeField] float _acceleration = 10f;
    [SerializeField] float _leanAcceleration = 1f;
    [SerializeField] float _leanIntensity = 10f;

    [SerializeField] float _maxVelocity = 2f;
    [SerializeField] float _minVelocitySqr = 5f;
    [SerializeField] Vector2 _hoverThresholds;

    private void Awake()
    {
        _animBlendTree = GetComponentInChildren<DragonBlendTree>();
        _flyPoseId = Animator.StringToHash( "FlyPose" );
    }

    public override void FixedUpdateNetwork()
    {
        _velocity *= 0.9f;
        _velocity += Vector3.ClampMagnitude( _target.position - transform.position, _maxVelocity ) * Runner.DeltaTime * _acceleration;

        if( _velocity.sqrMagnitude < _minVelocitySqr )
        {
            _velocity = _velocity.normalized * _minVelocitySqr * _minVelocitySqr;
        }

        Vector3 upVector = Vector3.up + ( _velocity - transform.forward * _velocity.magnitude ) * _leanIntensity;

        Vector3 forward = _velocity;

        _upVector = Vector3.MoveTowards( _upVector, upVector, Runner.DeltaTime * _leanAcceleration );
        Quaternion targetRot = Quaternion.LookRotation( forward, _upVector );

        transform.rotation = targetRot;
        transform.position += _velocity * Runner.DeltaTime;

        UpdateAnimation( AnimationHelper.GetAnimationTime( Runner ) );
    }

    float SmoothStep( float a, float b, float x ) // Shader-like smoothstep
    {
        var t = Mathf.Clamp01( ( x - a ) / ( b - a ) );
        return t * t * ( 3f - ( 2f * t ) );
    }

    void UpdateAnimation( double time )
    {
        float animationFactor = SmoothStep( _hoverThresholds.x, _hoverThresholds.y, _velocity.magnitude );
        _animBlendTree.SetAnimationTimeSpeed( time, animationFactor );
    }

    public override void Render()
    {
        if( Object.IsProxy )
        {
            UpdateAnimation( AnimationHelper.GetAnimationTimeProxy( Runner ) );
        }
        else
        {
            UpdateAnimation( AnimationHelper.GetAnimationTime( Runner ) );
        }

#if UNITY_EDITOR
        Debug.DrawLine( transform.position, _target.position, Color.blue );
        Debug.DrawLine( transform.position, transform.position + _velocity * 10f, Color.red );
#endif
    }

}
