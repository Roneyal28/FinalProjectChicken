using System;
using UnityEngine;

public class ChickenSoundsScripts : MonoBehaviour
{
    SoundFXManager SFXManager;

    private bool playFirstFootstep = true;

    private void Awake()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        SFXManager = FindFirstObjectByType<SoundFXManager>();
    }

    public void PlayFootstep()
    {
        if (SFXManager == null)
        {
            SFXManager = FindFirstObjectByType<SoundFXManager>();
        }

        if (SFXManager == null)
        {
            return;
        }

        AudioClip clip = playFirstFootstep ? SFXManager.walkOnWood1 : SFXManager.walkOnWood2;

        SFXManager.PlaySFX(clip);
        playFirstFootstep = !playFirstFootstep;
    }

    public void PlayFootSound1()
    {
        PlayFootstep();
    }

    public void PlayFootSound2()
    {
        PlayFootstep();
    }
}
