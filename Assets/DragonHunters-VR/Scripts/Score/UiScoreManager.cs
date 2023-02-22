using System.Collections.Generic;
using UnityEngine;

public class UiScoreManager : MonoBehaviour
{
    Dictionary<ScoreObject, List<UIScoreElement>> _scoreUi = new Dictionary<ScoreObject, List<UIScoreElement>>();
    [SerializeField] UIScoreElement _prefab;


    private void Awake()
    {
        int childcount = transform.childCount;
        for( int i = 0; i < childcount; ++i )
        {
            Destroy( transform.GetChild( i ).gameObject );
        }
    }

    public void OnScoreChanged( ScoreObject scoreObject, int score )
    {
        if( _scoreUi.ContainsKey( scoreObject ) == false )
        {
            return;
        }
        var list = _scoreUi[ scoreObject ];

        foreach( var item in list )
        {
            item.UpdateScore( score );
        }
    }

    public void OnPlayerJoined( Fusion.PlayerRef player, ScoreObject scoreObject )
    {
        UIScoreElement element = Instantiate( _prefab, transform );

        if( _scoreUi.ContainsKey( scoreObject ) == false )
        {
            _scoreUi.Add( scoreObject, new List<UIScoreElement>() );
        }
        _scoreUi[ scoreObject ].Add( element );
        element.UpdateScore( 0 );
        element.UpdatePlayerName( "P" + player.PlayerId );
    }

    public void OnPlayerLeft( ScoreObject scoreObject )
    {
        if( _scoreUi.ContainsKey( scoreObject ) )
        {
            foreach( var item in _scoreUi[ scoreObject ] )
            {
                if( item != null )
                {
                    Destroy( item.gameObject );
                }
            }
            _scoreUi[ scoreObject ].Clear();
        }
    }
}
