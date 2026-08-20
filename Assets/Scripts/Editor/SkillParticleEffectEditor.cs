using StopDefence.Vfx;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillParticleEffect))]
public sealed class SkillParticleEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        SkillParticleEffect effect = (SkillParticleEffect)target;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Play Preview"))
            {
                effect.Play();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Stop Preview"))
            {
                effect.Stop();
                SceneView.RepaintAll();
            }
        }
    }
}
