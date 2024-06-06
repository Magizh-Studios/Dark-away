using UnityEngine;

public class InventorySystem : InventoryManager
{
    //Function List!
    /*GiveItem(ItemName, Amount)
      TakeItem(ItemName, Amount)
      SetItemAmount(ItemName, Amout)
      DropItem(ItemName, Amount)
      GetItemAmount(ItemName)
      GetItemDatasByName(ItemName)*/

    public void GiveItem(ItemData.ItemName itemName, int amount)
    {   
        if(amount > 0)
        {
            GetItemDatasByName(itemName).ItemCount += amount;
            RefreshItemCount();
        }
        else
        {
            Debug.LogWarning("Amount Must Above 0!");
        }
    }

    public void TakeItem(ItemData.ItemName itemName , int amount)
    {
        if(GetItemDatasByName(itemName).ItemCount <= 0)
        {
            if(amount > 0)
            {
                GetItemDatasByName(itemName).ItemCount -= amount;
                RefreshItemCount();
            }
            else
            {
                Debug.LogWarning("Amount Must Above 0!");
            }
        }
        else
        {
            Debug.LogWarning("You cant able to Take This " + " ("+ itemName.ToString() + ") " + "Because It is Already 0 Qty in Player Hand!");
        }
    }

    public void SetItemAmount(ItemData.ItemName itemName, int amount)
    {
        if(amount >= 0)
        {
            GetItemDatasByName(itemName).ItemCount = amount;
            RefreshItemCount();
        }
        else
        {
            Debug.LogWarning("Amount Must Start With 0!");
        }
    }

    public void DropItem(ItemData.ItemName itemName, int amount)
    {
        if(amount > 0)
        {
            //Drop ItemData Logic Here!
        }
        else
        {
            Debug.LogWarning("Amount Must Above 0!");
        }
    }

    public int GetItemAmount(ItemData.ItemName itemName)
    {
        return GetItemDatasByName(itemName).ItemCount;
    }
}
