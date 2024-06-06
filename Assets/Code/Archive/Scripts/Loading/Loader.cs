using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public class DummyClass : MonoBehaviour { }
    public enum Scene { MainMenu , Game }

    public static event EventHandler OnLoadingStarted;

    public static float LoadingProgress { get; private set; }

    public static void LoadScene(Scene scene)
    {
        GameObject loadingGameObj = new ("Loading GameObj");
        loadingGameObj.AddComponent<DummyClass>().StartCoroutine(LoadSceneAsync(scene));

        //LoadingUi.Instance?.Show();
        OnLoadingStarted?.Invoke(null, EventArgs.Empty);
    }

    private static IEnumerator LoadSceneAsync(Scene toLoadScene)
    {
        yield return null;
        AsyncOperation loadingAsyncOperation = SceneManager.LoadSceneAsync(toLoadScene.ToString());

        while (!loadingAsyncOperation.isDone)
        {
            LoadingProgress = Mathf.Clamp01(loadingAsyncOperation.progress / 0.9f);
            yield return null;
        }
    }

    public static void ResetStaticEvents()
    {
        OnLoadingStarted = null;
    }
}
