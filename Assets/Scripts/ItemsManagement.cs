using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemsManagement : MonoBehaviour
{
    private GameObject wing;
    private GameObject item;
    bool canPickupItem = false;
    GameObject shotgun;
    private Animator shotgunAnim;
    private int ammoCount =0;
    void Awake()
    {
        wing = GameObject.FindGameObjectWithTag("Wing");
        shotgun = wing.GetComponentInChildren<SpriteRenderer>().gameObject;
        shotgunAnim = shotgun.GetComponent<Animator>();
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
            }
            if (item.tag == "Ammo")
            {
                AmmoCount+= 10;
            }
            Destroy(item);
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
    
    public int AmmoCount
    {
        get { return ammoCount; }
        set { ammoCount = value; }
    }
}
