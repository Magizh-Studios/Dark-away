using PlayerSystems.Interactables;
using UnityEngine;
using UnityEngine.AI;

public class Lamp : BaseLightSource, IInteractables, IHoldable
{
    [SerializeField] private InteractionIndigationUi indigationUi;
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    public Transform GetHoldableObject()
    {
        return transform;
    }

    protected override void Update()
    {
        base.Update();

        UpdateLampNavMesh();

    }

    private void UpdateLampNavMesh()
    {
        float intensityPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;
        float newIntensity = intensityPercentage * 3;
        newIntensity = Mathf.Max(newIntensity, 0f);
        navMeshObstacle.radius = newIntensity;
    }

    public void Interact()
    {
        if (FindObjectOfType<Player>().gameObject.TryGetComponent(out EquipHandler equipHandler))
        {
            equipHandler.SetHoldingObject(this);
            Debug.Log("Lamp Picked");
            
        }

    }


    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
