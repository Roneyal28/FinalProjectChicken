using UnityEngine;
using UnityEngine.InputSystem;

public class ShotgunFireReload : MonoBehaviour
{
    private ItemsManagement items;
   // [SerializeField] private int ammoCount;
   private bool oneInChamber = false;
    private Animator shotgunAnim;
    void Awake()
    {
        shotgunAnim = GetComponent<Animator>();
        items = GetComponentInParent<Transform>().gameObject.GetComponentInParent<ItemsManagement>();
    }

    void Update()
    {
       // ammoCount = items.AmmoCount;
        ShotgunAnimChange();
    }
    
    public void ShotgunAnimChange()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && items.AmmoCount > 0)
        {
            shotgunAnim.SetBool("isReloading", true);
            items.AmmoCount--;
            oneInChamber = true;
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame && oneInChamber)
        {
            shotgunAnim.SetBool("isFiring", true);
            oneInChamber = false;
        }
        if(shotgunAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9 && !shotgunAnim.GetCurrentAnimatorStateInfo(0).IsName("IdleAnim"))
        {
            shotgunAnim.SetBool("isFiring", false);
            shotgunAnim.SetBool("isReloading", false);
        }
    }
}
