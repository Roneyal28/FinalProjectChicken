using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("BGM and SFX")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    
    [Header("Sounds")]
    public AudioClip bgm;
    public AudioClip timer;
    public AudioClip jump;
    public AudioClip walk;
    public AudioClip eatFood;
    public AudioClip wingAttack;
    public AudioClip obtainItem;
    public AudioClip text;
    public AudioClip cancel;
    public AudioClip confirm;
    public AudioClip gunshooting;
    public AudioClip gunshooting2;
    public AudioClip gunshooting3;
    
}
