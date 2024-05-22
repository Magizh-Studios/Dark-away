using UnityEngine;

public class InspectableBoard : BaseInteractable
{
    public override void Interact()
    {
        Debug.Log("Interacted with :" + gameObject.name);
    }
}
