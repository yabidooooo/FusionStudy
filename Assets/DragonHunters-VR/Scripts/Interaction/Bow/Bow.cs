using UnityEngine;
using Fusion;

[OrderAfter( typeof( Hand ) )]
public class Bow : NetworkBehaviour
{
    [Networked] protected Hand BowHand { get; set; }
    [Networked] protected Hand StringHand { get; set; }
    [Networked] protected int ReleaseStartAnimationTick { get; set; }
    [Networked] float PullNormalized { get; set; }


    [SerializeField] Transform _visuals;
    [SerializeField] Transform _visualsString;

    [SerializeField] NetworkedAudioSource _releaseAudio;
    [SerializeField] NetworkedAudioSource _releaseFailAudio;

    [SerializeField] Arrow _arrowPrefab;
    [SerializeField] float _arrowForceFactor = 22f;
    [SerializeField] float _pullAnimationStartOffsetScale = 0.25f;


    [SerializeField] GameObject _arrowPreview;
    [SerializeField] GameObject _arrowPreviewInactive;

    [SerializeField] Highlightable _stringHighlight;
    [SerializeField] Vector2 _stringDistances = new Vector2( 0.12f, 0.715f );
    [SerializeField] float _shootThreshold = 0.4f;

    private Highlightable _highlight;
    public Highlightable Highlight
    {
        get
        {
            if( _highlight == null )
                _highlight = GetComponent<Highlightable>();
            return _highlight;
        }
    }
    BowBlendTree _animBlendTree;
    float _invPullMagnitude;

    public override void Spawned()
    {
        UpdateInterpolation();
    }
    protected virtual void Awake()
    {
        Highlight.GrabCallback += OnGrab;
        Highlight.DropCallback += OnDrop;

        _stringHighlight.GrabCallback += OnStringGrab;
        _stringHighlight.DropCallback += OnStringRelease;

        _animBlendTree = GetComponentInChildren<BowBlendTree>();

        _invPullMagnitude = 1f / ( _stringDistances.y - _stringDistances.x );
    }

    protected virtual void OnDestroy()
    {
        if( Highlight != null )
        {
            Highlight.GrabCallback -= OnGrab;
            Highlight.DropCallback -= OnDrop;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if( BowHand == null && StringHand == null )
        {
            return;
        }

        var bowPosition = BowHand != null ? BowHand.transform.position : transform.position;
        var bowRotation = BowHand != null ? BowHand.transform.rotation : Quaternion.identity;
        var stringPosition = StringHand != null ? StringHand.transform.position : bowPosition + bowRotation * -Vector3.forward * _stringDistances.x;

        UpdateBowPosition( transform, _stringHighlight.transform, bowPosition, bowRotation, stringPosition );

        float distance = _stringDistances.x;
        if( StringHand != null )
        {
            distance = Vector3.Distance( transform.position, StringHand.transform.position );
        }

        PullNormalized = ( distance - _stringDistances.x ) * _invPullMagnitude;

        _arrowPreview.SetActive( StringHand != null );
        _arrowPreviewInactive.SetActive( StringHand == null );
    }

    void UpdateBowPosition( Transform bowTarget, Transform stringTarget, Vector3 bowPosition, Quaternion bowRotation, Vector3 stringPosition )
    {
        bowTarget.position = bowPosition;

        Vector3 up = bowRotation * Vector3.forward;
        Vector3 forward = bowRotation * Vector3.up;

        var distance = _stringDistances.x;
        if( StringHand != null )
        {
            forward = stringPosition - bowPosition;
            distance = Vector3.Distance( bowTarget.position, stringPosition );
        }

        bowTarget.rotation = Quaternion.LookRotation( forward, up );
        stringTarget.position = bowPosition + bowTarget.rotation * Vector3.forward * Mathf.Clamp( distance, _stringDistances.x, _stringDistances.y );
        stringTarget.rotation = bowTarget.rotation;

        var visualPull = Mathf.Clamp01( ( distance - _stringDistances.x ) * _invPullMagnitude );

        if( Object.IsProxy )
        {
            _animBlendTree.SetAnimationTimeSpeed( AnimationHelper.GetAnimationTimeProxy( Runner ), AnimationHelper.GetAnimationTimeProxy( Runner, ReleaseStartAnimationTick ), visualPull );
        }
        else
        {
            _animBlendTree.SetAnimationTimeSpeed( AnimationHelper.GetAnimationTime( Runner ), AnimationHelper.GetAnimationTime( Runner, ReleaseStartAnimationTick ), visualPull );
        }
    }
    public override void Render()
    {
        bool showPreview = false;
        if( BowHand != null && BowHand.Object.HasInputAuthority )
        {
            showPreview = true;
            UpdateBowPosition( _visuals, _visualsString, BowHand.GetWorldPosition(), BowHand.GetWorldRotation(), StringHand == null ? default : StringHand.GetWorldPosition() );
        }
        else if( StringHand != null && StringHand.Object.HasInputAuthority )
        {
            showPreview = true;
            UpdateBowPosition( _visuals, _visualsString, transform.position, Quaternion.identity, StringHand.GetWorldPosition() );
        }

        _arrowPreview.SetActive( StringHand != null && showPreview );
        _arrowPreviewInactive.SetActive( StringHand == null && showPreview );
    }

    void OnStringGrab( Hand other )
    {
        if( StringHand != null )
        {
            StringHand.Drop();
        }
        StringHand = other;
    }

    void OnStringRelease()
    {
        if( PullNormalized > _shootThreshold )
        {
            Shoot();
            _releaseAudio.PlayOneShot();
        }
        else
        {
            _releaseFailAudio.PlayOneShot();
        }
        StringHand = null;

        // Safe when the release animation should start. offset Release time to control at which point Release anim starts depending on pull 
        ReleaseStartAnimationTick = Mathf.FloorToInt( Runner.Simulation.Tick - ( 1f / Runner.DeltaTime ) * ( 1f - PullNormalized ) * _pullAnimationStartOffsetScale );
    }

    void Shoot()
    {
        PlayerRef owner = PlayerRef.None;
        if( BowHand != null )
        {
            owner = BowHand.Object.InputAuthority;
        }
        else if( StringHand != null )
        {
            owner = StringHand.Object.InputAuthority;
        }

        Vector3 forward = -transform.forward;

        Vector3 arrowVelocity = forward * PullNormalized * _arrowForceFactor;
        var key = new NetworkObjectPredictionKey { Byte0 = (byte)Runner.Simulation.Tick, Byte1 = (byte)Object.InputAuthority.RawEncoded, Byte2 = 1 };
        Vector3 spawnPosition = _stringHighlight.transform.position + forward * 0.81f;
        var arrow = Runner.Spawn( _arrowPrefab, spawnPosition, Quaternion.LookRotation( forward ), owner, ( runner, no ) =>
        {
            no.GetComponent<Arrow>().InitNetworkState( arrowVelocity );
        }, key );
    }

    void UpdateInterpolation()
    {
        var nt = GetComponent<NetworkTransform>();
        if( nt != null )
        {
            nt.InterpolationDataSource = Object.HasInputAuthority ? InterpolationDataSources.NoInterpolation : InterpolationDataSources.Snapshots;
        }
    }

    void OnGrab( Hand other )
    {
        // todo: only allow for pc controls
        if( other == BowHand )
        {
            PullNormalized = 1f;
            OnStringRelease();
            return;
        }
        Object.AssignInputAuthority( other.Object.InputAuthority );

        if( BowHand != null )
        {
            BowHand.Drop();
        }
        BowHand = other;

        UpdateInterpolation();

    }

    void OnDrop()
    {
        BowHand = null;
    }
}
