using UnityEngine;

public class ScoreNumber : MonoBehaviour
{
    TMPro.TMP_Text _text;

    private void Awake()
    {
        _text = GetComponentInChildren<TMPro.TMP_Text>();
    }

    public void Setup( int score )
    {
        _text.SetText( score.ToString() );

    }
}
