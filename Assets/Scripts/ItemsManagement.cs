using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemsManagement : MonoBehaviour
{
    private GameObject wing;
    private GameObject item;
    bool canPickupItem = false;
    
    void Awake()
    {
        wing = GameObject.FindGameObjectWithTag("Wing");
        wing.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && canPickupItem && item != null)
        {
            if (item.tag == "ShotGun")
            {
                wing.SetActive(true);
            }
            item.SetActive(false);
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
}
