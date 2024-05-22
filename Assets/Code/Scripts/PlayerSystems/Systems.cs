using PlayerSystems.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class System<T> : SingletonBehavior<T> where T : MonoBehaviour
{

    public bool Status {
        get => _status;
        set {
            _status = value;
            OnStatusChange(_status);
        }
    }

    public virtual void OnStatusChange(bool currentStatus) { }

    public TItem GetClosestItem<TItem>(List<TItem> items) where TItem : IGetPosition
    {
        TItem closestItem = default(TItem);
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (var item in items)
        {
            float distance = Vector3.Distance(currentPosition, item.GetPosition());
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }

    private void OnDestroy()
    {
        Status = false;
    }

    private void OnDisable()
    {
        Status = false;
    }

    private bool _status = true;
    protected InputManager inputManager;


    public void SetInputManager(InputManager manager)
    {
        this.inputManager = manager;
    }

}
