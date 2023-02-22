using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class BowBlendTree : MonoBehaviour
{
    [SerializeField] AnimationClip _clipPulled;
    [SerializeField] AnimationClip _clipIdle;
    [SerializeField] AnimationClip _clipRelease;

    PlayableGraph _graph;
    AnimationMixerPlayable _pullMixer;
    AnimationMixerPlayable _releaseMixer;
    AnimationPlayableOutput _output;
    AnimationClipPlayable _releasePlayable;

    private void Awake()
    {
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode( DirectorUpdateMode.Manual );

        _pullMixer = AnimationMixerPlayable.Create( _graph, 2 );
        _pullMixer.SetPropagateSetTime( true );

        _pullMixer.ConnectInput( 0, AnimationClipPlayable.Create( _graph, _clipPulled ), 0, 0f );
        _pullMixer.ConnectInput( 1, AnimationClipPlayable.Create( _graph, _clipIdle ), 0, 1f );

        _releaseMixer = AnimationMixerPlayable.Create( _graph, 2 );
        _releaseMixer.ConnectInput( 0, _pullMixer, 0, 1f );

        _releasePlayable = AnimationClipPlayable.Create( _graph, _clipRelease );
        _releaseMixer.ConnectInput( 1, _releasePlayable, 0, 0f );

        _output = AnimationPlayableOutput.Create( _graph, "BowAnimation", GetComponent<Animator>() );
        _output.SetSourcePlayable( _releaseMixer );
    }

    private void OnDestroy()
    {
        _pullMixer.Destroy();
        _releaseMixer.Destroy();
        _graph.Destroy();
    }

    public void SetAnimationTimeSpeed( double time, double releaseTime, float pullNormalized )
    {
        _pullMixer.SetInputWeight( 0, pullNormalized );
        _pullMixer.SetInputWeight( 1, 1f - pullNormalized );

        _pullMixer.SetTime( time );

        _releasePlayable.SetTime( releaseTime );
        float releaseWeight = Mathf.Clamp01( (float)releaseTime );
        _releaseMixer.SetInputWeight( 0, releaseWeight );
        _releaseMixer.SetInputWeight( 1, 1f - releaseWeight );

        _graph.Evaluate();
    }
}