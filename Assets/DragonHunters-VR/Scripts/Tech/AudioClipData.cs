using UnityEngine;

using UnityEngine.Serialization;

[System.Serializable]
public struct AudioClipData
{
    [SerializeField] AudioClip[] _clips;
    [SerializeField] Vector2 _pitchVariance;
    [SerializeField] Vector2 _volumeVariance;

    public void Play( AudioSource source )
    {
        source.clip = _clips[ Random.Range( 0, _clips.Length ) ];
        source.volume = Random.Range( _volumeVariance.x, _volumeVariance.y );
        source.pitch = Random.Range( _pitchVariance.x, _pitchVariance.y );

        source.Play();
    }
}
