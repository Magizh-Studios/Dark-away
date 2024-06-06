using UnityEngine;

[DisallowMultipleComponent]
public class Interactor : MonoBehaviour
{
    public static Interactor Instance { get; private set; }

    [SerializeField] private float interactRadius = 5f;

    private SphereCollider sphereCollider;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
        if (other.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactable.SetPopUp(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IInteractable interactable))
        {
            interactable.SetPopUp(false);
        }
    }

    private void OnDrawGizmos()
    {
        if(sphereCollider != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        }
    }

}