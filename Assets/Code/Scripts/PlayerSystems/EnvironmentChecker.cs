using PlayerSystems.Collectables;
using PlayerSystems.Interactables;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentChecker : SingletonBehavior<EnvironmentChecker>
{
    [SerializeField] private float interactRadius;
    [SerializeField] private LayerMask interactableLayerMask;

    private List<IInteractables> currentInteractables;
    private List<ICollectables> currentCollectables;

    public event Action<List<ICollectables>> OnCollectablesChanged;
    public event Action<List<IInteractables>> OnInteractablesChanged;


    private SphereCollider sphereCollider;

    private void Start()
    {
        currentInteractables = new List<IInteractables>();
        currentCollectables = new List<ICollectables>();

        CreateOverLapCollider();
    }

    private void CreateOverLapCollider()
    {
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = interactRadius;
        sphereCollider.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.TryGetComponent(out IInteractables interactable))
        {
            if (!currentInteractables.Contains(interactable))
            {
                currentInteractables.Add(interactable);

                HandleInteractableChanges();
            }
        }

        if (other.gameObject.TryGetComponent(out ICollectables collectables))
        {
            if (!currentCollectables.Contains(collectables))
            {
                currentCollectables.Add(collectables);

                HandleCollectableChanges();
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IInteractables interactable))
        {
            currentInteractables.Remove(interactable);

            HandleInteractableChanges();
        }

        if (other.gameObject.TryGetComponent(out ICollectables collectables))
        {
            currentCollectables.Remove(collectables);

            HandleCollectableChanges();
        }

    }

    public void HandleCollectableChanges()
    {
        OnCollectablesChanged?.Invoke(currentCollectables);
    }


    public void HandleInteractableChanges()
    {
        OnInteractablesChanged?.Invoke(currentInteractables);
    }

    private void OnDrawGizmos()
    {
        if (sphereCollider != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        }
    }

    public void RemoveCollectableFromList(ICollectables item)
    {
        currentCollectables.Remove(item);
        HandleCollectableChanges();
    }

    public float GetInteractRadius() => interactRadius;
    public int GetInteractLayers() => interactableLayerMask;
}
