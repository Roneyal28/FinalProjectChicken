using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class BreakableBarrel : MonoBehaviour
{
    [Header("Barrel Damage States")]
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite halfBrokenSprite;
    [SerializeField] private Sprite brokenSprite;

    [Header("Hidden Pickups")]
    [Tooltip("Shotgun, ammo, and keys inside this radius are hidden until the barrel breaks.")]
    [SerializeField, Min(0.1f)] private float pickupSearchRadius = 2f;
    [SerializeField] private PopupData pickupPopup;

    private readonly List<GameObject> hiddenPickups = new List<GameObject>();
    private SpriteRenderer barrelRenderer;
    private Collider2D barrelCollider;
    private SoundFXManager soundFXManager;
    private int hitCount;
    private bool isBroken;

    private void Awake()
    {
        barrelRenderer = GetComponent<SpriteRenderer>();
        barrelCollider = GetComponent<Collider2D>();
        soundFXManager = FindFirstObjectByType<SoundFXManager>();

        if (fullSprite != null)
            barrelRenderer.sprite = fullSprite;
    }

    private void Start()
    {
        HideNearbyPickups();
    }

    public void TakeWingHit()
    {
        if (isBroken)
            return;

        hitCount++;

        if (hitCount == 1)
        {
            SetBarrelSpriteKeepingBottom(halfBrokenSprite);
            GetSoundFXManager()?.PlayBarrelHit();

            return;
        }

        BreakBarrel();
    }

    private void HideNearbyPickups()
    {
        hiddenPickups.Clear();
        HashSet<GameObject> foundPickups = new HashSet<GameObject>();

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, pickupSearchRadius);
        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            GameObject nearbyObject = nearbyCollider.gameObject;
            if (!nearbyObject.CompareTag("ShotGun") &&
                !nearbyObject.CompareTag("Ammo") &&
                !nearbyObject.CompareTag("key"))
                continue;

            if (foundPickups.Add(nearbyObject))
                hiddenPickups.Add(nearbyObject);
        }

        SetPickupsActive(false);
    }

    private void BreakBarrel()
    {
        isBroken = true;
        GetSoundFXManager()?.PlayBarrelBreak();

        SetBarrelSpriteKeepingBottom(brokenSprite);

        if (barrelCollider != null)
            barrelCollider.enabled = false;

        SetPickupsActive(true);

        if (pickupPopup != null && PopUpManager.Instance != null)
            PopUpManager.Instance.Show(pickupPopup);
    }

    private SoundFXManager GetSoundFXManager()
    {
        if (soundFXManager == null)
            soundFXManager = FindFirstObjectByType<SoundFXManager>();

        return soundFXManager;
    }

    private void SetBarrelSpriteKeepingBottom(Sprite nextSprite)
    {
        if (nextSprite == null || barrelRenderer == null)
            return;

        float previousBottom = barrelRenderer.bounds.min.y;
        barrelRenderer.sprite = nextSprite;
        float bottomCorrection = previousBottom - barrelRenderer.bounds.min.y;
        transform.position += Vector3.up * bottomCorrection;
    }

    private void SetPickupsActive(bool active)
    {
        foreach (GameObject pickup in hiddenPickups)
        {
            if (pickup != null)
                pickup.SetActive(active);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupSearchRadius);
    }
}
