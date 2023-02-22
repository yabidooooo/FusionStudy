using Fusion;
using UnityEngine;

public class ScoreObject : NetworkBehaviour
{
    [Networked( OnChanged = nameof( OnScoreChanged ) )]
    int _score { get; set; }

    [SerializeField] ScoreNumber _scorePrefab;

    public override void Spawned()
    {
        if( ScoreManager.Instance != null )
        {
            ScoreManager.Instance.Register( this );
        }
    }

    public override void Despawned( NetworkRunner runner, bool hasState )
    {
        if( ScoreManager.Instance != null )
        {
            ScoreManager.Instance.Unregister( this );
        }
    }

    public void AddScore( int value, Vector3 position, PlayerRef player )
    {
        _score += value;

        if( Object.HasStateAuthority )
        {
            SpawnScoreNumberRPC( value, position, player );
        }
    }

    // The score number effect is purely a short-lived visual effect that has no impact on gameplay
    // and has no relevance after it is created so it would be overkill to make it a NetworkObject.
    // Instead we use an RPC to spawn a local object on the client that scored.
    // Note that this is only required because the score number is supposed to spawn after the 
    // state Authority confirmed the hit. Otherwise it could just be instantiated
    // in prediction ( in AddScore() with if(Runner.IsForward) {...} ) without needing an RPC or any
    // additional networked data
    [Rpc]
    void SpawnScoreNumberRPC( int value, Vector3 position, [RpcTarget] PlayerRef player )
    {
        var score = Instantiate( _scorePrefab , position, Quaternion.LookRotation( transform.position - position ) );
        score.Setup( value );
    }

    static void OnScoreChanged( Changed<ScoreObject> changed )
    {
        ScoreManager.Instance?.OnScoreChanged( changed.Behaviour, changed.Behaviour._score );
    }
}
