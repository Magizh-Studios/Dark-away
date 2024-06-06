using PlayerSystems.Collectables;
using System;
using UnityEngine;

public class CollectableItem : MonoBehaviour, ICollectables
{
    public static event Predicate<CollectableItemSO> OnPlayerTryPickUpGatherableObject;

    [SerializeField] private CollectableItemSO collectableItemSo;
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
        var isPicked = OnPlayerTryPickUpGatherableObject?.Invoke(collectableItemSo);

        if (isPicked.Value == true)
        {
            Destroy(gameObject);
            EnvironmentChecker.Instance.RemoveCollectableFromList(this);
        }
        else
        {
            Debug.LogWarning($"Inventory Cant Accept Item : {collectableItemSo.itemName}");
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
