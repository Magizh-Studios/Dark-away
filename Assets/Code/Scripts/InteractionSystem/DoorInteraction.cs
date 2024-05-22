using DG.Tweening;
using UnityEngine;

public class DoorInteraction : BaseInteractable
{
    [SerializeField] private float openSpeed = 1.75f;
    [SerializeField] private DoorDirection directionType = DoorDirection.In;
    [SerializeField] private Ease easeMode = Ease.InBack;
    [SerializeField] private bool isOpen;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
      
    }

    public override void Interact()
    {
        EnvironmentChecker.Instance.OnInteractablesChanged += (interactables) =>
        {
            for (int i = 0; i < interactables.Count; i++)
            {
                if(interactables[i] is DoorInteraction)
                {
                    if(!isOpen)
                    ToggleDoor();
                }
            }
        };

        base.Interact();
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        Vector3 toRotation;

        Vector3 target = GetComponentInChildren<MeshRenderer>().bounds.center;

        Vector3 directionToTarget = (target - EnvironmentChecker.Instance.transform.position).normalized;

        float dotProduct = Vector3.Dot(target, directionToTarget);

        directionType = dotProduct > 0.3f ? DoorDirection.In : DoorDirection.Out;

        if (directionType == DoorDirection.In)
            toRotation = Vector3.up * (isOpen ? 90f : 0);
        else
            toRotation = Vector3.up * (isOpen ? 270 : 0);

        transform.DOLocalRotate(toRotation, openSpeed, RotateMode.Fast).SetEase(easeMode);

    }

    public enum DoorDirection
    {
        In,
        Out
    }
}
