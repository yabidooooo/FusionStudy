using UnityEngine;

public class AnimationSoundTrigger : MonoBehaviour
{
    [SerializeField] NetworkedAudioSource[] _audioSource;

    public void PlayAudio( int index )
    {
        _audioSource[ index ].PlayOneShot();
    }
}
