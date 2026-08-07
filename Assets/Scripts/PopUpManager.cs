using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
   private readonly struct PopupRequest
   {
      public readonly PopupData Popup;
      public readonly bool ShowOnlyWithoutKey;

      public PopupRequest(PopupData popup, bool showOnlyWithoutKey)
      {
         Popup = popup;
         ShowOnlyWithoutKey = showOnlyWithoutKey;
      }
   }

   public static PopUpManager Instance { get; private set; }

   [Header("Popup UI")]
   [Tooltip("The HUD panel that contains the popup text.")]
   [SerializeField] private GameObject notificatonParent;
   [Tooltip("The TextMeshPro text inside the popup panel.")]
   [SerializeField] private TMP_Text notificatonText;
   [Tooltip("The image RectTransform behind the text. Found automatically when left empty.")]
   [SerializeField] private RectTransform popupBackground;

   private readonly Queue<PopupRequest> notificationQueue = new Queue<PopupRequest>();
   private CanvasGroup notificationUICanvasGroup;
   private Coroutine displayRoutine;
   private RectTransform popupTextRect;
   private Vector2 defaultBackgroundSize;
   private Vector2 defaultTextSize;
   private SoundFXManager soundFXManager;



   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }

      Instance = this;
      soundFXManager = FindFirstObjectByType<SoundFXManager>();

      if (notificatonParent == null)
         notificatonParent = gameObject;

      if (notificatonText == null)
         notificatonText = notificatonParent.GetComponentInChildren<TMP_Text>(true);

      popupTextRect = notificatonText != null ? notificatonText.rectTransform : null;

      if (popupBackground == null)
      {
         RectTransform[] rects = notificatonParent.GetComponentsInChildren<RectTransform>(true);
         foreach (RectTransform rect in rects)
         {
            if (rect != popupTextRect && rect.name.ToLowerInvariant().Contains("background"))
            {
               popupBackground = rect;
               break;
            }
         }
      }

      if (popupBackground != null)
         defaultBackgroundSize = popupBackground.sizeDelta;

      if (popupTextRect != null)
         defaultTextSize = popupTextRect.sizeDelta;

      notificationUICanvasGroup = notificatonParent.GetComponent<CanvasGroup>();
      if (notificationUICanvasGroup == null)
         notificationUICanvasGroup = notificatonParent.AddComponent<CanvasGroup>();

      SetVisible(false);

   }

   private void OnDestroy()
   {
      if (Instance == this)
         Instance = null;
   }

   public void Show(PopupData popup, bool showOnlyWithoutKey = false)
   {
      if (popup == null)
      {
         Debug.LogWarning("A PopupTrigger tried to display an empty PopupData reference.", this);
         return;
      }

      if (notificatonText == null)
      {
         Debug.LogError("PopUpManager needs a TextMeshPro text reference inside its popup panel.", this);
         return;
      }

      bool requiresMissingKey = showOnlyWithoutKey || popup.showOnlyWithoutKey;
      notificationQueue.Enqueue(new PopupRequest(popup, requiresMissingKey));

      if (displayRoutine == null)
         displayRoutine = StartCoroutine(DisplayQueue());
   }

   private IEnumerator DisplayQueue()
   {
      while (notificationQueue.Count > 0)
      {
         PopupRequest request = notificationQueue.Dequeue();
         if (request.ShowOnlyWithoutKey && PlayerHasKey())
            continue;

         PopupData popup = request.Popup;
         notificatonText.text = popup.message;
         ApplyPopupSize(popup);
         PlayPopupSound();

         notificationUICanvasGroup.interactable = false;
         notificationUICanvasGroup.blocksRaycasts = false;

         float fadeInDuration = Mathf.Max(0f, popup.fadeInDuration);
         if (fadeInDuration > 0f)
         {
            float elapsed = 0f;
            notificationUICanvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
               elapsed += Time.deltaTime;
               notificationUICanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
               yield return null;
            }
         }

         notificationUICanvasGroup.alpha = 1f;

         yield return new WaitForSeconds(Mathf.Max(0f, popup.displayTime));

         float fadeDuration = Mathf.Max(0f, popup.fadeDuration);
         if (fadeDuration > 0f)
         {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
               elapsed += Time.deltaTime;
               notificationUICanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
               yield return null;
            }
         }

         SetVisible(false);
      }

      displayRoutine = null;
   }

   private static bool PlayerHasKey()
   {
      ItemsManagement itemsManagement = FindFirstObjectByType<ItemsManagement>();
      return itemsManagement != null && itemsManagement.HasKey;
   }

   private void PlayPopupSound()
   {
      if (soundFXManager == null)
         soundFXManager = FindFirstObjectByType<SoundFXManager>();

      soundFXManager?.PlayPopupSound();
   }

   private void ApplyPopupSize(PopupData popup)
   {
      Vector2 backgroundSize = popup.useCustomSize ? popup.customSize : defaultBackgroundSize;

      if (popupBackground != null)
         popupBackground.sizeDelta = backgroundSize;

      if (popupTextRect != null)
         popupTextRect.sizeDelta = popup.useCustomSize ? popup.customSize : defaultTextSize;
   }

   private void SetVisible(bool visible)
   {
      notificationUICanvasGroup.alpha = visible ? 1f : 0f;
      notificationUICanvasGroup.interactable = false;
      notificationUICanvasGroup.blocksRaycasts = false;
   }
}
