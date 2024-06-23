
using Micosmo.SensorToolkit;
using System.Collections.Generic;
using UnityEngine;

public class Torch : BaseLightSource,IHoldable
{
    public static Torch Instance { get; private set; }

    private FOVCollider fOVCollider;

    public FOVCollider FOVCollider => fOVCollider;

    public bool CanHoldTorch = true;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {

        PlayerIk.Instance.SetHoldingObject(CanHoldTorch ? this : null);

        if(!CanHoldTorch) {
            Hide();
        }

        EquipHandler.Instance.OnPlayerHoldingObject += EquipHandler_OnPlayerHoldingObject;
    }

    private void EquipHandler_OnPlayerHoldingObject(IHoldable holdable)
    {
        if(holdable != this as IHoldable)
        {
            Hide();
        }

        if (holdable == null) {
            Show();
            PlayerIk.Instance.SetHoldingObject(this);
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    protected override void CreateSafeZone()
    {
        // Get Fov Collider
        fOVCollider = GetComponent<FOVCollider>();

        // Creating Collider To Make Trigger
        fOVCollider.Length = affectRadius;

        // Initializing Collider Radius
        colliderMaxRadius = fOVCollider.Length;
    }

    protected override void UpdateColliderRadius()
    {
        //float radiusPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;
        //float newRadius = radiusPercentage * colliderMaxRadius;
        //newRadius = Mathf.Max(newRadius, 0f);
        //fOVCollider.Length = newRadius;
    }
    protected override void OnDrawGizmos()
    {
        
    }

    public Transform GetHoldableObject()
    {
        return transform;
    }

    protected override void OnTriggerStay(Collider other) {
        if (other.gameObject.TryGetComponent(out ILightAffectable lightAffectable)) {
            float distance = Vector3.Distance(other.gameObject.transform.position , transform.position);

            if(distance < GetLightDistance()) {
                lightAffectable.IsAffectedByLight = true;
            }
        }
    }

    private float GetLightDistance() {
        float radiusPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;
        float newRadius = radiusPercentage * colliderMaxRadius;
        return Mathf.Max(newRadius, 0f);
    }
    protected override void OnTriggerExit(Collider other) {
        if (other.gameObject.TryGetComponent(out ILightAffectable lightAffectable)) {
            lightAffectable.IsAffectedByLight = false;
        }
    }
}
