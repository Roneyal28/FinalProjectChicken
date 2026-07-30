using System;
using UnityEngine;

public class ChickenSoundsScripts : MonoBehaviour
{
    SoundFXManager SFXManager;

    private bool playFirstFootstep = true;

    private void Awake()
    {
        SFXManager = FindObjectOfType<SoundFXManager>().GetComponent<SoundFXManager>();

    }

    public void PlayFootstep()
    {
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
