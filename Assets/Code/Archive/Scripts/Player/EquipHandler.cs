using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EquipHandler : MonoBehaviour
{
    public static EquipHandler Instance { get; private set; }

    public event Action<IHoldable> OnPlayerHoldingObject;
    [SerializeField] private Transform objectHoldPosition;
    private Transform holdingObjectTransform = null;

    public BaseLightSource curLightSource { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        curLightSource = Torch.Instance;

        //InputManager.Instance.OnThrowKeyPerformed += InputManager_OnThrowKeyPerformed; // Can Be Refactorable, Future In ObjectHolder.
        InputManager.Instance.OnDropKeyPerformed += InputManager_OnDropKeyPerformed;
        UiManager.Instance.OnlightTogglePerformed += UiManager_Instance_OnlightTogglePerformed;
    }

    private void UiManager_Instance_OnlightTogglePerformed(object sender, UiManager.OnlightTogglePerformedArgs e) {
        if(e.OnlightTogglePerformed) {
            //light Source Should On
            if(curLightSource != null) {
                curLightSource.SetWorkingState(true);
            }
        }
        else {
            //light Source Should Off
            curLightSource.SetWorkingState(false);
        }
    }

    private void InputManager_OnDropKeyPerformed(object sender, EventArgs e)
    {
        HandleThrow();
    }

    private void InputManager_OnThrowKeyPerformed(object sender, System.EventArgs e)
    {
        HandleThrow();
    }

    private void HandleThrow()
    {
        if (objectHoldPosition.childCount > 0)
        {
            SetHoldingObjectGravity(true);
            SetHoldingObject(null);
        }
    }

    public void SetHoldingObject(IHoldable holdable)
    {
        if (holdable != null)
        {
            holdingObjectTransform = holdable.GetHoldableObject();
            holdingObjectTransform.SetParent(objectHoldPosition);
            holdingObjectTransform.transform.localPosition = Vector3.zero;

            curLightSource = holdable as BaseLightSource;

            PlayerIk.Instance.SetHoldingObject(holdable);

            OnPlayerHoldingObject?.Invoke(holdable);

            SetHoldingObjectGravity(false);
        }
        else
        {
            holdingObjectTransform.SetParent(null);

            PlayerIk.Instance.SetHoldingObject(null);

            curLightSource.SetWorkingState(false);

            curLightSource = null;

            OnPlayerHoldingObject?.Invoke(null);
        }

    }

    private void SetHoldingObjectGravity(bool gravity)
    {
        var rb = holdingObjectTransform.gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = !gravity;
    }

    private void Update()
    {
        //Debug.Log(isPlayerInSafeZone == true ? "Player In Safe Zone": " Im Out Of Safe Zone");
    }

    public Transform GetLampHoldingPosition()
    {
        return objectHoldPosition;
    }
}
