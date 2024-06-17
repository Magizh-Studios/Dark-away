using UnityEditor;
using UnityEngine;

public class MeshModifier : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material modedMaterial;
    [SerializeField] private ModifyMode modifyMode;

    private Material originalMaterial;

    private void Awake()
    {
        originalMaterial = meshRenderer.material;
    }

    public void DoModify()
    {
        if(modifyMode == ModifyMode.JustEnableDisable)
        {
            meshRenderer.enabled = false;
            Debug.Log("Materials Modfied");
        }
        else
        {
            meshRenderer.material = modedMaterial;
            Debug.Log("Materials Modfied");
        }
        
    }


    public void UnDoModify()
    {
        if (modifyMode == ModifyMode.JustEnableDisable)
        {
            meshRenderer.enabled = true;
            Debug.Log("Materials ReStored");
        }
        else
        {
            meshRenderer.material = originalMaterial;
            Debug.Log("Materials ReStored");
        }
      
    }
}
public enum ModifyMode
{
    JustEnableDisable,
    Transparenting
}


[CustomEditor(typeof(MeshModifier))]
public class MeshModifierEditor : Editor {
    SerializedProperty modedMaterial;
    SerializedProperty modifyMode;

    private void OnEnable() {
        // Link to the properties in the target script
        modedMaterial = serializedObject.FindProperty("modedMaterial");
        modifyMode = serializedObject.FindProperty("modifyMode");
    }

    public override void OnInspectorGUI() {
        // Update the serialized object's representation in the inspector
        serializedObject.Update();

        // Draw the default inspector UI
        DrawDefaultInspector();

        // Only show the modedMaterial field when modifyMode is Transparenting
        if ((ModifyMode)modifyMode.enumValueIndex == ModifyMode.Transparenting) {
            EditorGUILayout.PropertyField(modedMaterial);
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
