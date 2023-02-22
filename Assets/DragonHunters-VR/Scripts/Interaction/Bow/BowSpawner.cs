using UnityEngine;
using Fusion;

public class BowSpawner : MonoBehaviour
{
    [SerializeField] Bow _prefab;

    public void Spawn( Hand hand )
    {
        if( hand.Object.HasStateAuthority )
        {
            if( RetrieveOtherBowHighlight( hand, out var otherHighlightable ) )
            {
                hand.OtherHand.Drop();
                hand.ForceGrab( otherHighlightable );
            }
            else
            {
                Bow bow = hand.Runner.Spawn( _prefab, transform.position, transform.rotation );
                hand.ForceGrab( bow.Highlight );
            }
        }
    }

    bool RetrieveOtherBowHighlight( Hand hand, out Highlightable highlightable )
    {
        if( hand == null || hand.OtherHand == null || hand.OtherHand.CurrentlyGrabbedHighlight == null )
        {
            highlightable = null;
            return false;
        }

        highlightable = hand.OtherHand.CurrentlyGrabbedHighlight;
        var bow = highlightable.GetComponent<Bow>();
        return bow != null;
    }
}
