using UnityEngine;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour
{
    public static MainMenuUi Instance { get; private set; }

    [SerializeField] private Button playButton, settingsButton, creditsButton, quitButton;
    [SerializeField] private CreditsUi creditsUi;
    [SerializeField] private SettingsUi settingsUi;

    private void Awake()
    {
        Instance = this;

        playButton.onClick.AddListener(() =>
        {
            Loader.LoadScene(Loader.Scene.Game);
        });

        settingsButton.onClick.AddListener(() =>
        {
            settingsUi.Show();
            SetActiveMainMenuBtns(false);
        });

        creditsButton.onClick.AddListener(() =>
        {
            creditsUi.Show();
            creditsUi.RollNames();

            SetActiveMainMenuBtns(false);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
            Debug.Log("Application Exited");
        });
    }

    public void SetActiveMainMenuBtns(bool active)
    {
        playButton.interactable = active;
        settingsButton.interactable = active;
        creditsButton.interactable = active;
        quitButton.interactable = active;
    }
}
