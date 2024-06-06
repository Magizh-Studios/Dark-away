using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;
using System;

public abstract class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemData
    {
        public enum ItemName {Battry, Fues, MatchBox, ScrewDriver, FlashLight, Wood};
        public ItemName itemName;
        public bool UseAbleItem;
        public int ItemCount;
    }
    [SerializeField] private RectTransform itemSlider_RT;
    [SerializeField] private RectTransform invBtn_RT;
    [SerializeField] private CanvasGroup Bag_CG;
    [SerializeField] private CanvasGroup Arrow_CG;
    private bool isInvOpen = false;
    private bool isAnimComplited = true;
    private bool isActivityPanelOpen = false;
    [SerializeField] private RectTransform activityPanel_RT;
    [SerializeField] private GameObject[] activityPanelText_GO;
    [SerializeField] private TMP_Text[] itemCount_Txt;
    [SerializeField] protected ItemData[] itemDatas;
    [SerializeField] protected TMP_InputField quantity_IF;
    private ItemData currentClickItemData;

    //Animation Function Start!
    public void OnPressInvBtn()
    {
        if(!isInvOpen && isAnimComplited)
        {
            OpenInventory();
        }
        else if(isAnimComplited)
        {
            if(isActivityPanelOpen)
            {
                isActivityPanelOpen = false;
                Invoke("DeActiveActivityPanelText", 0.3f);
                activityPanel_RT.DOAnchorPosY(400f, 1f, false).OnComplete(() => {
                    activityPanel_RT.gameObject.SetActive(false);
                    CloseInventory();
                });
            }
            else
            {
                CloseInventory();
            }
        }
    }

    private void OpenInventory()
    {
        RefreshItemCount();
        isAnimComplited = false;
        isInvOpen = true;
        invBtn_RT.DOLocalRotate(new Vector3(0f, 0f, 360f), 1f, RotateMode.FastBeyond360).OnComplete(() => {
            isAnimComplited = true;
        });
        itemSlider_RT.gameObject.SetActive(true);
        itemSlider_RT.DOAnchorPos(new Vector3(830f, 460f, 0f), 1f, false).OnComplete(() => {
            for (int i = 0; i < itemCount_Txt.Length; i++)
            {
                itemCount_Txt[i].gameObject.SetActive(true);
            }
        });
        Bag_CG.DOFade(0f, 0.5f).OnComplete(() => {
            Arrow_CG.DOFade(1f, 0.5f);
        });
    }

    private void CloseInventory()
    {
        isAnimComplited = false;
        isInvOpen = false;
        invBtn_RT.DOLocalRotate(new Vector3(0f, 0f, -360f), 1f, RotateMode.FastBeyond360).OnComplete(() => {
            isAnimComplited = true;
        });
        for (int i = 0; i < itemCount_Txt.Length; i++)
        {
            itemCount_Txt[i].gameObject.SetActive(false);
        }
        itemSlider_RT.DOAnchorPos(new Vector3(1630f, 460f, 0f), 1f, false).OnComplete(() => {
            itemSlider_RT.gameObject.SetActive(false);
        });
        Arrow_CG.DOFade(0f, 0.5f).OnComplete(() => {
            Bag_CG.DOFade(1f, 0.5f);
        });
        if(currentClickItemData != null) currentClickItemData = null;
    }

    public void OnPressItemsBtn(int itemIndex)
    {
        StartCoroutine(ActivityPanelAnimTimer((ItemData.ItemName)itemIndex));
    }

    IEnumerator ActivityPanelAnimTimer(ItemData.ItemName itemName)
    {
        if(isInvOpen && isAnimComplited)
        {
            ItemData _itemData = GetItemDatasByName(itemName);
            currentClickItemData = _itemData;
            activityPanel_RT.gameObject.SetActive(true);
            if(_itemData.UseAbleItem)
            {
                if(isActivityPanelOpen)
                {
                    isActivityPanelOpen = true;
                    activityPanel_RT.DOSizeDelta(new Vector2(300f, 80f), 1f);
                    yield return new WaitForSeconds(0.5f);
                    activityPanel_RT.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    isActivityPanelOpen = true;
                    activityPanel_RT.GetChild(1).gameObject.SetActive(true);
                    activityPanel_RT.sizeDelta = new Vector2(300f, 80f);
                    activityPanel_RT.DOAnchorPosY(320f, 1f, false);
                    yield return new WaitForSeconds(0.5f);
                    for (int i = 0; i < activityPanelText_GO.Length; i++)
                    {
                        activityPanelText_GO[i].SetActive(true);
                    }
                }
            }
            else
            {
                activityPanel_RT.gameObject.SetActive(true);
                if(isActivityPanelOpen)
                {
                    isActivityPanelOpen = true;
                    activityPanel_RT.DOSizeDelta(new Vector2(200f, 80f), 1f);
                    yield return new WaitForSeconds(0.5f);
                    activityPanel_RT.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    isActivityPanelOpen = true;
                    activityPanel_RT.GetChild(1).gameObject.SetActive(false);
                    activityPanel_RT.sizeDelta = new Vector2(200f, 80f);
                    activityPanel_RT.DOAnchorPosY(320f, 1f, false);
                    yield return new WaitForSeconds(0.5f);
                    for (int i = 0; i < activityPanelText_GO.Length; i++)
                    {
                        activityPanelText_GO[i].SetActive(true);
                    }
                }
            }
        }
    }

    private void DeActiveActivityPanelText()
    {
        for (int i = 0; i < activityPanelText_GO.Length; i++)
        {
            activityPanelText_GO[i].SetActive(false);
        }
    }
    //Animation Function End!

    protected void RefreshItemCount()
    {
        for (int i = 0; i < itemCount_Txt.Length; i++)
        {
            itemCount_Txt[i].text = itemDatas[i].ItemCount.ToString();
        }
    }

    public ItemData GetItemDatasByName(ItemData.ItemName itemName)
    {
        for (int i = 0; i < itemDatas.Length; i++)
        {
            if(itemDatas[i].itemName == itemName)
            {
                return itemDatas[i];
            }
        }
        throw new Exception("ItemData Not Found!");
    }

    public void OnPressUseBtn()
    {
        Debug.Log(currentClickItemData.itemName + " Has Clicked");
    }

    public void OnPressDropBtn()
    {
        Debug.Log(quantity_IF.text + " : " + currentClickItemData.itemName + " has Droped for Inventory!");
    }
}
