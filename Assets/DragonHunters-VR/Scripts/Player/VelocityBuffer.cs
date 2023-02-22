using System.Collections.Generic;
using UnityEngine;
using Fusion;

[OrderAfter( typeof( GrabbableObject ) )]
public class VelocityBuffer : NetworkBehaviour
{
    const int VELOCITY_BUFFER_SIZE = 10;
    [Networked] [Capacity( VELOCITY_BUFFER_SIZE )] NetworkArray<Vector3> _ringBuffer => default;
    [Networked] int _bufferIndex { get; set; }
    [Networked] Vector3 _previousPosition { get; set; }


    public override void FixedUpdateNetwork()
    {
        _bufferIndex = ( _bufferIndex + 1 ) % VELOCITY_BUFFER_SIZE;
        _ringBuffer.Set( _bufferIndex, transform.position - _previousPosition );
        _previousPosition = transform.position;
    }
    // todo: angular velocity: https://answers.unity.com/questions/49082/rotation-quaternion-to-angular-velocity.html
    public Vector3 GetAverageVelocity()
    {
        Vector3 sum = new Vector3();

        for( int i = 0; i < VELOCITY_BUFFER_SIZE; ++i )
        {
            sum += _ringBuffer[ i ];
        }

        return ( sum / VELOCITY_BUFFER_SIZE ) / Runner.DeltaTime;
    }
}
