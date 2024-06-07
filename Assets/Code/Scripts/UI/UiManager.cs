using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : SingletonBehavior<UiManager>
{
    public event EventHandler<OnlightTogglePerformedArgs> OnlightTogglePerformed;
    public class OnlightTogglePerformedArgs {  public bool OnlightTogglePerformed; }

    [SerializeField] private Button interactBtn;
    [SerializeField] private Button collectBtn;
    [SerializeField] private Button lightSourceToggleButton;

    [SerializeField] private Slider fuelSlider;

    private bool lightToggle = true;

   protected override void Awake() {

        base.Awake();

        lightSourceToggleButton.onClick.AddListener(() => {

            lightToggle = !lightToggle;

            OnlightTogglePerformed?.Invoke(this, new OnlightTogglePerformedArgs {
                OnlightTogglePerformed = lightToggle
            });

            UpdateToggleText();
        });

        UpdateToggleText();
    }

    private void UpdateToggleText() {
        TextMeshProUGUI textMeshProUGUI = lightSourceToggleButton.GetComponentInChildren<TextMeshProUGUI>();
        if(textMeshProUGUI != null ) {
            textMeshProUGUI.text = lightToggle ? "OFF" : "ON";
        }
    }

    private void Start()
    {
        EnvironmentChecker.Instance.OnInteractablesChanged += (x) => SetInteractState(x.Count > 0);
        EnvironmentChecker.Instance.OnCollectablesChanged += (x) => SetCollectState(x.Count > 0 && x.FirstOrDefault().canCollect);

        EquipHandler.Instance.OnPlayerHoldingObject += EquipHandler_Instance_OnPlayerHoldingObject;
    }

    private void EquipHandler_Instance_OnPlayerHoldingObject(IHoldable holdable) {
        if(holdable != null) {
            if(holdable is BaseLightSource) {
                lightSourceToggleButton.gameObject.SetActive(true);
            }
        }
        else {
            lightSourceToggleButton.gameObject.SetActive(false);
        }
    }

    public void SetInteractState(bool state)
    {
        interactBtn.gameObject.SetActive(state);
    }

    public void SetCollectState(bool state)
    {
        collectBtn.gameObject.SetActive(state);
    }



    private void Update()
    {
        if (interactBtn.enabled || collectBtn.enabled)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (EquipHandler.Instance.curLightSource != null)
        {
            BaseLightSource source = EquipHandler.Instance.curLightSource;
            fuelSlider.value = source.GetCurrenFuelAmount();
        }

    }

}
