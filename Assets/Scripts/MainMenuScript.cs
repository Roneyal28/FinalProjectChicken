using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "Game";

    [Header("Buttons")]
    public Button playButton;
    public Button quitButton;

    [Header("Button Sounds")]
    public AudioSource audioSource;
    public AudioClip playButtonClip;
    public AudioClip quitButtonClip;
    public AudioClip playHoverClip;
    public AudioClip quitHoverClip;

    [Header("Timing")]
    public bool waitForSoundBeforeAction = true;
    public float fallbackDelaySeconds = 0.15f;

    private bool isTransitioning;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        FindButtonsIfNeeded();
        HookHoverSounds();
    }

    public void PlayGame()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(PlaySoundThenRun(playButtonClip, LoadGameScene));
    }

    public void QuitGame()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(PlaySoundThenRun(quitButtonClip, QuitApplication));
    }

    public void PlayButtonHoverSound()
    {
        PlaySound(playHoverClip);
    }

    public void QuitButtonHoverSound()
    {
        PlaySound(quitHoverClip);
    }

    private void FindButtonsIfNeeded()
    {
        if (playButton == null)
        {
            playButton = FindButtonByName("Play");
        }

        if (quitButton == null)
        {
            quitButton = FindButtonByName("Quit");
        }
    }

    private Button FindButtonByName(string buttonName)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        return buttonObject == null ? null : buttonObject.GetComponent<Button>();
    }

    private void HookHoverSounds()
    {
        HookHoverSound(playButton, PlayButtonHoverSound);
        HookHoverSound(quitButton, QuitButtonHoverSound);
    }

    private void HookHoverSound(Button button, UnityEngine.Events.UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener(_ => callback.Invoke());
        trigger.triggers.Add(entry);
    }

    private IEnumerator PlaySoundThenRun(AudioClip clip, System.Action action)
    {
        isTransitioning = true;
        float delay = PlaySound(clip);

        if (waitForSoundBeforeAction)
        {
            yield return new WaitForSeconds(delay);
        }

        action?.Invoke();
    }

    private float PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return Mathf.Max(0f, fallbackDelaySeconds);
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        return Mathf.Max(clip.length, fallbackDelaySeconds);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}