using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public static void AddScoreForPlayer( int score, Vector3 position, PlayerRef player )
    {
        var scoreObject = Instance.GetScoreObjectFor( player );
        if( scoreObject != null && score > 0 )
        {
            scoreObject.AddScore( score, position, player );
        }
    }


    [SerializeField] UiScoreManager _uiManager;
    Dictionary<PlayerRef, ScoreObject> _scoreObjects = new Dictionary<PlayerRef, ScoreObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void Register( ScoreObject score )
    {
        _scoreObjects.Add( score.Object.InputAuthority, score );

        if( _uiManager != null )
        {
            _uiManager.OnPlayerJoined( score.Object.InputAuthority, score );
        }
    }

    public void Unregister( ScoreObject score )
    {
        if( score.Object == null || _scoreObjects.ContainsKey( score.Object.InputAuthority ) == false )
        {
            return;
        }
        if( _uiManager != null )
        {
            _uiManager.OnPlayerLeft( score );
        }
        _scoreObjects.Remove( score.Object.InputAuthority );
    }

    public ScoreObject GetScoreObjectFor( PlayerRef player )
    {
        if( _scoreObjects.ContainsKey( player ) == false )
        {
            return null;
        }
        return _scoreObjects[ player ];
    }

    public void OnScoreChanged( ScoreObject scoreObject, int score )
    {
        if( _uiManager != null )
        {
            _uiManager.OnScoreChanged( scoreObject, score );
        }
    }


}
