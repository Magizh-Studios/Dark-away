using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObject", menuName = "ScriptableObject/Items")]
public class Item : ScriptableObject {
    public InventoryManager.ItemData.ItemName itemName;
}