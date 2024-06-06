using UnityEngine;
using UnityEngine.AI;

public class Fire : BaseLightSource
{
    [SerializeField] private ParticleSystem fireParticleSystem;
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    protected override void Update()
    {
        base.Update();

        UpdateFireVisual();
        UpdateFireNavMesh();
    }

    private void UpdateFireNavMesh()
    {
        float intensityPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;
        float newIntensity = intensityPercentage * 3;
        newIntensity = Mathf.Max(newIntensity, 0f);
        navMeshObstacle.radius = newIntensity;
    }

    private void UpdateFireVisual()
    {
        float intensityPercentage = currentFuelAmount / FUEL_CAPACITY_MAX;
        float newIntensity = intensityPercentage * 3;
        newIntensity = Mathf.Max(newIntensity, 0f);
        fireParticleSystem.startLifetime = newIntensity;
    }
}
