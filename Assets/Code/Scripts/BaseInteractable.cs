using PlayerSystems.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInteractable : MonoBehaviour, IInteractables
{
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public virtual void Interact()
    {
        Debug.Log("Interacted with :" + gameObject.name);
    }
}
