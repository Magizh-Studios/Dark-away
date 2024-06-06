using UnityEngine;
public interface IInteractable
{
    void Interact(Transform interactorTransform);

    void SetPopUp(bool canPopUp);
}