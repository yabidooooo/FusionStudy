using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField] float _delay;

    IEnumerator Start()
    {
        yield return new WaitForSeconds( _delay );
        Destroy( gameObject );
    }
}
