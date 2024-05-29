using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] private Button interactBtn;
    [SerializeField] private Button collectBtn;


    private void Start()
    {
        EnvironmentChecker.Instance.OnInteractablesChanged += (x) => SetInteractState(x.Count > 0);
        EnvironmentChecker.Instance.OnCollectablesChanged += (x) => SetCollectState(x.Count > 0);
    }

    public void SetInteractState(bool state)
    {
        interactBtn.gameObject.SetActive(state);
    }

    public void SetCollectState(bool state)
    {
        collectBtn.gameObject.SetActive(state);
    }


}
