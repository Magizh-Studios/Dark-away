using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUi : MonoBehaviour
{
    public bool CanVibrate { get; private set; }

    [SerializeField] private float openDelay = 0.3f , closeDelay = 0.3f;
    [SerializeField] private Ease openEase = Ease.Linear , closeEase = Ease.Linear;
    [SerializeField] private Button closeButton;
    [Space]
    [SerializeField] private AudioMixer masterAudioMixer;
    [Space]
    [SerializeField] private TMP_Dropdown grapicsDropdown;
    [SerializeField] private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider;
    [SerializeField] private Toggle vibrateToggle;

    private RectTransform rectTransform;

    private const string GRAPICS_QUALITY_INDEX_KEY = "GrapicsQuality";
    private const string MASTER_VOLUME_KEY = "Master";
    private const string MUSIC_VOLUME_KEY = "Music";
    private const string SFX_VOLUME_KEY = "Sfx";
    private const string VIBRATE_KEY = "Vibrate";

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        closeButton.onClick.AddListener(() =>
        {
            Hide(HideMode.Tween);

            MainMenuUi.Instance.SetActiveMainMenuBtns(true);
        });

        Hide(HideMode.Instant);
    }

    private void Start()
    {
        if (HasKeyInPlayerPrefs(GRAPICS_QUALITY_INDEX_KEY))
        {
            //Already Player Prefs Has Saved data
            int grapicsIndex = PlayerPrefs.GetInt(GRAPICS_QUALITY_INDEX_KEY);
            SetGrapicsQuality(grapicsIndex);
            grapicsDropdown.value = grapicsIndex;
        }

        if (HasKeyInPlayerPrefs(MASTER_VOLUME_KEY))
        {
            float masterSliderValue = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
            masterAudioMixer.SetFloat(MASTER_VOLUME_KEY,masterSliderValue);
            masterVolumeSlider.value = masterSliderValue;
        }

        if (HasKeyInPlayerPrefs(MUSIC_VOLUME_KEY))
        {
            float musicSliderValue = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
            masterAudioMixer.SetFloat(MUSIC_VOLUME_KEY, musicSliderValue);
            musicVolumeSlider.value = musicSliderValue;
        }

        if (HasKeyInPlayerPrefs(SFX_VOLUME_KEY))
        {
            float sfxSliderValue = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
            masterAudioMixer.SetFloat(SFX_VOLUME_KEY, sfxSliderValue);
            sfxVolumeSlider.value = sfxSliderValue;
        }

        if (HasKeyInPlayerPrefs(VIBRATE_KEY))
        {
            CanVibrate = PlayerPrefs.GetString(VIBRATE_KEY) == "true";
            vibrateToggle.isOn = CanVibrate;
        }
    }
    public void Show()
    {
        rectTransform.DOScale(Vector3.one,openDelay).SetEase(openEase);
    }

    public void Hide(HideMode hideMode)
    {
        if(hideMode == HideMode.Tween)
            rectTransform.DOScale(Vector3.zero, closeDelay).SetEase(closeEase);
        else
            rectTransform.localScale = Vector3.zero;
    }
    public enum HideMode { Tween,Instant }

    public void SetGrapicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        PlayerPrefs.SetInt(GRAPICS_QUALITY_INDEX_KEY, qualityIndex);
    }

    public void SetMasterVolume(float volume)
    {
        masterAudioMixer.SetFloat(MASTER_VOLUME_KEY, volume);

        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
    }
    public void SetMusicVolume(float volume)
    {
        masterAudioMixer.SetFloat(MUSIC_VOLUME_KEY, volume);
        // Saving Music Volume
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
    }
    public void SetSfxVolume(float volume)
    {
        masterAudioMixer.SetFloat (SFX_VOLUME_KEY, volume);
        // Saving Sfx Volume
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    public void SetCanVibrate(bool canVibrate)
    {
        this.CanVibrate = canVibrate;

        PlayerPrefs.SetString(VIBRATE_KEY,canVibrate == true ? "true" : "false");
    }

    private bool HasKeyInPlayerPrefs(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
}
