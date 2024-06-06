using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySingleUi : MonoBehaviour
{
    public static Action<CollectableItemSO> OnItemIconClicked;

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantity;

    [SerializeField] private Button interactButton;

    private CollectableItemSO gatherableObjectSO;

    private void Awake()
    {
        interactButton.onClick.AddListener(() =>
        {
            OnItemIconClicked?.Invoke(gatherableObjectSO);
        });
    }
    public void SetGatherableObjectSO(CollectableItemSO gatherableObjectSO)
    {
        this.gatherableObjectSO = gatherableObjectSO;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        iconImage.sprite = gatherableObjectSO.icon;
        itemNameText.text = gatherableObjectSO.itemName;
        itemQuantity.text = GetQuantityFromItem(gatherableObjectSO).ToString();
    }

    private int GetQuantityFromItem(CollectableItemSO gatherableObjectSO)
    {
        // Filter inventory items based on the gatherable object type
        var inventoryItems = Inventory.Instance.GetInventoryItems()
            .Where(item => item == gatherableObjectSO);

        // Count the number of filtered inventory items
        int quantity = inventoryItems.Count();

        return quantity;
    }
}
