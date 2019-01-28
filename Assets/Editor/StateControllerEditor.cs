using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StateController))]
public class StateControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var cont = target as StateController;

        if (cont.HasTarget)
        {
            EditorGUILayout.LabelField("Distance To Target: " + cont.DistanceToTarget);
        }

        GUILayout.Space(10);

        if (Btn("Target Random Object"))
        {
            var targets = FindObjectsOfType<MeshRenderer>();
            cont.Target = targets[UnityEngine.Random.Range(0, targets.Length)].gameObject;
        }

        if (Btn("Target Random Ship"))
        {
            var targets = FindObjectsOfType<Ship>();
            cont.Target = targets[UnityEngine.Random.Range(0, targets.Length)].gameObject;
        }

        if (Btn("Target Player Ship"))
        {
            cont.Target = GameSettings.pc.ship.gameObject;
        }

        if (Btn("Clear Target"))
        {
            cont.Target = null;
        }

        if (Btn("Clear Current State"))
        {
            cont.pastStates.Clear();
            cont.currentState = null;
        }

        if (Application.isPlaying && Btn("Toggle AI"))
        {
            cont.aiIsActive = !cont.aiIsActive;
        }

        if (Application.isPlaying && Btn("Full Stop"))
        {
            cont.pastStates.Clear();
            cont.currentState = cont.stopState;
        }
    }

    private void PlayingButtons()
    {
        if (!Application.isPlaying) return;
    }

    private void NonPlayingButtons()
    {
        if (Application.isPlaying) return;
    }

    private bool Btn(String str)
    {
        return GUILayout.Button(str);
    }
}