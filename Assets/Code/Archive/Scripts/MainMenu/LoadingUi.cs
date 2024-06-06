using UnityEngine;
using UnityEngine.UI;

public class LoadingUi : MonoBehaviour
{
    public static LoadingUi Instance { get; private set; }

    [SerializeField] private Transform loadingUiTransform;
    [SerializeField] private Slider loadingBarSlider;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        loadingBarSlider.value = 0f;
        Loader.OnLoadingStarted += Loader_OnLoadingStarted;

        Hide();
    }

    private void Loader_OnLoadingStarted(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Update()
    {
        if (loadingUiTransform.gameObject.activeInHierarchy)
            loadingBarSlider.value = Loader.LoadingProgress;
    }

    public void Show()
    {
        loadingBarSlider.value = 0f;
        loadingUiTransform.gameObject.SetActive(true);
    }

    public void Hide()
    {
        loadingBarSlider.value = 0f;
        loadingUiTransform.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Loader.ResetStaticEvents();
    }
}
