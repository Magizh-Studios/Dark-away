using PlayerSystems.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveItem : MonoBehaviour, IInteractables
{
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void Interact()
    {

#if UNITY_EDITOR
        Debug.Log($"Performing Interaction:{gameObject.name}");
#endif

    }
}
