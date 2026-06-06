using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Target
{
    [Header("Movement")]
    public float walkSpeed = 1.8f;
    public float rushSpeed = 4f;
    public float detectionRange = 60f;
    public float attackRange = 2.5f;
    public float rotationSpeed = 3f;
    public float rushDistance = 10f;

    [Header("Combat")]
    public float attackDamage = 20f;
    public float attackRate = 0.9f;

    [Header("Hit Reaction")]
    public float staggerDuration = 0.4f;
    public float knockbackForce = 40f;
    public float stunDuration = 0.8f;

    [Header("Death")]
    public float destroyAfterDelay = 6f;

    [Header("Blood")]
    public int sprayPerHit = 20;
    public int splatterOnDeath = 60;
    public int geyserOnDeath = 50;

    [Header("Audio")]
    public AudioClip[] hurtSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] ambientMoans;
    public float ambientMoanInterval = 6f;
    public float soundVolume = 1f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Collider mainCollider;

    private float nextAttackTime;
    private float nextMoanTime;
    private bool dead = false;

    private float staggerTimer = 0f;
    private float stunTimer = 0f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogWarning("[Zombie] No Player found! Tag your FPSController as 'Player'.");

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        mainCollider = GetComponent<Collider>();
        nextMoanTime = Time.time + Random.Range(2f, ambientMoanInterval);

        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.acceleration = 6f;
        }

        Renderer[] rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
            Debug.LogError("[Zombie] No Renderers found! The FBX model has no visible mesh.");
        if (transform.localScale.magnitude < 0.01f)
            Debug.LogError("[Zombie] Scale is nearly zero! Set scale to 1,1,1.");
    }

    void Update()
    {
        if (dead || player == null) return;

        if (staggerTimer > 0f)
        {
            staggerTimer -= Time.deltaTime;
            stunTimer = stunDuration;
        }
        else if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return;

        bool inRange = dist <= attackRange;
        bool stunned = stunTimer > 0f;

        if (!stunned && !inRange)
        {
            bool rushing = dist <= rushDistance;
            float speed = rushing ? rushSpeed : walkSpeed;
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.speed = speed;
                if (agent.isOnNavMesh)
                    agent.SetDestination(player.position);
                else
                    SimpleMove(player.position, speed);
            }
            else
            {
                SimpleMove(player.position, speed);
            }

            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
        }
        else if (!stunned && inRange)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
                agent.ResetPath();

            Vector3 faceDir = (player.position - transform.position).normalized;
            faceDir.y = 0;
            if (faceDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(faceDir), Time.deltaTime * rotationSpeed * 2f);
        }

        if (inRange && Time.time >= nextAttackTime && !stunned)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + 1f / attackRate;
        }

        if (ambientMoans.Length > 0 && Time.time >= nextMoanTime)
        {
            audioSource?.PlayOneShot(ambientMoans[Random.Range(0, ambientMoans.Length)], soundVolume * 0.4f);
            nextMoanTime = Time.time + Random.Range(ambientMoanInterval * 0.5f, ambientMoanInterval * 1.5f);
        }

        if (animator != null)
        {
            float s = agent != null ? agent.velocity.magnitude / walkSpeed : 0f;
            if (stunTimer > 0f) s *= 0.3f;
            animator.SetFloat(SpeedHash, Mathf.Clamp01(s));
        }
    }

    void SimpleMove(Vector3 target, float speed)
    {
        Vector3 d = (target - transform.position).normalized;
        d.y = 0;
        transform.position += d * speed * Time.deltaTime;
    }

    IEnumerator AttackRoutine()
    {
        animator?.SetTrigger(AttackHash);
        if (audioSource != null && attackSounds.Length > 0)
            audioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)], soundVolume);
        yield return new WaitForSeconds(0.35f);
        if (dead) yield break;
        float dist = Vector3.Distance(transform.position, player.position);
        if (player != null && dist <= attackRange + 0.5f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(attackDamage);
            BloodEffect.SpawnBloodSpray(player.position + Vector3.up * 0.8f + Random.insideUnitSphere * 0.2f, -transform.forward, 15);
        }
    }

    public override void OnHit(Vector3 point, Vector3 normal, float amount)
    {
        if (dead) return;

        staggerTimer = staggerDuration;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.ResetPath();

        BloodEffect.SpawnBloodSpray(point, normal, sprayPerHit);
        BloodEffect.SpawnBloodSpray(point, -normal, sprayPerHit / 2);
        if (Random.value < 0.3f)
            BloodEffect.SpawnBloodSplatter(point, 15);

        animator?.SetTrigger(HitHash);
        if (audioSource != null && hurtSounds.Length > 0)
            audioSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)], soundVolume);

        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        if (dead) return;
        dead = true;
        isDead = true;

        if (audioSource != null && deathSounds.Length > 0)
            audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)], soundVolume);

        BloodEffect.SpawnBloodSplatter(transform.position + Vector3.up * 0.8f, splatterOnDeath);
        BloodEffect.SpawnDeathGeyser(transform.position + Vector3.up * 0.5f, geyserOnDeath);

        for (int i = 0; i < 3; i++)
            BloodEffect.SpawnGroundBloodPool(transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)));

        if (agent != null) agent.enabled = false;
        if (animator != null) animator.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 30f;
        rb.drag = 2f;
        rb.angularDrag = 2f;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        Vector3 dir = (transform.position - player.position).normalized;
        rb.AddForce(dir * 40f + Vector3.down * 30f, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(-30f, 30f), 0, Random.Range(-30f, 30f)), ForceMode.Impulse);

        Destroy(gameObject, destroyAfterDelay);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rushDistance);
    }
}
