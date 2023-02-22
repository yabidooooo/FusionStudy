using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Fusion;

[CustomEditor( typeof( EnemyHealth ) )]
public class EnemyHealthInspector : Editor
{
    readonly string _hitboxPropertyName = "_hitboxDamage";

    bool _Foldout;
    public override void OnInspectorGUI()
    {
        var enemyHealth = target as EnemyHealth;
        DrawPropertiesExcluding( serializedObject, _hitboxPropertyName );
        serializedObject.ApplyModifiedProperties();

        var root = enemyHealth.GetComponent<HitboxRoot>();
        if( root == null )
        {
            EditorGUILayout.HelpBox( "No HitboxRoot found", MessageType.Error );
            return;
        }

        var prop = serializedObject.FindProperty( _hitboxPropertyName );
        if( root.Hitboxes.Length != prop.arraySize )
        {
            EditorGUILayout.HelpBox( "Hitbox array size missmatch", MessageType.Error );
            if( GUILayout.Button( "Fix Array Size" ) )
            {
                var delta = root.Hitboxes.Length - prop.arraySize;
                if( delta > 0 )
                {
                    for( int i = 0; i < delta; ++i )
                    {
                        int index = prop.arraySize;
                        Debug.Log( "Insert array element at " + index );
                        prop.InsertArrayElementAtIndex( index );
                    }

                }
                else if( delta < 0 )
                {
                    for( int i = 0; i < -delta; ++i )
                    {
                        int index = prop.arraySize - 1;
                        Debug.Log( "Remove array element at " + index );
                        prop.DeleteArrayElementAtIndex( index );
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
            return;
        }

        _Foldout = EditorGUILayout.BeginFoldoutHeaderGroup( _Foldout, "Hitbox Damages" );
        if( _Foldout )
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            for( int i = 0; i < prop.arraySize; ++i )
            {
                EditorGUILayout.PropertyField( prop.GetArrayElementAtIndex( i ), new GUIContent() { text = root.Hitboxes[ i ].name } );
            }
            if ( EditorGUI.EndChangeCheck() )
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
