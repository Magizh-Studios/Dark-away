using DG.Tweening;
using PlayerSystems.Interactables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractables
{
    [SerializeField] private float openSpeed;
    [SerializeField] private DoorDirection direction;
    private bool isOpen;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void Interact()
    {
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        Vector3 toRotation;
        if (direction == DoorDirection.In)
            toRotation = Vector3.up * (isOpen ? 90f : 0);
        else
            toRotation = Vector3.up * (isOpen ? 270 : 0);

        transform.DOLocalRotate(toRotation, openSpeed, RotateMode.Fast);
    }
}

public enum DoorDirection
{
    In,
    Out
}