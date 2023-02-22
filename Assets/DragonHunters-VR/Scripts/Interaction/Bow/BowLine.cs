using UnityEngine;

[RequireComponent( typeof( LineRenderer ) )]
public class BowLine : MonoBehaviour
{
    [SerializeField] Transform[] _attachmentPoints;

    LineRenderer _line;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = _attachmentPoints.Length;
    }

    private void LateUpdate()
    {
        for( int i = 0; i< _attachmentPoints.Length; ++i )
        {
            _line.SetPosition( i, _attachmentPoints[ i ].position );
        }
    }
}
