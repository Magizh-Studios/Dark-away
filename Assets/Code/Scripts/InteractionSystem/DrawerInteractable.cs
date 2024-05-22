using DG.Tweening;
using System;
using UnityEngine;

public class DrawerInteractable : BaseInteractable
{
    [SerializeField] private float openSpeed = 1.75f;
    [SerializeField] private float openAmount = 0.5f;
    [SerializeField] private CabinetAxis openAxis = CabinetAxis.X;
    [SerializeField] private Ease easeMode = Ease.Linear;
    [SerializeField] private bool isOpen;

    [SerializeField] private GameObject[] curItems;

    private float dotThreshold = 0;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public override void Interact()
    {
        base.Interact();
        ToggleDrawer();
    }

    private void ToggleDrawer()
    {
        isOpen = !isOpen;

        var amount = isOpen ? openAmount : 0;

        Vector3 target = transform.forward;

        Vector3 directionToTarget = (target - EnvironmentChecker.Instance.transform.position).normalized;

        float dotProduct = Vector3.Dot(target, directionToTarget);

        if (dotProduct > dotThreshold)
        {
            switch (openAxis)
            {
                case CabinetAxis.X:
                    transform.DOLocalMoveX(amount, openSpeed).SetEase(easeMode);
                    break;
                case CabinetAxis.Y:
                    transform.DOLocalMoveY(amount, openSpeed).SetEase(easeMode);
                    break;
                case CabinetAxis.Z:
                    transform.DOLocalMoveZ(amount, openSpeed).SetEase(easeMode);
                    break;
                case CabinetAxis.N_X:
                    transform.DOLocalMoveX(-amount, openSpeed).SetEase(easeMode);
                    break;
                case CabinetAxis.N_Y:
                    transform.DOLocalMoveY(-amount, openSpeed).SetEase(easeMode);
                    break;
                case CabinetAxis.N_Z:
                    transform.DOLocalMoveZ(-amount, openSpeed).SetEase(easeMode);
                    break;
            }
            OnToggle();
        }
    }

    private void OnToggle()
    {
        if (curItems.Length > 0)
            Invoke("SetObjectState", openSpeed / 2f);
    }

    void SetObjectState()
    {
        foreach (var item in curItems)
        {
            item?.SetActive(isOpen);
        }
    }


    public enum CabinetAxis
    {
        X, Y, Z,
        N_X, N_Y, N_Z
    }

}
