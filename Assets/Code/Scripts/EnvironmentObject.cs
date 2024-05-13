using System;
using UnityEngine;

/// <summary>
/// Default Implementation Triggers OnPlayerEnter/OnPlayerExit Events
/// </summary>
public abstract class EnvironmentObject : MonoBehaviour
{

    public Action OnPlayerEnter;
    public Action OnPlayerExit;

    private bool isStaying;

    public void OnTriggerEnter(Collider other)
    {
        if (isStaying)
            return;

        if (other.gameObject.TryGetComponent(out EnvironmentChecker checker))
        {
            isStaying = true;
            OnPlayerEnter?.Invoke();
            Destroy(gameObject);
        }


    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out EnvironmentChecker _))
        {
            isStaying = false;
            OnPlayerExit?.Invoke();
        }
    }

}

