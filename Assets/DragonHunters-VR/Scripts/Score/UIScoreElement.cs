using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIScoreElement : MonoBehaviour
{
    [SerializeField] TMP_Text _playerText;
    [SerializeField] TMP_Text _scoreText;

    public void UpdatePlayerName( string name )
    {
        _playerText.SetText( name );
    }

    public void UpdateScore( int value )
    {
        _scoreText.SetText( value.ToString() );
    }
}
