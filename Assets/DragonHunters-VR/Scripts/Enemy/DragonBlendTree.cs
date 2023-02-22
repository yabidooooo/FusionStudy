using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class DragonBlendTree : MonoBehaviour
{

    [SerializeField] AnimationClip _clipFlyingFast;
    [SerializeField] AnimationClip _clipFlyingSlow;

    PlayableGraph _graph;
    AnimationMixerPlayable _mixer;
    AnimationPlayableOutput _output;

    private void Awake()
    {
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode( DirectorUpdateMode.Manual );

        _mixer = AnimationMixerPlayable.Create( _graph, 2 );
        _mixer.SetPropagateSetTime( true );

        _mixer.ConnectInput( 0, AnimationClipPlayable.Create( _graph, _clipFlyingFast ), 0, 0f );
        _mixer.ConnectInput( 1, AnimationClipPlayable.Create( _graph, _clipFlyingSlow ), 0, 1f );

        _output = AnimationPlayableOutput.Create( _graph, "DragonAnimation", GetComponent<Animator>() );
        _output.SetSourcePlayable( _mixer );
    }

    private void OnDestroy()
    {
        _mixer.Destroy();
        _graph.Destroy();
    }

    public void SetAnimationTimeSpeed( double time, float flySpeedNormalized )
    {
        _mixer.SetInputWeight( 0, flySpeedNormalized );
        _mixer.SetInputWeight( 1, 1f - flySpeedNormalized );
        _mixer.SetTime( time );

        _graph.Evaluate();
    }
}
