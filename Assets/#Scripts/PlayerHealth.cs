using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvincible = false;

    [Header("UI")]
    public Slider healthSlider;
    public Image damageOverlay;
    public float overlayFadeSpeed = 2f;
    public Color overlayColor = new Color(0.6f, 0f, 0f, 0.3f);

    [Header("Blood Effects")]
    public GameObject deathBloodPrefab;
    public bool screenBloodOnHit = true;

    [Header("Audio")]
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    public AudioSource audioSource;

    [Header("Respawn")]
    public float respawnDelay = 3f;
    public Transform respawnPoint;

    private FirstPersonController fpsController;
    private float overlayAlpha = 0f;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        fpsController = GetComponent<FirstPersonController>();

        if (damageOverlay != null)
        {
            Color c = overlayColor;
            c.a = 0f;
            damageOverlay.color = c;
        }

        UpdateHealthUI();
    }

    void Update()
    {
        if (damageOverlay != null && overlayAlpha > 0)
        {
            overlayAlpha = Mathf.Lerp(overlayAlpha, 0f, Time.deltaTime * overlayFadeSpeed);
            Color c = overlayColor;
            c.a = overlayAlpha;
            damageOverlay.color = c;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isInvincible) return;

        currentHealth -= amount;

        if (screenBloodOnHit)
        {
            overlayAlpha = Mathf.Min(1f, overlayAlpha + 0.3f);
        }

        BloodEffect.SpawnBloodSpray(
            transform.position + Vector3.up * Random.Range(0.5f, 1.2f) + Random.insideUnitSphere * 0.3f,
            Random.insideUnitSphere.normalized,
            8
        );

        if (audioSource != null && hurtSounds.Length > 0)
        {
            audioSource.PlayOneShot(hurtSounds[Random.Range(0, hurtSounds.Length)], 0.7f);
        }

        if (fpsController != null)
        {
            StartCoroutine(PlayHitShake());
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    IEnumerator PlayHitShake()
    {
        float elapsed = 0f;
        float duration = 0.15f;
        float intensity = 0.05f;

        Transform cam = GetComponentInChildren<Camera>()?.transform;
        if (cam == null) yield break;

        Vector3 originalPos = cam.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.localPosition = originalPos + Random.insideUnitSphere * intensity * (1f - elapsed / duration);
            yield return null;
        }

        cam.localPosition = originalPos;
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Die()
    {
        isDead = true;

        if (deathBloodPrefab != null)
        {
            Instantiate(deathBloodPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        BloodEffect.SpawnDeathGeyser(transform.position + Vector3.up * 0.5f, 50);
        BloodEffect.SpawnBloodSplatter(transform.position + Vector3.up * 0.8f, 40);

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (fpsController != null)
            fpsController.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        if (damageOverlay != null)
        {
            Color c = overlayColor;
            c.a = 0.8f;
            damageOverlay.color = c;
        }

        yield return new WaitForSeconds(respawnDelay);

        currentHealth = maxHealth;
        isDead = false;

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        if (fpsController != null)
            fpsController.enabled = true;

        if (damageOverlay != null)
        {
            Color c = overlayColor;
            c.a = 0f;
            damageOverlay.color = c;
        }
        overlayAlpha = 0f;

        UpdateHealthUI();
    }
}
