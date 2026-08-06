using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayableDirector))]
public class CutsceneEndLoader : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private PlayableDirector director;

    private void Awake()
    {
        Time.timeScale = 1f;
        director = GetComponent<PlayableDirector>();
        director.stopped += ReturnToMainMenu;
    }

    private void OnDestroy()
    {
        if (director != null)
            director.stopped -= ReturnToMainMenu;
    }

    private void ReturnToMainMenu(PlayableDirector stoppedDirector)
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
