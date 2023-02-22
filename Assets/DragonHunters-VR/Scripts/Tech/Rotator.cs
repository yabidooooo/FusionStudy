using UnityEngine;
using Fusion;

[RequireComponent( typeof( NetworkTransform ) )]
public class Rotator : SimulationBehaviour
{
    [SerializeField] Vector3 _rotationSpeed;
    public override void FixedUpdateNetwork()
    {
        transform.Rotate( _rotationSpeed * Runner.DeltaTime );
    }
}
