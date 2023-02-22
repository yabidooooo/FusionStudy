using UnityEngine;
using UnityEngine.Events;
using Fusion;

public class Highlightable : NetworkBehaviour
{
    int _highlightCount;
    [SerializeField] GameObject _highlight;
    public bool HandCanDropByReleasing = true;

    public UnityAction<Hand> GrabCallback;
    public UnityAction DropCallback;

    public void Grab( Hand hand )
    {
        StopHighlight();
        GrabCallback?.Invoke( hand );
    }

    public void Drop()
    {
        StartHighlight();
        DropCallback?.Invoke();
    }

    private void Awake()
    {
        _highlight.SetActive( false );
    }

    public void StartHighlight()
    {
        _highlightCount++;
        if( _highlight != null && _highlightCount > 0 )
        {
            _highlight.SetActive( true );
        }
    }
    public void StopHighlight()
    {
        _highlightCount--;
        if( _highlight != null && _highlightCount <= 0 )
        {
            _highlight.SetActive( false );
        }
    }
}
