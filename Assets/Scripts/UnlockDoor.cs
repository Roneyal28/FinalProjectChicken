using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnlockDoor : MonoBehaviour
{
    [SerializeField] GameObject closedDoor;
    [SerializeField] GameObject openDoor;

    [Header("Door Sounds")]
    [SerializeField] private AudioClip lockedDoorSound;
    [Range(0f, 1f)] [SerializeField] private float lockedDoorVolume = 1f;
    [Range(0.1f, 3f)] [SerializeField] private float lockedDoorMinPitch = 0.9f;
    [Range(0.1f, 3f)] [SerializeField] private float lockedDoorMaxPitch = 1.1f;
    [SerializeField] private AudioClip openedDoorSound;
    [Range(0f, 1f)] [SerializeField] private float openedDoorVolume = 1f;

    private ItemsManagement items;
    private SoundFXManager soundFXManager;
    private bool playerInRange;
    private bool isOpen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        openDoor.SetActive(false);
        items = GameObject.FindGameObjectWithTag("Player").GetComponent<ItemsManagement>();
        soundFXManager = FindFirstObjectByType<SoundFXManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && playerInRange && !isOpen)
        {
            if (items != null && items.HasKey)
            {
                isOpen = true;
                closedDoor.SetActive(false);
                openDoor.SetActive(true);
                items.HasKey = false;

                if (soundFXManager != null)
                {
                    soundFXManager.PlaySFX(openedDoorSound, openedDoorVolume);
                }
            }
            else if (soundFXManager != null)
            {
                soundFXManager.PlaySFXWithRandomPitch(
                    lockedDoorSound,
                    lockedDoorVolume,
                    lockedDoorMinPitch,
                    lockedDoorMaxPitch);
            }
        }
    }

    
   private void OnTriggerStay2D (Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
