using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChickenController : MonoBehaviour
{
    [Header("Chicken Components")]
    private SpriteRenderer chickenSR;
    private Rigidbody2D chickenRB;
    private Animator anim;
    private GameObject wing;
    private Vector3 wingOriginalPosition;
    private SpriteRenderer shotgunSR;
    private ShotgunFireReload shotgunController;
    
    [Header("Movement")]
    [SerializeField] int Step = 1;
    [SerializeField] int speed = 5;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 4f;
    [SerializeField] LayerMask floorLayer;
    private bool isOnFloor;

    [Header("Kinematic Borders")]
    [SerializeField] bool useKinematicBorders = true;
    [SerializeField] Vector2 minKinematicPosition = new Vector2(-38f, -10.7f);
    [SerializeField] Vector2 maxKinematicPosition = new Vector2(38f, -2.7f);

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;
    private bool isGrounded;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private ParticleSystem damageParticles;

    [Header("Attack")]
    [SerializeField] private int attackDamage = 50;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackHeight = 1.2f;
    [SerializeField] private LayerMask enemyLayer;
    
    SoundFXManager SFXManager;

    private Collider2D chickenCollider;
    private float startingGravityScale;
    private Vector2 moveInput;
    private Vector2 currentMoveVelocity;
    private bool jumpPressed;
    private bool attackPressed;
    private bool useGravityMode;
    private bool isJumping;
    private float jumpStartY;
    private bool jumpAnimationLocked;
    private int jumpAnimationStartFrame;
    private bool attackAnimationLocked;
    private int attackAnimationStartFrame;
    private bool hurtAnimationLocked;
    private int hurtAnimationStartFrame;
    private string isWalking = "isWalking";

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    bool IsAnimationFinished(string animationName)
    {
        if (anim == null)
        {
            return true;
        }

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(animationName)) return true;
        if (info.normalizedTime >0.95f) return true;
        return false;
    }

    void Awake()
    {
        chickenSR = GetComponent<SpriteRenderer>();
        chickenRB = GetComponent<Rigidbody2D>();
        chickenCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        wing = GameObject.FindGameObjectWithTag("Wing");
        wingOriginalPosition = wing.transform.position;
        shotgunSR= wing.GetComponentInChildren<SpriteRenderer>();
        startingGravityScale = chickenRB.gravityScale;
        currentHealth = maxHealth;
        if (healthBar == null || !healthBar.HasBothSliders)
        {
            foreach (HealthBar foundHealthBar in FindObjectsOfType<HealthBar>())
            {
                if (foundHealthBar.HasBothSliders)
                {
                    healthBar = foundHealthBar;
                    break;
                }
            }
        }

        SetupHealthBar();
        MoveKinematicOnFloor();
        SFXManager = FindObjectOfType<SoundFXManager>().GetComponent<SoundFXManager>();
    }

    void Update()
    {
        ReadInput();
        CheckFloor();
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            currentHealth = Mathf.Clamp(currentHealth , 0, maxHealth);
            UpdateHealthBar();
        }
        if (jumpPressed && CanJump())
        {
            Jump();
            PlayJumpAnimation();
        }

        if (attackPressed && !jumpAnimationLocked && !attackAnimationLocked && !hurtAnimationLocked)
        {
            PlayAttackAnimation();
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        CheckFloor();

        if (!isOnFloor)
        {
            StartGravityMode();
        }

        if (useGravityMode)
        {
            MoveWithGravity();
        }
        else
        {
            MoveKinematicOnFloor();
        }

        jumpPressed = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetGroundCheckPosition(), groundCheckRadius);
        }

        if (useKinematicBorders)
        {
            Vector2 min = GetMinBorder();
            Vector2 max = GetMaxBorder();
            Vector2 center = (min + max) * 0.5f;
            Vector2 size = max - min;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetAttackCenter(), GetAttackSize());
    }

    private void ReadInput()
    {
        moveInput = Vector2.zero;
        attackPressed = false;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            moveInput.y += 1;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            moveInput.x -= 1;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            moveInput.y -= 1;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            moveInput.x += 1;
        }

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        attackPressed = Keyboard.current.fKey.wasPressedThisFrame;

        if (attackAnimationLocked)
        {
            moveInput = Vector2.zero;
            jumpPressed = false;
        }
    }

    private void CheckFloor()
    {
        isOnFloor = IsTouchingWalkableFloor();
        isGrounded = IsTouchingSolidFloor();
    }

    private bool IsTouchingWalkableFloor()
    {
        if (chickenCollider == null)
        {
            return false;
        }

        LayerMask walkableLayer = floorLayer.value == 0 ? groundLayer : floorLayer;
        Bounds bounds = chickenCollider.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f, walkableLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit != chickenCollider && hit.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsTouchingSolidFloor()
    {
        if (chickenCollider == null)
        {
            return false;
        }

        Bounds bounds = chickenCollider.bounds;
        Vector2 checkOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.7f, 0.05f);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            checkOrigin,
            checkSize,
            0f,
            Vector2.down,
            groundCheckRadius);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == chickenCollider || hit.collider.isTrigger)
            {
                continue;
            }

            if (hit.normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetGroundCheckPosition()
    {
        if (chickenCollider != null)
        {
            Bounds bounds = chickenCollider.bounds;
            return new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        }

        return groundCheck.position;
    }

    private bool CanJump()
    {
        return isOnFloor || isGrounded;
    }

    private void Jump()
    {
        StartGravityMode();
        isJumping = true;
        jumpStartY = chickenRB.position.y;
        chickenRB.linearVelocity = new Vector2(currentMoveVelocity.x * Step * speed, jumpForce);
    }

    private void MoveKinematicOnFloor()
    {
        chickenRB.bodyType = RigidbodyType2D.Kinematic;
        chickenRB.gravityScale = 0f;
        chickenRB.linearVelocity = Vector2.zero;

        UpdateSmoothedMovement();

        if (currentMoveVelocity.x != 0)
        {
            FlipChicken(currentMoveVelocity.x);
        }

        Vector2 movement = currentMoveVelocity * Step * speed * Time.fixedDeltaTime;
        Vector2 nextPosition = ClampToKinematicBorders(chickenRB.position + movement);
        chickenRB.MovePosition(nextPosition);
    }

    private void MoveWithGravity()
    {
        chickenRB.bodyType = RigidbodyType2D.Dynamic;
        chickenRB.gravityScale = startingGravityScale;

        UpdateSmoothedMovement();

        if (currentMoveVelocity.x != 0)
        {
            FlipChicken(currentMoveVelocity.x);
        }

        chickenRB.linearVelocity = new Vector2(currentMoveVelocity.x * Step * speed, chickenRB.linearVelocity.y);

        bool landedFromJump = isJumping &&
            isOnFloor &&
            chickenRB.linearVelocity.y <= 0f &&
            chickenRB.position.y <= jumpStartY;

        bool landedAfterFalling = !isJumping &&
            isOnFloor &&
            chickenRB.linearVelocity.y <= 0.05f;

        bool landedOnSolidFloor = isOnFloor &&
            isGrounded &&
            chickenRB.linearVelocity.y <= 0.05f;

        if (landedFromJump || landedAfterFalling || landedOnSolidFloor)
        {
            isJumping = false;
            useGravityMode = false;
            chickenRB.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateSmoothedMovement()
    {
        if (attackAnimationLocked)
        {
            currentMoveVelocity = Vector2.zero;
            return;
        }

        float smoothing = moveInput == Vector2.zero ? deceleration : acceleration;
        currentMoveVelocity = Vector2.MoveTowards(
            currentMoveVelocity,
            moveInput,
            smoothing * Time.fixedDeltaTime
        );
    }

    private void StartGravityMode()
    {
        useGravityMode = true;
        chickenRB.bodyType = RigidbodyType2D.Dynamic;
        chickenRB.gravityScale = startingGravityScale;
    }

    private Vector2 ClampToKinematicBorders(Vector2 position)
    {
        if (!useKinematicBorders)
        {
            return position;
        }

        Vector2 min = GetMinBorder();
        Vector2 max = GetMaxBorder();

        return new Vector2(
            Mathf.Clamp(position.x, min.x, max.x),
            Mathf.Clamp(position.y, min.y, max.y)
        );
    }

    private Vector2 GetMinBorder()
    {
        return new Vector2(
            Mathf.Min(minKinematicPosition.x, maxKinematicPosition.x),
            Mathf.Min(minKinematicPosition.y, maxKinematicPosition.y)
        );
    }

    private Vector2 GetMaxBorder()
    {
        return new Vector2(
            Mathf.Max(minKinematicPosition.x, maxKinematicPosition.x),
            Mathf.Max(minKinematicPosition.y, maxKinematicPosition.y)
        );
    }

    private void UpdateAnimation()
    {
        if (anim == null)
        {
            return;
        }

        if (hurtAnimationLocked)
        {
            anim.SetBool(isWalking, false);

            if (Time.frameCount > hurtAnimationStartFrame && IsAnimationFinished("HurtAnimation"))
            {
                hurtAnimationLocked = false;
                RefreshShotgunAnimationVisibility();
                PlayAnimationFromStart(moveInput != Vector2.zero ? "ChickenWalk" : "IdleAnimation");
            }
            else
            {
                return;
            }
        }

        if (jumpAnimationLocked)
        {
            anim.SetBool(isWalking, false);

            if (Time.frameCount > jumpAnimationStartFrame && IsAnimationFinished("JumpAnimation"))
            {
                jumpAnimationLocked = false;
                RefreshShotgunAnimationVisibility();
            }
            else
            {
                return;
            }
        }

        if (attackAnimationLocked)
        {
            anim.SetBool(isWalking, false);

            if (Time.frameCount > attackAnimationStartFrame && IsAnimationFinished("AttackAnimation"))
            {
                attackAnimationLocked = false;
                RefreshShotgunAnimationVisibility();
                PlayAnimationFromStart(moveInput != Vector2.zero ? "ChickenWalk" : "IdleAnimation");
            }
            else
            {
                return;
            }
        }

        anim.SetBool(isWalking, moveInput != Vector2.zero);
    }

    private void PlayJumpAnimation()
    {
        jumpAnimationLocked = true;
        jumpAnimationStartFrame = Time.frameCount;
        RefreshShotgunAnimationVisibility();
        PlayAnimationFromStart("JumpAnimation");
        SFXManager.PlaySFX(SFXManager.jump);
        SFXManager.PlaySFX(SFXManager.jump2);
    }

    private void RefreshShotgunAnimationVisibility()
    {
        if (shotgunSR == null)
            return;

        if (shotgunController == null)
            shotgunController = shotgunSR.GetComponent<ShotgunFireReload>();

        if (shotgunController != null)
        {
            bool shouldHide = jumpAnimationLocked || attackAnimationLocked || hurtAnimationLocked;
            shotgunController.SetChickenAnimationHidden(shouldHide);
        }
    }

    private void PlayAttackAnimation()
    {
        attackAnimationLocked = true;
        attackAnimationStartFrame = Time.frameCount;
        RefreshShotgunAnimationVisibility();
        moveInput = Vector2.zero;
        currentMoveVelocity = Vector2.zero;
        chickenRB.linearVelocity = Vector2.zero;
        anim.SetBool(isWalking, false);
        PlayAnimationFromStart("AttackAnimation");

        if (SFXManager != null && SFXManager.wingAttack != null)
        {
            SFXManager.PlaySFX(SFXManager.wingAttack);
        }

        if (SFXManager != null && SFXManager.wingAttack2 != null)
        {
            SFXManager.PlaySFX(SFXManager.wingAttack2);
        }

    }

    public void AttackHit()
    {
        DamageEnemiesInAttackRange();
    }

    private void PlayAnimationFromStart(string animationName)
    {
        if (anim != null)
        {
            anim.Play(animationName);
        }
    }

    private void DamageEnemiesInAttackRange()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(GetAttackCenter(), GetAttackSize(), 0f, GetEnemyLayerMask());
        HashSet<EnemyBehavior> damagedEnemies = new HashSet<EnemyBehavior>();
        HashSet<WolfController> damagedWolves = new HashSet<WolfController>();

        foreach (Collider2D hit in hits)
        {
            EnemyBehavior enemy = hit.GetComponentInParent<EnemyBehavior>();

            if (enemy != null && damagedEnemies.Add(enemy))
            {
                Vector2 attackDirection = (Vector2)enemy.transform.position - GetAttackCenter();
                enemy.TakeDamage(attackDamage, attackDirection);
            }

            WolfController wolf = hit.GetComponentInParent<WolfController>();

            if (wolf != null && damagedWolves.Add(wolf))
            {
                Vector2 attackDirection = (Vector2)wolf.transform.position - GetAttackCenter();
                wolf.TakeDamage(attackDamage, attackDirection);
            }
        }

        Collider2D[] barrelHits = Physics2D.OverlapBoxAll(GetAttackCenter(), GetAttackSize(), 0f);
        HashSet<BreakableBarrel> damagedBarrels = new HashSet<BreakableBarrel>();

        foreach (Collider2D hit in barrelHits)
        {
            BreakableBarrel barrel = hit.GetComponentInParent<BreakableBarrel>();
            if (barrel != null && damagedBarrels.Add(barrel))
                barrel.TakeWingHit();
        }
    }

    private Vector2 GetAttackCenter()
    {
        float facingDirection = chickenSR != null && chickenSR.flipX ? -1f : 1f;
        Vector2 center = transform.position;

        if (chickenCollider != null)
        {
            Bounds bounds = chickenCollider.bounds;
            center = bounds.center;
            center.x += facingDirection * (bounds.extents.x + attackRange * 0.5f);
        }
        else
        {
            center.x += facingDirection * attackRange;
        }

        return center;
    }

    private Vector2 GetAttackSize()
    {
        return new Vector2(attackRange, attackHeight);
    }

    private LayerMask GetEnemyLayerMask()
    {
        if (enemyLayer.value != 0)
        {
            return enemyLayer;
        }

        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        if (enemyLayerIndex >= 0)
        {
            return 1 << enemyLayerIndex;
        }

        return Physics2D.DefaultRaycastLayers;
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth >= previousHealth)
        {
            return;
        }

        SpawnDamageParticles(attackDirection);
        PlayHurtAnimation();
        if (SFXManager != null)
        {
            SFXManager.PlayRandomChickenDamageSound();
        }
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayHurtAnimation()
    {
        if (anim == null)
        {
            return;
        }

        hurtAnimationLocked = true;
        hurtAnimationStartFrame = Time.frameCount;
        jumpAnimationLocked = false;
        attackAnimationLocked = false;
        RefreshShotgunAnimationVisibility();
        anim.SetBool(isWalking, false);
        PlayAnimationFromStart("HurtAnimation");
    }

    private void SpawnDamageParticles(Vector2 attackDirection)
    {
        if (damageParticles == null)
        {
            return;
        }

        Vector2 direction = attackDirection.sqrMagnitude > 0f
            ? attackDirection.normalized
            : Vector2.right;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle);
        Vector3 spawnPosition = chickenCollider != null
            ? chickenCollider.bounds.center
            : transform.position;
        ParticleSystem instance = Instantiate(damageParticles, spawnPosition, spawnRotation);
        foreach (ParticleSystem particleSystem in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            particleSystem.Play(false);
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    private void SetupHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

    public void FlipChicken(float x)
    {
        bool facingLeft = x < 0;
        chickenSR.flipX = facingLeft;

        if (shotgunSR != null && shotgunSR.gameObject.activeSelf)
        {
            if (shotgunController == null)
                shotgunController = shotgunSR.GetComponent<ShotgunFireReload>();

            if (shotgunController != null)
                shotgunController.SetFacingLeft(facingLeft);
            else
                shotgunSR.flipX = facingLeft;
        }
    }

    public void CanWalkUp()
    {
        if (isOnFloor && !isGrounded && !useGravityMode)
        {
            chickenRB.gravityScale = 0;
        }
        else 
        {
            chickenRB.gravityScale = 1;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BarnExit"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("CutScene");
        }
    }
}
