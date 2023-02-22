using UnityEngine;
using UnityEngine.Events;


public class WorldButton : MonoBehaviour
{
    public UnityEvent OnClicked;
    public UnityEvent<Hand> OnClickedHand;

    Highlightable _highlight;

    private void Awake()
    {
        _highlight = GetComponentInChildren<Highlightable>();
        _highlight.GrabCallback += OnGrab;
    }

    private void OnDestroy()
    {
        if( _highlight != null )
        {
            _highlight.GrabCallback -= OnGrab;
        }
    }

    void OnGrab( Hand hand )
    {
        OnClicked?.Invoke();
        OnClickedHand?.Invoke( hand );
    }
}
