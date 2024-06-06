using PlayerSystems.Collectables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private Button interactBtn;
    [SerializeField] private Button collectBtn;
    [SerializeField] private Slider fuelSlider;

    private void Start()
    {
        EnvironmentChecker.Instance.OnInteractablesChanged += (x) => SetInteractState(x.Count > 0);
        EnvironmentChecker.Instance.OnCollectablesChanged += (x) => SetCollectState(x.Count > 0 && x.FirstOrDefault().canCollect);
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
