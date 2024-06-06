using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    public event Action OnInteractTriggered;

    public event Action OnCollectTriggered;

   
    public event EventHandler OnInteractionKeyPerformed;
    //public event EventHandler OnThrowKeyPerformed;
    public event EventHandler OnDropKeyPerformed;

    private PlayerInputAction inputActions;

    [SerializeField] private Button dropButton;

    private void Awake()
    {
        Instance = this;

        dropButton.onClick.AddListener(() => {
            OnDropKeyPerformed?.Invoke(this, EventArgs.Empty);
        });


        dropButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        inputActions = new PlayerInputAction();

        inputActions.Enable();

        inputActions.Player.Interaction.performed += Interaction_performed;
        //inputActions.Player.Throw.performed += Throw_performed;

        EquipHandler.Instance.OnPlayerHoldingObject += EquipHandler_OnPlayerHoldingObject;
    }

    private void EquipHandler_OnPlayerHoldingObject(IHoldable holdable)
    {
        dropButton.gameObject.SetActive(holdable is Lamp);
    }

    //private void Throw_performed(InputAction.CallbackContext obj)
    //{
    //    OnThrowKeyPerformed?.Invoke(this, EventArgs.Empty);
    //}

    private void Interaction_performed(InputAction.CallbackContext obj)
    {
        OnInteractionKeyPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public void Collect()
    {
        OnCollectTriggered?.Invoke();
    }

    public void Interact()
    {
        OnInteractTriggered?.Invoke();
    }

}
