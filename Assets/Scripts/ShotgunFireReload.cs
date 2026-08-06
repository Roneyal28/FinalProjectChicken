using UnityEngine;
using UnityEngine.InputSystem;

public class ShotgunFireReload : MonoBehaviour
{
    private enum ShotgunState
    {
        Hidden,
        Drawing,
        Idle,
        Reloading,
        Firing
    }

    private ItemsManagement items;
    private ParticleSystem buckshotParticles;
    private ParticleSystemRenderer particleRenderer;
    private Transform particleEmitter;
    private SpriteRenderer shotgunSprite;
    private SpriteRenderer wingSprite;
    private Transform wingTransform;
    private Animator shotgunAnimator;
    private SoundFXManager soundFXManager;
    private ShotgunState state = ShotgunState.Hidden;
    private bool oneInChamber;
    private bool facingLeft;
    private Vector2 rightShotgunOffset;
    private Vector2 leftShotgunOffset;
    private Vector2 particleRightLocalPosition = new Vector2(0.243f, 0.047f);
    private Vector2 particleLeftLocalPosition = new Vector2(-0.243f, 0.047f);
    private Vector3 particleRightLocalEulerAngles = new Vector3(0f, 90f, 0f);
    private Vector3 particleLeftLocalEulerAngles = new Vector3(0f, -90f, 0f);
    private Vector2 lastAppliedShotgunOffset;
    private Vector3 wingReferenceLocalPosition;
    private bool wingReferenceFlipX;
    private bool wingReferenceFacingLeft;
    private bool wingVisibleAfterDraw = true;
    private bool hiddenForChickenAnimation;
    private int particleDamage = 1;
    private int shotId;

    private static readonly int IsReloading = Animator.StringToHash("isReloading");
    private static readonly int IsFiring = Animator.StringToHash("isFiring");

    private void Awake()
    {
        shotgunAnimator = GetComponent<Animator>();
        shotgunSprite = GetComponent<SpriteRenderer>();
        FindWingSprite();
        buckshotParticles = GetComponentInChildren<ParticleSystem>(true);
        items = GetComponentInParent<ItemsManagement>();
        soundFXManager = FindFirstObjectByType<SoundFXManager>();

        if (buckshotParticles != null)
        {
            particleEmitter = buckshotParticles.transform;
            particleRenderer = buckshotParticles.GetComponent<ParticleSystemRenderer>();

            if (particleRenderer != null && shotgunSprite != null)
            {
                particleRenderer.sortingLayerID = shotgunSprite.sortingLayerID;
                particleRenderer.sortingOrder = shotgunSprite.sortingOrder + 1;
            }

            ShotgunParticleDamage damage = buckshotParticles.GetComponent<ShotgunParticleDamage>();
            if (damage == null)
                damage = buckshotParticles.gameObject.AddComponent<ShotgunParticleDamage>();

            damage.SetDamage(particleDamage);
            buckshotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Configure(
        Vector2 rightHandOffset,
        Vector2 leftHandOffset,
        Vector2 rightEmitterOffset,
        Vector2 leftEmitterOffset,
        Vector3 rightEmitterRotation,
        Vector3 leftEmitterRotation,
        int damage)
    {
        rightShotgunOffset = rightHandOffset;
        leftShotgunOffset = leftHandOffset;
        particleRightLocalPosition = rightEmitterOffset;
        particleLeftLocalPosition = leftEmitterOffset;
        particleRightLocalEulerAngles = rightEmitterRotation;
        particleLeftLocalEulerAngles = leftEmitterRotation;
        particleDamage = Mathf.Max(1, damage);

        if (buckshotParticles != null)
        {
            ShotgunParticleDamage particleDamageHandler = buckshotParticles.GetComponent<ShotgunParticleDamage>();
            if (particleDamageHandler != null)
                particleDamageHandler.SetDamage(particleDamage);
        }

        ApplyFacing();
    }

    private void LateUpdate()
    {
        ApplyFacing();
    }

    private void Update()
    {
        UpdateAnimationState();

        if (hiddenForChickenAnimation || state != ShotgunState.Idle)
            return;

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            TryReload();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryFire();
    }

    public void OnPickedUp()
    {
        ChickenController chicken = GetComponentInParent<ChickenController>();
        SpriteRenderer chickenSprite = chicken != null ? chicken.GetComponent<SpriteRenderer>() : null;
        SetFacingLeft(chickenSprite != null && chickenSprite.flipX);

        oneInChamber = false;
        ResetAnimatorParameters();
        SetWingVisible(false);

        if (shotgunAnimator != null)
        {
            PlayAnimation("DrawAnim");
            state = ShotgunState.Drawing;
        }
        else
        {
            state = ShotgunState.Idle;
            SetWingVisible(true);
        }

        if (soundFXManager == null)
            soundFXManager = FindFirstObjectByType<SoundFXManager>();

        soundFXManager?.PlayShotgunDraw();
    }

    public void SetFacingLeft(bool shouldFaceLeft)
    {
        facingLeft = shouldFaceLeft;
        ApplyFacing();
    }

    public void SetChickenAnimationHidden(bool hidden)
    {
        hiddenForChickenAnimation = hidden;
        RefreshWeaponVisibility();
    }

    private void ApplyFacing()
    {
        if (shotgunSprite != null)
            shotgunSprite.flipX = facingLeft;

        ApplyWingFacing();

        Vector3 shotgunPosition = transform.localPosition;
        shotgunPosition.x -= lastAppliedShotgunOffset.x;
        shotgunPosition.y -= lastAppliedShotgunOffset.y;
        shotgunPosition.x = Mathf.Abs(shotgunPosition.x) * (facingLeft ? -1f : 1f);

        lastAppliedShotgunOffset = facingLeft ? leftShotgunOffset : rightShotgunOffset;
        shotgunPosition.x += lastAppliedShotgunOffset.x;
        shotgunPosition.y += lastAppliedShotgunOffset.y;
        transform.localPosition = shotgunPosition;

        if (particleEmitter == null)
            return;

        Vector2 emitterPosition = facingLeft ? particleLeftLocalPosition : particleRightLocalPosition;
        particleEmitter.localPosition = new Vector3(emitterPosition.x, emitterPosition.y, particleEmitter.localPosition.z);
        particleEmitter.localEulerAngles = facingLeft
            ? particleLeftLocalEulerAngles
            : particleRightLocalEulerAngles;
    }

    private void TryReload()
    {
        if (oneInChamber || items == null || items.AmmoCount <= 0)
            return;

        items.AmmoCount--;
        shotgunAnimator.SetBool(IsReloading, true);
        PlayAnimation("ReloadAnim");
        state = ShotgunState.Reloading;
        soundFXManager?.PlayShotgunReload();
    }

    private void TryFire()
    {
        if (!oneInChamber)
            return;

        oneInChamber = false;
        shotgunAnimator.SetBool(IsFiring, true);
        PlayAnimation("FireAnim");
        state = ShotgunState.Firing;
        FireShotgun();
        soundFXManager?.PlayShotgunShoot();
    }

    private void UpdateAnimationState()
    {
        if (state == ShotgunState.Hidden || state == ShotgunState.Idle || shotgunAnimator == null)
            return;

        AnimatorStateInfo currentState = shotgunAnimator.GetCurrentAnimatorStateInfo(0);

        if (state == ShotgunState.Drawing && currentState.IsName("IdleAnim"))
        {
            state = ShotgunState.Idle;
            SetWingVisible(true);
            return;
        }

        if (shotgunAnimator.IsInTransition(0) || currentState.normalizedTime < 1f)
            return;

        bool finishedDrawing = state == ShotgunState.Drawing;

        if (state == ShotgunState.Reloading)
            oneInChamber = true;

        ResetAnimatorParameters();
        PlayAnimation("IdleAnim");
        state = ShotgunState.Idle;

        if (finishedDrawing)
            SetWingVisible(true);
    }

    private void FindWingSprite()
    {
        foreach (SpriteRenderer childSprite in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (childSprite == shotgunSprite)
                continue;

            wingSprite = childSprite;
            wingTransform = childSprite.transform;
            wingReferenceLocalPosition = wingTransform.localPosition;
            wingReferenceFlipX = wingSprite.flipX;
            wingReferenceFacingLeft = shotgunSprite != null && shotgunSprite.flipX;
            break;
        }
    }

    private void ApplyWingFacing()
    {
        if (wingSprite == null || wingTransform == null)
            return;

        bool mirrorFromPlacedDirection = facingLeft != wingReferenceFacingLeft;
        wingSprite.flipX = mirrorFromPlacedDirection ? !wingReferenceFlipX : wingReferenceFlipX;

        Vector3 wingPosition = wingReferenceLocalPosition;
        if (mirrorFromPlacedDirection)
            wingPosition.x = -wingPosition.x;

        wingTransform.localPosition = wingPosition;
    }

    private void SetWingVisible(bool visible)
    {
        wingVisibleAfterDraw = visible;
        RefreshWeaponVisibility();
    }

    private void RefreshWeaponVisibility()
    {
        if (shotgunSprite != null)
            shotgunSprite.enabled = !hiddenForChickenAnimation;

        if (wingSprite != null)
            wingSprite.enabled = !hiddenForChickenAnimation && wingVisibleAfterDraw;
    }

    private void PlayAnimation(string stateName)
    {
        if (shotgunAnimator != null)
            shotgunAnimator.Play(stateName, 0, 0f);
    }

    private void ResetAnimatorParameters()
    {
        if (shotgunAnimator == null)
            return;

        shotgunAnimator.SetBool(IsReloading, false);
        shotgunAnimator.SetBool(IsFiring, false);
    }

    public void FireShotgun()
    {
        if (buckshotParticles == null)
            return;

        shotId++;
        ShotgunParticleDamage damageHandler = buckshotParticles.GetComponent<ShotgunParticleDamage>();
        if (damageHandler != null)
            damageHandler.BeginShot(shotId, particleDamage);

        buckshotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        buckshotParticles.Play(true);
    }
}
