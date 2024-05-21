using PlayerSystems.Collectables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableItem : MonoBehaviour, ICollectables
{

    public CollectableItemSO itemData;

    public void Collect()
    {
#if UNITY_EDITOR
        Debug.Log($"Collected Object :{itemData.itemName}, Capacity Added:{itemData.capacity}");
#endif
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }


}
