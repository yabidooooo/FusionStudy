using UnityEngine;
using UnityEngine.Serialization;
using Fusion;

[RequireComponent( typeof( AudioSource ) )]
public class NetworkedAudioSource : SimulationBehaviour
{
    AudioSource _source;
    [SerializeField] AudioClipData[] _clips;
    [SerializeField] bool _playAnyOnAwake = false;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();

        if( _playAnyOnAwake )
        {
            DoPlayOneShot();
        }
    }

    public void PlayOneShot()
    {
        if( Runner != null && Runner.IsForward )
        {
            DoPlayOneShot();
        }
    }

    void DoPlayOneShot()
    {
        _clips[ Random.Range( 0, _clips.Length ) ].Play( _source );
    }


}
