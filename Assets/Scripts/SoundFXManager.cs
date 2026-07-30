using System;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("BGM and SFX")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource timerSource;
    
    [Header("Sounds")]
    public AudioClip bgm;
    public AudioClip timer;
    public AudioClip jump;
    public AudioClip jump2;
    public AudioClip walkOnWood1;
    public AudioClip walkOnWood2;
    public AudioClip eatFood;
    public AudioClip wingAttack;
    public AudioClip wingAttack2;
    public AudioClip obtainItem;
    public AudioClip text;
    public AudioClip cancel;
    public AudioClip confirm;
    public AudioClip gunShooting;
    public AudioClip gunShooting2;
    public AudioClip gunShooting3;
    public AudioClip takeDamage;
    public AudioClip takeDamage2;
    public AudioClip takeDamage3;
    public AudioClip ratHit;
    public AudioClip ratDeath;
    
    
    


    [Header("SFX Volumes")]
    [Range(0f, 1f)] public float timerVolume = 1f;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    [Range(0f, 1f)] public float jump2Volume = 1f;
    [Range(0f, 1f)] public float walkOnWood1Volume = 0.3f;
    [Range(0f, 1f)] public float walkOnWood2Volume = 0.3f;
    [Range(0f, 1f)] public float eatFoodVolume = 1f;
    [Range(0f, 1f)] public float wingAttackVolume = 1f;
    [Range(0f, 1f)] public float wingAttack2Volume = 1f;
    [Range(0f, 1f)] public float obtainItemVolume = 1f;
    [Range(0f, 1f)] public float textVolume = 1f;
    [Range(0f, 1f)] public float cancelVolume = 1f;
    [Range(0f, 1f)] public float confirmVolume = 1f;
    [Range(0f, 1f)] public float gunShootingVolume = 1f;
    [Range(0f, 1f)] public float gunShooting2Volume = 1f;
    [Range(0f, 1f)] public float gunShooting3Volume = 1f;
    [Range(0f, 1f)] public float takeDamageVolume = 1f;
    [Range(0f, 1f)] public float takeDamageVolume2 = 1f;
    [Range(0f, 1f)] public float takeDamageVolume3 = 1f;
    [Range(0f, 1f)] public float ratHitVolume = 1f;
    [Range(0f, 1f)] public float ratDeathVolume = 1f;




    private void Start()
    {
        musicSource.clip = bgm;
        musicSource.Play();
        PlayTimer();
    }

    private void Update()
    {
        if (timerSource != null)
        {
            timerSource.volume = timerVolume;
        }
    }

    private void PlayTimer()
    {
        if (timerSource == null)
        {
            timerSource = gameObject.AddComponent<AudioSource>();
        }

        timerSource.clip = timer;
        timerSource.volume = timerVolume;
        timerSource.loop = true;
        timerSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        SFXSource.PlayOneShot(clip, GetSFXVolume(clip));
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        SFXSource.PlayOneShot(clip, volume);
    }

    private float GetSFXVolume(AudioClip clip)
    {
        if (clip == timer) return timerVolume;
        if (clip == jump) return jumpVolume;
        if (clip == jump2) return jump2Volume;
        if (clip == walkOnWood1) return walkOnWood1Volume;
        if (clip == walkOnWood2) return walkOnWood2Volume;
        if (clip == eatFood) return eatFoodVolume;
        if (clip == wingAttack) return wingAttackVolume;
        if (clip == wingAttack2) return wingAttack2Volume;
        if (clip == obtainItem) return obtainItemVolume;
        if (clip == text) return textVolume;
        if (clip == cancel) return cancelVolume;
        if (clip == confirm) return confirmVolume;
        if (clip == gunShooting) return gunShootingVolume;
        if (clip == gunShooting2) return gunShooting2Volume;
        if (clip == gunShooting3) return gunShooting3Volume;
        if (clip == takeDamage) return takeDamageVolume;
        if (clip == takeDamage2) return takeDamageVolume2;
        if (clip == takeDamage3) return takeDamageVolume3;
        if (clip == ratHit) return ratHitVolume;
        if (clip == ratDeath) return ratDeathVolume;
       
        return 1f;
    }
}
