using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fusion;

[RequireComponent( typeof( HighlightCollector ) )]
[RequireComponent( typeof( VelocityBuffer ) )]
public class Hand : NetworkBehaviour
{
    HighlightCollector _highlightCollector;
    [Networked] public Highlightable CurrentlyGrabbedHighlight { get; private set; }
    [Networked] NetworkBool _grabbing { get; set; }

    public Hand OtherHand;
    [SerializeField] Transform _visuals;
    public VelocityBuffer VelocityBuffer { get; private set; }

    TeleportHandler _teleportHandler;

    public LocalController LocalController { get; set; }

    [Networked]
    InputAction _previousInputAction { get; set; }

    private void Awake()
    {
        VelocityBuffer = GetComponent<VelocityBuffer>();
        _highlightCollector = GetComponent<HighlightCollector>();
        _teleportHandler = GetComponentInChildren<TeleportHandler>();
    }

    public void SetLocalController( LocalController other )
    {
        LocalController = other;

        if( LocalController != null )
        {
            var nt = GetComponent<NetworkTransform>();
            nt.InterpolationDataSource = InterpolationDataSources.NoInterpolation;
        }
    }

    public void UpdateInput( InputDataController input )
    {
        _previousInputAction = input.PreprocessActions( _previousInputAction );
        UpdatePose( input.LocalPosition, input.LocalRotation );

        if( input.GetAction( InputAction.GRAB ) )
        {
            if( CurrentlyGrabbedHighlight == null )
            {
                Grab();
            }
            else
            {
                CurrentlyGrabbedHighlight.Grab( this );
            }
        }

        if( _grabbing && input.GetAction( InputAction.DROP ) && CanDropActiveHighlight() )
        {
            Drop();
        }

        _teleportHandler?.UpdateInput( input );

        _visuals.localScale = _grabbing ? Vector3.one * 0.8f : Vector3.one;
    }

    bool CanDropActiveHighlight()
    {
        return CurrentlyGrabbedHighlight == null || CurrentlyGrabbedHighlight.HandCanDropByReleasing;
    }

    void UpdatePose( Vector3 localPosition, Quaternion localRotation )
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }
    public void UpdateLocalPose( Vector3 localPosition, Quaternion localRotation )
    {
        _visuals.position = transform.parent.TransformPoint( localPosition );
        _visuals.rotation = transform.parent.rotation * localRotation;
    }

    public Vector3 GetWorldPosition()
    {
        if( LocalController != null )
        {
            return transform.parent.TransformPoint( LocalController.GetLocalPosition() );
        }
        return transform.position;
    }
    public Quaternion GetWorldRotation()
    {
        if( LocalController != null )
        {
            return transform.parent.rotation * LocalController.GetLocalRotation();
        }
        return transform.rotation;
    }

    public void ForceGrab( Highlightable highlightable )
    {
        Drop();
        CurrentlyGrabbedHighlight = highlightable;
        highlightable.Grab( this );
        _grabbing = true;
    }

    void Grab()
    {

        if( _highlightCollector.CurrentHighlight != null )
        {
            ForceGrab( _highlightCollector.CurrentHighlight );
        }
        else
        {
            CurrentlyGrabbedHighlight = null;
            _grabbing = true;
        }
    }

    public void Drop()
    {
        if( CurrentlyGrabbedHighlight != null )
        {
            CurrentlyGrabbedHighlight.Drop();
            CurrentlyGrabbedHighlight = null;
        }

        _grabbing = false;
    }

    public override void Render()
    {
        if( LocalController != null )
        {
            UpdateLocalPose( LocalController.GetLocalPosition(), LocalController.GetLocalRotation() );
        }
    }
    public override void Despawned( NetworkRunner runner, bool hasState )
    {
        if( CurrentlyGrabbedHighlight != null )
        {
            runner.Despawn( CurrentlyGrabbedHighlight.Object );
        }
    }
}
