using UnityEngine;

public class TorchCollisionDetector : MonoBehaviour
{
    [SerializeField] Collider torchCollider;
    private bool canDetect = true;
   
    private void OnTriggerStay(Collider other)
    {
        if (!canDetect) return;

        if (other.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactable.SetPopUp(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!canDetect) return;

        if (other.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactable.SetPopUp(false);
        }
    }

    public void Detection(bool canDetect)
    {
        this.canDetect = canDetect;
        SetActiveTorchCollider(canDetect);
    }

    private void SetActiveTorchCollider(bool canDetect)
    {
        torchCollider.enabled = canDetect;
    }
}
