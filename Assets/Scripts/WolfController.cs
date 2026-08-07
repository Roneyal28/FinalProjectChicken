using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class WolfController : MonoBehaviour
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

    [Header("Attack")]
    public int attackDamage = 20;
    [SerializeField] private float attackRange = 1.2f;
    public float attackDelay = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private ParticleSystem damageParticles;

    [Header("Animation")]
    [SerializeField] private string idleStateName = "WolfIdle";
    [SerializeField] private string walkStateName = "WolfRun";
    [SerializeField] private string attackStateName = "WolfAttack";
    [SerializeField] private string hitStateName = "WolfHit";
    [SerializeField] private string hitLayerName = "Hit Layer";
    [SerializeField] private string deathStateName = "WolfDie";
    [SerializeField] private float deathAnimationDuration = 1f;

    [Header("Audio")]
    public AudioClip walkingSound;
    public AudioClip attackingSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float walkingSoundVolume = 0.531f;
    [Range(0f, 1f)] public float attackingSoundVolume = 0.531f;
    [Range(0f, 1f)] public float hitSoundVolume = 0.531f;
    [Range(0f, 1f)] public float deathSoundVolume = 0.531f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool useDirectionalAudio = true;
    [SerializeField] private float fullPanDistance = 8f;

    private Rigidbody2D wolfRB;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D wolfCollider;
    private Transform audioListenerTransform;
    private AudioSource oneShotAudioSource;
    private Vector2 startPosition;
    private float actionTimer;
    private float attackTimer;
    private float attackDelayTimer;
    private int direction = 1;
    private bool isMoving;
    private bool isAttacking;
    private bool isWaitingToAttack;
    private bool previousMovingState;
    private bool hasSetAnimationState;
    private bool isDead;
    private int hitLayerIndex = -1;
    private readonly HashSet<int> animatorStateHashes = new HashSet<int>();

    private void Awake()
    {
        wolfRB = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        wolfCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
        currentHealth = maxHealth;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        SetupOneShotAudioSource();

        CacheAnimatorStates();
        hitLayerIndex = animator != null ? animator.GetLayerIndex(hitLayerName) : -1;
        SetupRigidbody();

        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener != null)
        {
            audioListenerTransform = listener.transform;
        }
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

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        UpdateDirectionalAudio();
        attackTimer -= Time.deltaTime;

        if (isAttacking && IsCurrentAnimationFinished(attackStateName))
        {
            isAttacking = false;
            hasSetAnimationState = false;
        }

        if (!isAttacking && attackTimer <= 0f)
        {
            ChickenController chicken = FindChickenInRange();
            if (chicken != null)
            {
                BeginAttackDelay(chicken);
            }
            else
            {
                CancelAttackDelay();
            }
        }
        else if (attackTimer > 0f)
        {
            CancelAttackDelay();
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            StopMoving();
            return;
        }

        if (isAttacking || isWaitingToAttack)
        {
            StopMoving();
            return;
        }

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

        UpdateAnimation();
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

        wolfRB.linearVelocity = new Vector2(direction * moveSpeed, wolfRB.linearVelocity.y);
        // The source wolf artwork faces left, so flip it while moving right.
        spriteRenderer.flipX = direction > 0;
    }

    private void StopMoving()
    {
        wolfRB.linearVelocity = new Vector2(0f, wolfRB.linearVelocity.y);
    }

    private void UpdateAnimation()
    {
        if (!hasSetAnimationState || previousMovingState != isMoving)
        {
            PlayState(isMoving ? walkStateName : idleStateName);
            previousMovingState = isMoving;
            hasSetAnimationState = true;
        }

        if (audioSource == null)
        {
            return;
        }

        if (isMoving && walkingSound != null)
        {
            if (!audioSource.isPlaying || audioSource.clip != walkingSound)
            {
                audioSource.clip = walkingSound;
                audioSource.volume = walkingSoundVolume;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.volume = walkingSoundVolume;
            }
        }
        else if (audioSource.clip == walkingSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.volume = 1f;
        }
    }

    private void StartAttack(ChickenController chicken)
    {
        isWaitingToAttack = false;
        attackDelayTimer = 0f;
        isAttacking = true;
        isMoving = false;
        attackTimer = attackCooldown;
        StopWalkingSound();
        StopMoving();

        direction = chicken.transform.position.x < transform.position.x ? -1 : 1;
        spriteRenderer.flipX = direction > 0;
        PlayState(attackStateName);

        // Damage is applied once per attack. The animation event remains dedicated to sound.
        Vector2 attackDirection = (Vector2)chicken.transform.position - (Vector2)transform.position;
        chicken.TakeDamage(attackDamage, attackDirection);
    }

    private void BeginAttackDelay(ChickenController chicken)
    {
        if (!isWaitingToAttack)
        {
            isWaitingToAttack = true;
            isMoving = false;
            StopWalkingSound();
            StopMoving();
            PlayState(idleStateName);
        }

        direction = chicken.transform.position.x < transform.position.x ? -1 : 1;
        spriteRenderer.flipX = direction > 0;
        attackDelayTimer += Time.deltaTime;

        if (attackDelayTimer >= Mathf.Max(0f, attackDelay))
        {
            StartAttack(chicken);
        }
    }

    private void CancelAttackDelay()
    {
        if (!isWaitingToAttack)
        {
            attackDelayTimer = 0f;
            return;
        }

        isWaitingToAttack = false;
        attackDelayTimer = 0f;
        hasSetAnimationState = false;
        PickNewAction();
    }

    public void PlayAttackSound()
    {
        if (audioSource != null && attackingSound != null)
        {
            PlayOneShot(attackingSound, attackingSoundVolume);
        }
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        SpawnDamageParticles(attackDirection);

        if (currentHealth > 0 && animator != null && hitLayerIndex >= 0)
        {
            animator.Play(hitStateName, hitLayerIndex, 0f);
        }

        if (currentHealth > 0 && audioSource != null && hitSound != null)
        {
            PlayOneShot(hitSound, hitSoundVolume);
        }

        if (currentHealth == 0)
        {
            Die();
        }
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

    private void Die()
    {
        isDead = true;
        StopWalkingSound();
        StopMoving();

        if (wolfCollider != null)
        {
            wolfCollider.enabled = false;
        }

        if (hitLayerIndex >= 0)
        {
            animator.SetLayerWeight(hitLayerIndex, 0f);
        }

        PlayState(deathStateName);

        if (audioSource != null && deathSound != null)
        {
            PlayOneShot(deathSound, deathSoundVolume);
        }

        float destroyDelay = deathSound != null
            ? Mathf.Max(deathAnimationDuration, deathSound.length)
            : deathAnimationDuration;
        Destroy(gameObject, destroyDelay);
    }

    private void UpdateDirectionalAudio()
    {
        if (audioSource == null)
        {
            return;
        }

        if (!useDirectionalAudio || audioListenerTransform == null)
        {
            audioSource.panStereo = 0f;
            if (oneShotAudioSource != null)
            {
                oneShotAudioSource.panStereo = 0f;
            }
            return;
        }

        float safePanDistance = Mathf.Max(0.01f, fullPanDistance);
        float horizontalOffset = transform.position.x - audioListenerTransform.position.x;
        float pan = Mathf.Clamp(horizontalOffset / safePanDistance, -1f, 1f);
        audioSource.panStereo = pan;
        if (oneShotAudioSource != null)
        {
            oneShotAudioSource.panStereo = pan;
        }
    }

    private ChickenController FindChickenInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, GetPlayerLayerMask());

        foreach (Collider2D hit in hits)
        {
            ChickenController chicken = hit.GetComponentInParent<ChickenController>();
            if (chicken != null && chicken.gameObject.activeInHierarchy)
            {
                return chicken;
            }
        }

        return null;
    }

    private int GetPlayerLayerMask()
    {
        if (playerLayer.value != 0)
        {
            return playerLayer;
        }

        int playerLayerIndex = LayerMask.NameToLayer("Player");
        return playerLayerIndex >= 0 ? 1 << playerLayerIndex : Physics2D.DefaultRaycastLayers;
    }

    private void PlayState(string stateName)
    {
        int stateHash = Animator.StringToHash(stateName);
        if (animator != null && animatorStateHashes.Contains(stateHash))
        {
            animator.Play(stateHash, 0, 0f);
        }
    }

    private bool IsCurrentAnimationFinished(string stateName)
    {
        if (animator == null || !animatorStateHashes.Contains(Animator.StringToHash(stateName)))
        {
            return true;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 0.95f;
    }

    private void CacheAnimatorStates()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            animatorStateHashes.Add(Animator.StringToHash(clip.name));
        }
    }

    private void StopWalkingSound()
    {
        if (audioSource != null && audioSource.clip == walkingSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.volume = 1f;
        }
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (oneShotAudioSource == null || clip == null)
        {
            return;
        }

        oneShotAudioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private void SetupOneShotAudioSource()
    {
        oneShotAudioSource = gameObject.AddComponent<AudioSource>();
        oneShotAudioSource.playOnAwake = false;
        oneShotAudioSource.loop = false;
        oneShotAudioSource.volume = 1f;

        if (audioSource == null)
        {
            return;
        }

        oneShotAudioSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
        oneShotAudioSource.pitch = audioSource.pitch;
        oneShotAudioSource.spatialBlend = audioSource.spatialBlend;
        oneShotAudioSource.dopplerLevel = audioSource.dopplerLevel;
        oneShotAudioSource.rolloffMode = audioSource.rolloffMode;
        oneShotAudioSource.minDistance = audioSource.minDistance;
        oneShotAudioSource.maxDistance = audioSource.maxDistance;
    }

    private void SetupRigidbody()
    {
        wolfRB.bodyType = RigidbodyType2D.Dynamic;
        wolfRB.simulated = true;
        wolfRB.gravityScale = 1f;
        wolfRB.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector3.left * maxDistanceFromStart, center + Vector3.right * maxDistanceFromStart);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
