using PlayerSystems.Collectables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CollectableItem : MonoBehaviour, ICollectables
{

    public CollectableItemSO itemData;

    public bool canCollect { get; set; }

    private void OnEnable()
    {
        canCollect = true;
        GetComponent<Collider>().enabled = true;
    }

    private void OnDisable()
    {
        canCollect = false;
        GetComponent<Collider>().enabled = false;
    }

    public void Collect()
    {
#if UNITY_EDITOR
        Debug.Log($"Collected Object :{itemData.itemName}, Capacity Added:{itemData.capacity}");
#endif
        Destroy(gameObject);
        EnvironmentChecker.Instance.RemoveCollectableFromList(this);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }


}
