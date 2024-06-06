using System;
using UnityEngine;

public abstract class BaseLightSource : MonoBehaviour
{
    [SerializeField] protected float affectRadius = 5f;        // Safe Zone Radius
    [SerializeField] protected Light lightSource;
    [SerializeField] protected float fuelDecreaseSpeed = 2f;

    public readonly float FUEL_CAPACITY_MAX = 100f;
    protected float currentFuelAmount = 0f;

    public float GetCurrenFuelAmount()
    {
        return currentFuelAmount;
    }

    protected float lightMaxIntencity;
    protected float colliderMaxRadius;
    protected bool isWorking = false;

    protected bool isPlayerInsideSafeZone = false;

    protected SphereCollider sphereCollider;
    protected virtual void Awake()
    {
        lightMaxIntencity = lightSource.intensity;
        currentFuelAmount = FUEL_CAPACITY_MAX;

        CreateSafeZone();
    }

    protected virtual void CreateSafeZone()
    {
        // Creating Collider To Make Trigger
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = affectRadius;
        sphereCollider.isTrigger = true;

        // Initializing Collider Radius
        colliderMaxRadius = sphereCollider.radius;
    }

    protected virtual void Update()
    {
        HandleFuelDrain();
        UpdateColliderRadius();
        UpdateLightVisual();
    }

    protected virtual void UpdateColliderRadius()
    {
        float radiusPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;
        float newRadius = radiusPercentage * colliderMaxRadius;
        newRadius = Mathf.Max(newRadius, 0f);
        sphereCollider.radius = newRadius;
    }

    protected virtual void HandleFuelDrain()
    {
        isWorking = currentFuelAmount > 0;

        if (isWorking)
        {
            DecreaseFuel(fuelDecreaseSpeed * Time.deltaTime);
        }

        currentFuelAmount = Mathf.Clamp(currentFuelAmount, 0, FUEL_CAPACITY_MAX);
    }

    protected virtual void UpdateLightVisual()
    {
        // Calculate the intensity percentage based on the current fuel amount
        float intensityPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;

        // Calculate the new intensity value based on the percentage
        float newIntensity = intensityPercentage * lightMaxIntencity;

        // Ensure that the intensity doesn't go below zero
        newIntensity = Mathf.Max(newIntensity, 0f);

        // Set the light intensity
        lightSource.intensity = newIntensity;
    }


    public void AddFuel(float fuelAmount)
    {
        currentFuelAmount = Mathf.Clamp(currentFuelAmount + fuelAmount, 0, FUEL_CAPACITY_MAX);
    }

    protected void DecreaseFuel(float fuelAmount)
    {
        currentFuelAmount = Mathf.Clamp(currentFuelAmount - fuelAmount, 0, FUEL_CAPACITY_MAX);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            // Player Inside Safe Area
            player.SetSafeZoneMode(true);
            isPlayerInsideSafeZone = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            // Player Out Of Safe Area
            player.SetSafeZoneMode(false);
            isPlayerInsideSafeZone = false;
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = !isPlayerInsideSafeZone ? Color.green : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, affectRadius);
    }
}
