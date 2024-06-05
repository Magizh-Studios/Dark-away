using DG.Tweening;
using PlayerSystems.Collectables;
using System;
using UnityEngine;

public class DrawerInteractable : BaseInteractable
{
    [SerializeField] private float openSpeed = 1.75f;
    [SerializeField] private float openAmount = 0.5f;
    [SerializeField] private CabinetAxis openAxis = CabinetAxis.X;
    [SerializeField] private Ease easeMode = Ease.Linear;

    [SerializeField] private Transform[] doors;

    private Transform curDrawer;

    private int currentIndex = -1;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public override void Interact()
    {
        base.Interact();
        Toggle();

    }

    private void Toggle()
    {
        if (currentIndex < 0)
        {
            currentIndex = 0;
            OpenDoor(currentIndex);
        }
        else if (currentIndex >= 0)
        {
            if (currentIndex == doors.Length - 1)
            {
                CloseAll();
                return;
            }
            CloseDoor(currentIndex);
            OpenDoor(++currentIndex);
        }
    }

    public void CloseAll()
    {
        foreach (var door in doors)
        {
            CloseDoor(door);
        }
        currentIndex = -1;
    }
    private void CloseDoor(int index)
    {
        MoveDrawer(doors[index], false);
    }

    private void CloseDoor(Transform door)
    {
        int index = System.Array.IndexOf(doors, door);
        if (index >= 0)
        {
            CloseDoor(index);
        }
    }

    private void OpenDoor(int index)
    {
        MoveDrawer(doors[index], true);
    }

    public void MoveDrawer(Transform drawer, bool toOpen)
    {
        curDrawer = drawer;
        float amount;
        if (!toOpen)
        {
            amount = 0;
        }
        else
            amount = openAmount;

        switch (openAxis)
        {
            case CabinetAxis.X:
                drawer.DOLocalMoveX(amount, openSpeed).SetEase(easeMode).OnComplete(() => {
                    drawer.localPosition = new Vector3(amount, drawer.localPosition.y, drawer.localPosition.z);
                });
                break;
            case CabinetAxis.Y:
                drawer.DOLocalMoveY(amount, openSpeed).SetEase(easeMode).OnComplete(() => {
                    drawer.localPosition = new Vector3(drawer.localPosition.x, amount, drawer.localPosition.z);
                });
                break;
            case CabinetAxis.Z:
                drawer.DOLocalMoveZ(amount, openSpeed).SetEase(easeMode).OnComplete(() => {
                    drawer.localPosition = new Vector3(drawer.localPosition.x, drawer.localPosition.y, amount);
                });
                break;
            case CabinetAxis.N_X:
                drawer.DOLocalMoveX(-amount, openSpeed).SetEase(easeMode).OnComplete(() => {
                    drawer.localPosition = new Vector3(-amount, drawer.localPosition.y, drawer.localPosition.z);
                });
                break;
            case CabinetAxis.N_Y:
                drawer.DOLocalMoveY(-amount, openSpeed).SetEase(easeMode).OnComplete(() => {
                    drawer.localPosition = new Vector3(drawer.localPosition.x, -amount, drawer.localPosition.z);
                });
                break;
            case CabinetAxis.N_Z:
                drawer.DOLocalMoveZ(-amount, openSpeed).SetEase(easeMode).OnComplete(() => {
                    drawer.localPosition = new Vector3(drawer.localPosition.x, drawer.localPosition.y, -amount);
                });
                break;
        }
        OnToggle(toOpen);
    }

    private void OnToggle(bool toOpen)
    {
        if (curDrawer.childCount > 0)
            SetContentsState(toOpen);
    }

    void SetContentsState(bool toOpen)
    {
        for (int i = 0; i < curDrawer.childCount; i++)
        {
            curDrawer.GetChild(i).gameObject.SetActive(toOpen);

            ICollectables collectable = curDrawer.GetChild(i).gameObject.GetComponent<ICollectables>();
            if (!toOpen)
            {
                EnvironmentChecker.Instance.RemoveCollectableFromList(collectable);
            }
            else
            {
                EnvironmentChecker.Instance.AddCollectableToList(collectable);
            }
        }
    }


    public enum CabinetAxis
    {
        X, Y, Z,
        N_X, N_Y, N_Z
    }

}
