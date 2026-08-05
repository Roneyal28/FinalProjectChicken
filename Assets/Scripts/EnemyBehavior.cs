using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float maxDistanceFromStart = 4f;

    [Header("Timing")]
    [SerializeField] private bool startMoving = true;
    [SerializeField] private float minMoveTime = 1f;
    [SerializeField] private float maxMoveTime = 3f;
    [SerializeField] private float minIdleTime = 0.4f;
    [SerializeField] private float maxIdleTime = 1.2f;
    [Range(0f, 1f)]
    [SerializeField] private float idleChance = 0.35f;
    [SerializeField] private ParticleSystem damageParticles;
    [Header("Animation")]
    [SerializeField] private string walkingBoolName = "isWalking";
    [SerializeField] private string walkStateName = "ratWalk";
    [SerializeField] private string idleStateName = "ratIdle";
    [SerializeField] private string damageStateName = "ratDamage";
    [SerializeField] private string deathStateName = "ratDie";
    [SerializeField] private float damageAnimationDuration = 0.35f;
    [SerializeField] private float deathAnimationDuration = 1f;

    [Header("Collision")]
    [SerializeField] private string enemyLayerName = "Enemy";
    [SerializeField] private string playerLayerName = "Player";

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private GameObject deathEffect;

    private Rigidbody2D enemyRB;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private SoundFXManager SFXManager;
    private Vector2 startPosition;
    private float actionTimer;
    private int direction = 1;
    private bool isMoving = true;
    private bool hasWalkingBool;
    private bool previousMovingState;
    private bool hasSetAnimationState;
    private bool isTakingDamage;
    private bool isDead;
    private float damageTimer;
    private Color normalColor;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        normalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        SFXManager = FindObjectOfType<SoundFXManager>();
        startPosition = transform.position;
        currentHealth = maxHealth;
        hasWalkingBool = HasAnimatorBool(walkingBoolName);
        SetupIgnoredCollisions();
        SetupRigidbody();
    }

    private void Start()
    {
        if (startMoving)
        {
            StartMoving();
        }
        else
        {
            PickNewAction();
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            StopMoving();
            return;
        }

        bool damageAnimationPlaying = UpdateDamageTimer();

        actionTimer -= Time.fixedDeltaTime;

        if (actionTimer <= 0f)
        {
            PickNewAction();
        }

        if (isMoving)
        {
            Move();
        }
        else
        {
            StopMoving();
        }

        if (!damageAnimationPlaying)
        {
            UpdateAnimation();
        }
    }

    private void LateUpdate()
    {
        if (!isTakingDamage && !isDead)
        {
            RestoreNormalColor();
        }
    }

    private bool UpdateDamageTimer()
    {
        if (!isTakingDamage)
        {
            return false;
        }

        damageTimer -= Time.fixedDeltaTime;

        if (damageTimer <= 0f)
        {
            isTakingDamage = false;
            isMoving = true;
            actionTimer = Random.Range(minMoveTime, maxMoveTime);
            hasSetAnimationState = false;
            RestoreNormalColor();
            return false;
        }

        return true;
    }

    private void PickNewAction()
    {
        isMoving = Random.value > idleChance;

        if (isMoving)
        {
            StartMoving();
        }
        else
        {
            actionTimer = Random.Range(minIdleTime, maxIdleTime);
        }
    }

    private void StartMoving()
    {
        isMoving = true;
        direction = Random.value < 0.5f ? -1 : 1;
        actionTimer = Random.Range(minMoveTime, maxMoveTime);
    }

    private void Move()
    {
        float distanceFromStart = transform.position.x - startPosition.x;

        if (distanceFromStart >= maxDistanceFromStart)
        {
            direction = -1;
        }
        else if (distanceFromStart <= -maxDistanceFromStart)
        {
            direction = 1;
        }

        if (enemyRB != null)
        {
            enemyRB.linearVelocity = new Vector2(direction * moveSpeed, enemyRB.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector2.right * direction * moveSpeed * Time.fixedDeltaTime);
        }

        FlipSprite();
    }

    private void StopMoving()
    {
        if (enemyRB != null)
        {
            enemyRB.linearVelocity = new Vector2(0f, enemyRB.linearVelocity.y);
        }
    }

    private void FlipSprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (hasWalkingBool)
        {
            animator.SetBool(walkingBoolName, isMoving);
        }

        if (!hasSetAnimationState || previousMovingState != isMoving)
        {
            RestoreNormalColor();
            animator.Play(isMoving ? walkStateName : idleStateName, 0, 0f);
            previousMovingState = isMoving;
            hasSetAnimationState = true;
        }
    }

    private void RestoreNormalColor()
    {
        if (spriteRenderer != null && !isDead)
        {
            spriteRenderer.color = normalColor;
        }
    }

    private bool HasAnimatorBool(string parameterName)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Bool)
            {
                return true;
            }
        }

        return false;
    }

    private void SetupIgnoredCollisions()
    {
        int enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        int playerLayer = LayerMask.NameToLayer(playerLayerName);

        if (enemyLayer >= 0)
        {
            gameObject.layer = enemyLayer;
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        }

        if (enemyLayer >= 0 && playerLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, true);
        }
    }

    private void SetupRigidbody()
    {
        if (enemyRB == null)
        {
            return;
        }

        enemyRB.bodyType = RigidbodyType2D.Dynamic;
        enemyRB.simulated = true;
        enemyRB.gravityScale = 1f;
        enemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        enemyRB.WakeUp();
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;

        SpawnDamageParticles(attackDirection);

        if (currentHealth <= 0)
        {
            StartDeath();
            return;
        }

        PlayDamageAnimation();
    }

    private void PlayDamageAnimation()
    {
        isTakingDamage = true;
        damageTimer = damageAnimationDuration;

        if (animator != null)
        {
            animator.Play(damageStateName, 0, 0f);
        }

        if (SFXManager != null)
        {
            SFXManager.PlaySFX(SFXManager.ratHit);
        }
    }

    private void StartDeath()
    {
        isDead = true;
        currentHealth = 0;
        StopMoving();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.Play(deathStateName, 0, 0f);
        }

        if (SFXManager != null)
        {
            SFXManager.PlaySFX(SFXManager.ratDeath);
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, deathAnimationDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Vector3 left = center + Vector3.left * maxDistanceFromStart;
        Vector3 right = center + Vector3.right * maxDistanceFromStart;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawWireSphere(left, 0.1f);
        Gizmos.DrawWireSphere(right, 0.1f);
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
        Instantiate(damageParticles, transform.position, spawnRotation);
    }
}
