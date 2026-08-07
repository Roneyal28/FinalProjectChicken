using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PopupTrigger : MonoBehaviour
{
   [SerializeField] private PopupData popup;
   [SerializeField] private bool showOnlyOnce = true;


   private bool hasShown;
   private ItemsManagement itemsManagement;


   private void Reset()
   {
      Collider2D trigger = GetComponent<Collider2D>();
      trigger.isTrigger = true;
   }

   private void OnValidate()
   {
      Collider2D trigger = GetComponent<Collider2D>();
      if (trigger != null)
         trigger.isTrigger = true;
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (!other.CompareTag("Player") || (showOnlyOnce && hasShown))
         return;

      bool requiresMissingKey = popup != null && popup.showOnlyWithoutKey;

      if (itemsManagement == null)
         itemsManagement = other.GetComponentInParent<ItemsManagement>();

      if (itemsManagement == null)
         itemsManagement = FindFirstObjectByType<ItemsManagement>();

      if (requiresMissingKey && itemsManagement != null && itemsManagement.HasKey)
      {
         if (showOnlyOnce)
            hasShown = true;

         return;
      }

      if (PopUpManager.Instance == null)
      {
         Debug.LogError("No active PopUpManager was found in the scene.", this);
         return;
      }

      hasShown = true;
      PopUpManager.Instance.Show(popup, requiresMissingKey);
   }
}
