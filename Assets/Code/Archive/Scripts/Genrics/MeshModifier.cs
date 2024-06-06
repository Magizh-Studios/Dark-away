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