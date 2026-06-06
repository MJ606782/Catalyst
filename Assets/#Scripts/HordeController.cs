using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HordeController : MonoBehaviour
{
    [Header("References")]
    public ZombieSpawner spawner;
    public Camera playerCamera;

    [Header("Camera Shake")]
    public float shakeIntensity = 0.15f;
    public float shakeDuration = 0.2f;

    [Header("Kill Streak")]
    public Text streakText;
    public float streakTimeout = 3f;
    public int[] streakThresholds = { 5, 10, 20, 35, 50 };
    public string[] streakMessages = {
        "BLOODBATH!",
        "MASSACRE!",
        "CARNAGE!",
        "APOCALYPSE!",
        "GENOCIDE!"
    };

    [Header("Streak Blood")]
    public int baseBloodParticles = 15;
    public int bonusBloodPerStreak = 5;

    private int streak = 0;
    private float lastKillTime;
    private int previousAliveCount;

    void Start()
    {
        if (spawner == null) spawner = FindObjectOfType<ZombieSpawner>();
        if (playerCamera == null) playerCamera = Camera.main;
        previousAliveCount = spawner != null ? spawner.AliveCount : 0;
        if (streakText != null) streakText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (spawner == null) return;

        int current = spawner.AliveCount;
        int diff = previousAliveCount - current;

        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
                OnZombieKilled();
        }

        previousAliveCount = current;

        if (streak > 0 && Time.time - lastKillTime > streakTimeout)
            ResetStreak();
    }

    void OnZombieKilled()
    {
        streak++;
        lastKillTime = Time.time;

        int extraBlood = Mathf.Min(streak / 2, 25) * bonusBloodPerStreak;
        BloodEffect.SpawnBloodSplatter(
            GetRandomScreenPoint(),
            30 + extraBlood
        );

        if (streak >= 3)
        {
            StartCoroutine(ShakeCamera(shakeIntensity * (1f + streak * 0.05f), shakeDuration));
        }

        for (int i = 0; i < streakThresholds.Length; i++)
        {
            if (streak == streakThresholds[i])
            {
                ShowStreakMessage(streakMessages[i], streak);
                BloodEffect.SpawnDeathGeyser(GetRandomScreenPoint(), 60 + extraBlood);
                StartCoroutine(ShakeCamera(shakeIntensity * 1.5f, shakeDuration * 1.5f));
                break;
            }
        }

        UpdateStreakUI();
    }

    void ShowStreakMessage(string msg, int count)
    {
        if (streakText != null)
        {
            streakText.text = $"{count}x {msg}";
            streakText.gameObject.SetActive(true);
            streakText.color = Color.red;
            streakText.fontSize = Mathf.Min(48 + count, 80);
            StartCoroutine(FadeStreakText());
        }
    }

    IEnumerator FadeStreakText()
    {
        yield return new WaitForSeconds(1.5f);
        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime * 2f;
            if (streakText != null)
            {
                Color c = streakText.color;
                c.a = Mathf.Clamp01(t);
                streakText.color = c;
            }
            yield return null;
        }
        if (streakText != null) streakText.gameObject.SetActive(false);
    }

    void UpdateStreakUI()
    {
        if (streakText != null && streak > 0 && streak < 3)
        {
            streakText.text = $"{streak}x";
            streakText.gameObject.SetActive(true);
        }
    }

    void ResetStreak()
    {
        streak = 0;
        if (streakText != null)
        {
            streakText.gameObject.SetActive(false);
            streakText.color = Color.white;
        }
    }

    Vector3 GetRandomScreenPoint()
    {
        if (playerCamera == null) return Vector3.zero;
        Ray r = playerCamera.ScreenPointToRay(new Vector3(
            Screen.width * Random.Range(0.2f, 0.8f),
            Screen.height * Random.Range(0.2f, 0.8f),
            0
        ));
        return r.origin + r.direction * Random.Range(2f, 8f);
    }

    IEnumerator ShakeCamera(float intensity, float duration)
    {
        if (playerCamera == null) yield break;
        Vector3 orig = playerCamera.transform.localPosition;
        float t = duration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            float decay = t / duration;
            playerCamera.transform.localPosition = orig + Random.insideUnitSphere * intensity * decay;
            yield return null;
        }
        playerCamera.transform.localPosition = orig;
    }
}
