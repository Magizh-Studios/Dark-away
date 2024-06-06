using System.Collections.Generic;
using UnityEngine;

public class HidePointManager : MonoBehaviour
{
    public static HidePointManager Instance { get; private set; }

    [SerializeField] private List<HidePoint> hidePointList;
    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        Instance = this;
    }

    public HidePoint GetNearHidePointOutPlayerFov()
    {
        HidePoint targetHidePoint;
        do
        {
            HidePoint hidePoint = hidePointList[UnityEngine.Random.Range(0,hidePointList.Count)];
            targetHidePoint = hidePoint;

        } while (IsInPlayerFov(targetHidePoint));

        return targetHidePoint;
    }

    private bool IsInPlayerFov(HidePoint hidePoint)
    {
        Vector3 dir = (hidePoint.transform.position - playerTransform.position).normalized;

        if(Physics.Raycast(transform.position, dir ,out RaycastHit hitInfo, 
            Vector3.Distance(hidePoint.transform.position,playerTransform.position)))
        {
            if(hitInfo.collider.gameObject.TryGetComponent(out Player player))
            {
                return true;
            }
        }
        return false;
    }
}
