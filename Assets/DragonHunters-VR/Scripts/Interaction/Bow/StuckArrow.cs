using UnityEngine;
using Fusion;
using System.Collections.Generic;

[OrderAfter( typeof( NetworkTransform ), typeof( NetworkRigidbody ) )]
public class StuckArrow : NetworkBehaviour, IPredictedSpawnBehaviour, IAfterClientPredictionReset
{
    Arrow _arrow;
    [SerializeField] float _maxPenetration = 0.4f;
    [SerializeField] float _minPenetration = 0.1f;
    [SerializeField] float _impactForce = 0.25f;
    [SerializeField] int _stuckArrowLimit = 10;

    static List<StuckArrow> s_StuckArrows = new List<StuckArrow>();


    NetworkTransform _networkTransform;

    [Networked( OnChanged = nameof( OnStuckChanged ) )] NetworkBool _networkedStuck { get; set; }
    bool _predictedStuck;
    public bool Stuck
    {
        get => Object.IsPredictedSpawn ? _predictedStuck : (bool)_networkedStuck;
        private set { if( Object.IsPredictedSpawn ) _predictedStuck = value; else _networkedStuck = value; }
    }

    [Networked] NetworkTransform _networkedStuckParent { get; set; }
    NetworkTransform _predictedStuckParent;
    NetworkTransform _stuckParent
    {
        get => Object.IsPredictedSpawn ? _predictedStuckParent : _networkedStuckParent;
        set { if( Object.IsPredictedSpawn ) _predictedStuckParent = value; else _networkedStuckParent = value; }
    }

    [Networked] Vector3 _networkedStuckPosition { get; set; }
    Vector3 _predictedStuckPosition;
    Vector3 _stuckPosition
    {
        get => Object.IsPredictedSpawn ? _predictedStuckPosition : _networkedStuckPosition;
        set { if( Object.IsPredictedSpawn ) _predictedStuckPosition = value; else _networkedStuckPosition = value; }
    }

    [Networked] Quaternion _networkedStuckRotation { get; set; }
    Quaternion _predictedStuckRotation;
    Quaternion _stuckRotation
    {
        get => Object.IsPredictedSpawn ? _predictedStuckRotation : _networkedStuckRotation;
        set { if( Object.IsPredictedSpawn ) _predictedStuckRotation = value; else _networkedStuckRotation = value; }
    }

    public override void Spawned()
    {
        UpdateStuck();
    }

    public void InitNetworkState()
    {
        Stuck = false;
        _stuckParent = null;

    }

    private void Awake()
    {
        _arrow = GetComponent<Arrow>();
        _networkTransform = GetComponent<NetworkTransform>();
    }

    public void AfterClientPredictionReset()
    {
        UpdateStuck();
    }

    public void GetStuck( Fusion.LagCompensatedHit hitInfo, Vector3 velocity )
    {
        if( Stuck )
        {
            return;
        }

        Stuck = true;

        _stuckParent = hitInfo.GameObject.GetComponentInParent<NetworkTransform>();


        var hitPoint = hitInfo.Point + transform.forward * Mathf.Clamp( velocity.magnitude, _minPenetration, _maxPenetration );
        transform.position = hitPoint;

        ITarget target = null;

        if( hitInfo.Collider != null )
        {
            var rb = hitInfo.Collider.attachedRigidbody;
            if( rb != null )
            {
                rb.AddForceAtPosition( velocity * _impactForce, hitInfo.Point, ForceMode.Impulse );
                target = rb.GetComponent<ITarget>();
            }
            else
            {
                target = hitInfo.Collider.GetComponent<ITarget>();
            }
        }

        if( target == null && hitInfo.Type == HitType.Hitbox )
        {
            target = hitInfo.Hitbox.Root.GetComponent<ITarget>();
        }

        if( target != null )
        {
            int hitboxIndex = hitInfo.Hitbox != null ? hitInfo.Hitbox.HitboxIndex : -1;
            target.OnHit( hitInfo.Point, hitInfo.Normal, hitboxIndex, Object.InputAuthority );
        }

        if( _stuckParent != null )
        {
            if( _stuckParent is NetworkRigidbody )
            {
                var rb = _stuckParent as NetworkRigidbody;
                _stuckParent.transform.position = rb.Rigidbody.position;
                _stuckParent.transform.rotation = rb.Rigidbody.rotation;
                Debug.Log( "Update transform to rb" );
            }
            _stuckPosition = _stuckParent.transform.InverseTransformPoint( hitPoint );
            _stuckRotation = Quaternion.Inverse( _stuckParent.transform.rotation ) * transform.rotation;
        }

        UpdateStuck();
    }

    static void OnStuckChanged( Changed<StuckArrow> changed )
    {
        if( changed.Behaviour._networkedStuck == true )
        {
            changed.Behaviour.OnArrowStuckChanged();
        }
    }

    void OnArrowStuckChanged()
    {
        s_StuckArrows.Add( this );
        while( s_StuckArrows.Count > _stuckArrowLimit && s_StuckArrows.Count > 0 )
        {
            Runner.Despawn( s_StuckArrows[ 0 ].Object );
            s_StuckArrows.RemoveAt( 0 );
        }
    }

    void UpdateStuck()
    {

        if( _stuckParent != null )
        {
            if( _networkTransform.InterpolationTarget.parent != _stuckParent.InterpolationTarget )
            {
                _networkTransform.InterpolationTarget.SetParent( _stuckParent.InterpolationTarget );
                _networkTransform.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
            }
            _networkTransform.InterpolationTarget.localPosition = _stuckPosition;
            _networkTransform.InterpolationTarget.localRotation = _stuckRotation;
        }
        else if( _networkTransform.InterpolationTarget.parent != transform )
        {
            _networkTransform.InterpolationTarget.SetParent( transform );
            _networkTransform.InterpolationDataSource = InterpolationDataSources.Predicted;
            _networkTransform.InterpolationTarget.localPosition = default;
            _networkTransform.InterpolationTarget.localRotation = default;
        }
    }

    public void PredictedSpawnSpawned()
    {
        _stuckParent = null;
    }

    public void PredictedSpawnUpdate()
    {
    }

    public void PredictedSpawnRender()
    {
    }

    public void PredictedSpawnFailed()
    {
    }

    void IPredictedSpawnBehaviour.PredictedSpawnSuccess()
    {
        UpdateStuck();
    }

    public override void Despawned( NetworkRunner runner, bool hasState )
    {
        _stuckParent = null;
        UpdateStuck();
        base.Despawned( runner, hasState );
    }
}
