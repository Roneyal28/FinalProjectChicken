using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnlockDoor : MonoBehaviour
{
    [SerializeField] GameObject closedDoor;
    [SerializeField] GameObject openDoor;
    private ItemsManagement items;
    bool canOpenDoor = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        openDoor.SetActive(false);
        items = GameObject.FindGameObjectWithTag("Player").GetComponent<ItemsManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && canOpenDoor)
        {
            closedDoor.SetActive(false);
            openDoor.SetActive(true);
            items.HasKey = false;
        }
    }

    
   private void OnTriggerStay2D (Collider2D collision)
    {
        if (collision.CompareTag("Player") && items != null && items.HasKey)
        {
            canOpenDoor = true;
        }
    }
}
