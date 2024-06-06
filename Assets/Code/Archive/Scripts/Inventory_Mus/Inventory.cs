using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public event EventHandler<OnInventoryModifiedArgs> OnInventoryModified;
    public class OnInventoryModifiedArgs: EventArgs {  public List<CollectableItemSO> gatherableObjectSoList; }


    [SerializeField] private int inventoryItemLimit = 8;

    private List<CollectableItemSO> inventoryItems;

    [SerializeField] private List<ItemFuelData> itemFuelDataList;
    private void Awake()
    {
        Instance = this;
        inventoryItems = new List<CollectableItemSO>(inventoryItemLimit);
    }

    private void OnEnable()
    {
        CollectableItem.OnPlayerTryPickUpGatherableObject += GatherableObject_OnPlayerTryPickUpGatherableObject;
        InventorySingleUi.OnItemIconClicked += OnItemClicked; 
    }

    private bool OnTryPickUpCollectable(CollectableItemSO pickedUpItemSO)
    {
        if (inventoryItems.Count < inventoryItemLimit)
        {
            //AddToInventory(pickedUpItemSO);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnItemClicked(CollectableItemSO gatherableObjectSO)
    {
        // Temporary Use
        UseItem(gatherableObjectSO);
    }

    private bool GatherableObject_OnPlayerTryPickUpGatherableObject(CollectableItemSO gatherableObjectSO)
    {
        CollectableItemSO pickuppedItemSO = gatherableObjectSO;

        if (inventoryItems.Count < inventoryItemLimit)
        {
            AddToInventory(pickuppedItemSO);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void AddToInventory(CollectableItemSO pickuppedItemSO)
    {
        inventoryItems.Add(pickuppedItemSO);
        Debug.Log($"Inventory Added Item : {pickuppedItemSO.itemName}");

        OnInventoryModified?.Invoke(this, new OnInventoryModifiedArgs { gatherableObjectSoList = GetInventoryItems() });
    }

    public void UseItem(CollectableItemSO gatherableObjectSO)
    {
        if (gatherableObjectSO != null)
        {
            switch (gatherableObjectSO.usageType)
            {
                case GatherableObjectType.Usable:
                    // Kind A like Battery
                    Debug.Log($"Item {gatherableObjectSO.itemName} Is Used");

                    AddFuelToLightSource(gatherableObjectSO);

                    RemoveFromInventory(gatherableObjectSO);
                    break;
            }
        }
    }

    private void AddFuelToLightSource(CollectableItemSO gatherableObjectSO)
    {
        for (int i = 0; i < itemFuelDataList.Count; i++)
        {
            if (itemFuelDataList[i].FuelSO == gatherableObjectSO)
            {
                itemFuelDataList[i].LightSource.AddFuel(gatherableObjectSO.capacity);
            }
        }
    }

    private void RemoveFromInventory(CollectableItemSO gatherableObjectSO)
    {
        inventoryItems.Remove(gatherableObjectSO);

        Debug.Log($"Inventory Used Item : {gatherableObjectSO.itemName}");
        OnInventoryModified?.Invoke(this, new OnInventoryModifiedArgs { gatherableObjectSoList = GetInventoryItems() });
    }

    public List<CollectableItemSO> GetInventoryItems() => inventoryItems;

    public bool TryGetGatherableObject(CollectableItemSO gatherableSOInput, out CollectableItemSO newGatherableSO)
    {
        if (inventoryItems.Any(s => s == gatherableSOInput)) // founded That Object 
        {
            var gatherableObjSO = inventoryItems.Where(s => s == gatherableSOInput).FirstOrDefault(); // Get That Founded
            newGatherableSO = gatherableObjSO;  // Pass Through Parameter
            RemoveFromInventory(gatherableObjSO);
            return true;
        }
        else
        {
            newGatherableSO = null;
            return false;
        }
    }

    [Serializable]
    public class ItemFuelData
    {
        public BaseLightSource LightSource;
        public CollectableItemSO FuelSO;
    }
}
