using UnityEngine;
using Fusion;

[OrderAfter( typeof( HitboxManager ) )]
public class Arrow : NetworkBehaviour, IPredictedSpawnBehaviour
{
    [SerializeField] NetworkedAudioSource _audio;
    [SerializeField] LayerMask _collisionLayers;

    [Networked] Vector3 _networkedVelocity { get; set; }
    Vector3 _predictedVelocity;
    Vector3 Velocity
    {
        get => Object.IsPredictedSpawn ? _predictedVelocity : _networkedVelocity;
        set { if( Object.IsPredictedSpawn ) _predictedVelocity = value; else _networkedVelocity = value; }
    }

    bool Stuck => _stuckArrow.Stuck;

    Vector3 _predictedPreviousTickPosition;
    Quaternion _predictedPreviousTickRotation;


    NetworkTransform _networkTransform;
    Transform _interpolationTarget => _networkTransform.InterpolationTarget;


    readonly Vector3 GRAVITY = new Vector3( 0f, -9.81f, 0f );


    StuckArrow _stuckArrow;

    private void Awake()
    {
        _networkTransform = GetComponent<NetworkTransform>();
        _stuckArrow = GetComponent<StuckArrow>();
    }

    public void InitNetworkState( Vector3 velocity )
    {
        Velocity = velocity;
        _stuckArrow.InitNetworkState();

    }
    public override void FixedUpdateNetwork()
    {
        if( Stuck )
        {
            return;
        }

        MoveArrow();
        CheckOutOfBounds();
    }
    void MoveArrow()
    {
        Velocity = Velocity + Runner.DeltaTime * GRAVITY;
        var pos = transform.position;
        var nextPosition = pos + Velocity * Runner.DeltaTime;
        var direction = nextPosition - pos;


        if( Runner.LagCompensation.Raycast( pos, direction, direction.magnitude, Object.InputAuthority, out var hitinfo, _collisionLayers.value, HitOptions.IncludePhysX ) )
        {
            _audio.PlayOneShot();
            _stuckArrow.GetStuck( hitinfo, Velocity );
        }
        else
        {
            transform.position = nextPosition;
            transform.rotation = Quaternion.LookRotation( direction );
        }
    }

    void CheckOutOfBounds()
    {
        if( transform.position.y < -50f )
        {
            Runner.Despawn( Object );
        }
    }

    void IPredictedSpawnBehaviour.PredictedSpawnUpdate()
    {
        FixedUpdateNetwork();
        _networkTransform.PredictedSpawnCacheTransformState();
    }
    void IPredictedSpawnBehaviour.PredictedSpawnFailed()
    {
        Runner.Despawn( Object, true );
    }
    public void PredictedSpawnSpawned() { }
    void IPredictedSpawnBehaviour.PredictedSpawnSuccess() { }
    public void PredictedSpawnRender() { }

}
