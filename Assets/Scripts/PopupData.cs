using UnityEngine;

[CreateAssetMenu(
   fileName = "NewPopupData",
   menuName = "Game/Popup Data")]
public class PopupData : ScriptableObject
{
   [TextArea(2, 6)] public string message;
   [Min(0f)]
   public float displayTime = 2f;
   [Min(0f)]
   public float fadeInDuration = 1f;
   [Min(0f)]
   public float fadeDuration = 1f;

   [Header("Optional Size Override")]
   [Tooltip("Use a different background size for this message only.")]
   public bool useCustomSize;
   [Min(1f)]
   public Vector2 customSize = new Vector2(500f, 80f);
}
