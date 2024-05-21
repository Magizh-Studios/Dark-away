using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectableItem", menuName = "Create/Collectable Item", order = 1)]
public class CollectableItemSO : ScriptableObject
{
    public string itemName;
    public string itemIcon;
    public GameObject itemPrefab;
    public float capacity;

}
