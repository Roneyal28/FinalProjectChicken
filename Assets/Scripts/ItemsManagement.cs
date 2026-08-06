using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemsManagement : MonoBehaviour
{
    [Header("Shotgun Positioning")]
    [Tooltip("Extra local offset added while facing right. Animation movement is preserved.")]
    [SerializeField] private Vector2 shotgunRightOffset = Vector2.zero;
    [Tooltip("Extra local offset added while facing left. Animation movement is preserved.")]
    [SerializeField] private Vector2 shotgunLeftOffset = Vector2.zero;

    [Header("Shotgun Particle Emitter")]
    [SerializeField] private Vector2 particleRightOffset = new Vector2(0.243f, 0.047f);
    [SerializeField] private Vector2 particleLeftOffset = new Vector2(-0.243f, 0.047f);
    [SerializeField] private Vector3 particleRightRotation = new Vector3(0f, 90f, 0f);
    [SerializeField] private Vector3 particleLeftRotation = new Vector3(0f, -90f, 0f);

    [Header("Shotgun Damage")]
    [SerializeField, Min(1)] private int shotgunParticleDamage = 1;

    [Header("Pickup Notifications")]
    [SerializeField] private PopupData shootingPopup;

    private GameObject wing;
    private GameObject item;
    bool canPickupItem = false;
    GameObject shotgun;
    private ShotgunFireReload shotgunController;
    private int ammoCount =0;

    [Header("UI elements")] 
    [SerializeField] private Image shotgunCounter;
    [SerializeField] private Image leftShell;
    [SerializeField] private Image rightShell;
    [SerializeField] private TextMeshProUGUI counter;
    
    void Awake()
    {
        shotgunCounter.enabled = false;
        leftShell.enabled = false;
        rightShell.enabled = false;
        counter.enabled = false;
        wing = GameObject.FindGameObjectWithTag("Wing");
        shotgun = wing.GetComponentInChildren<SpriteRenderer>().gameObject;
        shotgunController = shotgun.GetComponent<ShotgunFireReload>();
        if (shotgunController == null)
            shotgunController = shotgun.AddComponent<ShotgunFireReload>();

        shotgunController.Configure(
            shotgunRightOffset,
            shotgunLeftOffset,
            particleRightOffset,
            particleLeftOffset,
            particleRightRotation,
            particleLeftRotation,
            shotgunParticleDamage);

        shotgun.SetActive(false);
    }

    void Update()
    {
        PickUp();
    }

    private void PickUp()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && canPickupItem && item != null)
        {
            if (item.tag == "ShotGun")
            {
                shotgun.SetActive(true);
                shotgunCounter.enabled = true;
                shotgunController.OnPickedUp();

                if (PopUpManager.Instance != null)
                    PopUpManager.Instance.Show(shootingPopup);
            }
            if (item.tag == "Ammo")
            {
                AmmoCount+= 10;
                counter.enabled = true;
                counter.text = AmmoCount.ToString();
            }
            Destroy(item);
            item = null;
            canPickupItem = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "ShotGun" || collision.tag == "Ammo")
        {
            canPickupItem = true;
            item = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == item)
        {
            item = null;
            canPickupItem = false;
        }
    }
    
    public int AmmoCount
    {
        get { return ammoCount; }
        set { ammoCount = value; }
    }
}
