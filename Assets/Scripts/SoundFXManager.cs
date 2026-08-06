using System;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [Header("Barrel Sounds")]
    public AudioClip barrelHit;
    public AudioClip barrelBreak;

    [Header("Popup Sound")]
    public AudioClip popupSound;

    [Header("Shotgun Animation Sounds")]
    public AudioClip shotgunDraw;
    public AudioClip shotgunReload;
    public AudioClip shotgunShoot;
    
    
    


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

    [Header("Barrel Volumes")]
    [Range(0f, 1f)] public float barrelHitVolume = 1f;
    [Range(0f, 1f)] public float barrelBreakVolume = 1f;

    [Header("Popup Volume")]
    [Range(0f, 1f)] public float popupSoundVolume = 1f;

    [Header("Popup Pitch Range")]
    [Range(0.1f, 3f)] public float popupSoundMinPitch = 0.9f;
    [Range(0.1f, 3f)] public float popupSoundMaxPitch = 1.1f;

    [Header("Shotgun Volumes")]
    [Range(0f, 1f)] public float shotgunDrawVolume = 1f;
    [Range(0f, 1f)] public float shotgunReloadVolume = 1f;
    [Range(0f, 1f)] public float shotgunShootVolume = 1f;

    [Header("Shotgun Pitch Ranges")]
    [Range(0.1f, 3f)] public float shotgunReloadMinPitch = 0.9f;
    [Range(0.1f, 3f)] public float shotgunReloadMaxPitch = 1.1f;
    [Range(0.1f, 3f)] public float shotgunShootMinPitch = 0.9f;
    [Range(0.1f, 3f)] public float shotgunShootMaxPitch = 1.1f;

    [Header("Chicken Damage Pitch Ranges")]
    [Range(0.1f, 3f)] public float takeDamageMinPitch = 0.9f;
    [Range(0.1f, 3f)] public float takeDamageMaxPitch = 1.1f;
    [Range(0.1f, 3f)] public float takeDamage2MinPitch = 0.9f;
    [Range(0.1f, 3f)] public float takeDamage2MaxPitch = 1.1f;
    [Range(0.1f, 3f)] public float takeDamage3MinPitch = 0.9f;
    [Range(0.1f, 3f)] public float takeDamage3MaxPitch = 1.1f;




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

        SFXSource.pitch = 1f;
        SFXSource.PlayOneShot(clip, GetSFXVolume(clip));
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        SFXSource.pitch = 1f;
        SFXSource.PlayOneShot(clip, volume);
    }

    public void PlayRandomChickenDamageSound()
    {
        AudioClip[] clips = { takeDamage, takeDamage2, takeDamage3 };
        float[] volumes = { takeDamageVolume, takeDamageVolume2, takeDamageVolume3 };
        float[] minimumPitches = { takeDamageMinPitch, takeDamage2MinPitch, takeDamage3MinPitch };
        float[] maximumPitches = { takeDamageMaxPitch, takeDamage2MaxPitch, takeDamage3MaxPitch };

        int availableCount = 0;
        int[] availableIndices = new int[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null)
            {
                availableIndices[availableCount++] = i;
            }
        }

        if (availableCount == 0 || SFXSource == null)
        {
            return;
        }

        int selectedIndex = availableIndices[Random.Range(0, availableCount)];
        float minPitch = Mathf.Min(minimumPitches[selectedIndex], maximumPitches[selectedIndex]);
        float maxPitch = Mathf.Max(minimumPitches[selectedIndex], maximumPitches[selectedIndex]);
        SFXSource.pitch = Random.Range(minPitch, maxPitch);
        SFXSource.PlayOneShot(clips[selectedIndex], volumes[selectedIndex]);
    }

    public void PlayShotgunDraw()
    {
        PlaySFX(shotgunDraw, shotgunDrawVolume);
    }

    public void PlayShotgunReload()
    {
        PlaySFXWithRandomPitch(
            shotgunReload,
            shotgunReloadVolume,
            shotgunReloadMinPitch,
            shotgunReloadMaxPitch);
    }

    public void PlayShotgunShoot()
    {
        AudioClip clip = shotgunShoot != null ? shotgunShoot : gunShooting;
        float volume = shotgunShoot != null ? shotgunShootVolume : gunShootingVolume;
        PlaySFXWithRandomPitch(clip, volume, shotgunShootMinPitch, shotgunShootMaxPitch);
    }

    public void PlayBarrelHit()
    {
        PlaySFX(barrelHit, barrelHitVolume);
    }

    public void PlayBarrelBreak()
    {
        PlaySFX(barrelBreak, barrelBreakVolume);
    }

    public void PlayPopupSound()
    {
        PlaySFXWithRandomPitch(
            popupSound,
            popupSoundVolume,
            popupSoundMinPitch,
            popupSoundMaxPitch);
    }

    private void PlaySFXWithRandomPitch(AudioClip clip, float volume, float minimumPitch, float maximumPitch)
    {
        if (clip == null || SFXSource == null)
            return;

        float minPitch = Mathf.Min(minimumPitch, maximumPitch);
        float maxPitch = Mathf.Max(minimumPitch, maximumPitch);
        SFXSource.pitch = Random.Range(minPitch, maxPitch);
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
        if (clip == barrelHit) return barrelHitVolume;
        if (clip == barrelBreak) return barrelBreakVolume;
        if (clip == popupSound) return popupSoundVolume;
       
        return 1f;
    }
}
