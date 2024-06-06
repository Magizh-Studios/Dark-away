using UnityEditor;

[CustomEditor(typeof(MeshModifier))]
public class MeshModifierEditor : Editor
{
    SerializedProperty meshRendererProp;
    SerializedProperty modedMaterialProp;
    SerializedProperty modifyModeProp;

    private void OnEnable()
    {
        meshRendererProp = serializedObject.FindProperty("meshRenderer");
        modedMaterialProp = serializedObject.FindProperty("modedMaterial");
        modifyModeProp = serializedObject.FindProperty("modifyMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector fields
        EditorGUILayout.PropertyField(meshRendererProp);
        EditorGUILayout.PropertyField(modifyModeProp);

        // Show modedMaterial field only when Transparenting mode is selected
        ModifyMode mode = (ModifyMode)modifyModeProp.enumValueIndex;
        if (mode == ModifyMode.Transparenting)
        {
            EditorGUILayout.PropertyField(modedMaterialProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
