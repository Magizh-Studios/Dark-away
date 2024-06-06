using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUi : MonoBehaviour
{
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private Transform itemTemplateSingleUi;

    [SerializeField] private Button inventoryToggleButton;

    private List<CollectableItemSO> instantiatedItemsUi = new List<CollectableItemSO>();
    private void Awake()
    {
        inventoryToggleButton.onClick.AddListener(() =>
        {
            ToggleInventory();
        });
    }

    private void ToggleInventory()
    {
        itemsContainer.gameObject.SetActive(!itemsContainer.gameObject.activeSelf);
    }

    private void Start()
    {
        Inventory.Instance.OnInventoryModified += Inventoty_OnInventoryModified;

        itemsContainer.gameObject.SetActive(false);
        itemTemplateSingleUi.gameObject.SetActive(false);
    }

    private void Inventoty_OnInventoryModified(object sender, Inventory.OnInventoryModifiedArgs e)
    {
        UpdateInventoryUi(e.gatherableObjectSoList);
    }

    private void UpdateInventoryUi(List<CollectableItemSO> gatherableObjectSoList)
    {
        CleanUpTemplates();

        SpawnTemplates(gatherableObjectSoList);
    }

    private void SpawnTemplates(List<CollectableItemSO> gatherableObjectSoList)
    {
        instantiatedItemsUi.Clear();

        foreach (CollectableItemSO gatherableObjectSO in gatherableObjectSoList)
        {
            if(!instantiatedItemsUi.Contains(gatherableObjectSO))
            {
                InventorySingleUi inventorySingleUi = Instantiate(itemTemplateSingleUi, itemsContainer).GetComponent<InventorySingleUi>();
                inventorySingleUi.gameObject.SetActive(true);
                inventorySingleUi.SetGatherableObjectSO(gatherableObjectSO);

                instantiatedItemsUi.Add(gatherableObjectSO);
            }
            
        }
    }

    //private void CleanUpTemplates()
    //{
    //    foreach(Transform child in  itemsContainer)
    //    {
    //        if (child == itemTemplateSingleUi) continue;

    //        Destroy(child.gameObject);
    //    }
        
    //}

    private void CleanUpTemplates()
    {
        foreach (Transform child in itemsContainer)
        {
            if (child != itemTemplateSingleUi)
            {
                Destroy(child.gameObject);
            }
        }
    }

}
