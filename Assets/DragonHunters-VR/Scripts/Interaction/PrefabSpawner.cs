using Fusion;
public class PrefabSpawner : NetworkBehaviour
{
    public void Spawn( NetworkObject prefab )
    {
        if( Object.HasStateAuthority )
        {
            Runner.Spawn( prefab, transform.position, transform.rotation );
        }
    }
}
