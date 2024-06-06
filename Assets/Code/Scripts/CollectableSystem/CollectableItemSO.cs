using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GatherableObjectSO", menuName = "ScriptableObject/GatherableObjectSO")]
public class CollectableItemSO : ScriptableObject
{
    public GameObject prefab;     // To Instantiate .
    public Sprite icon;          // To Show In Ui Elements.
    public string itemName;           // To Show Name.
    public List<string> itemDescription;          // To Show item Description
    public GatherableObjectType usageType;   // Type that you Can Save Some Time.
    public StoringType storageType;               // Store type That Detemines whether it Removable Or Not
    public float capacity;                           // like Health,And Battery Power Only For Usable
    public GameObject itemSetUppedPrefab;         // for Respawn item
}
public enum GatherableObjectType  // This Enum Catagarising Objects.
{
    Usable
}

public enum StoringType // this enum Determines is Storable Or Not
{
    Removable,
    NonRemovable
}