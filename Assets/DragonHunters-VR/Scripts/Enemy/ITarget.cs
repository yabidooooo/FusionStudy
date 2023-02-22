using UnityEngine;
using Fusion;

public interface ITarget
{
    public void OnHit( Vector3 position, Vector3 hitDirection, int hitboxIndex, PlayerRef player );
}
