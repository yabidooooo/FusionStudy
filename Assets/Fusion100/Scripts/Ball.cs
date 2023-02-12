using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public override void FixedUpdateNetwork()
    {
        transform.position += 5 * transform.forward * Runner.DeltaTime;
    }
}
