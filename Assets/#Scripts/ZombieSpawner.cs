using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Prefab")]
    public GameObject zombiePrefab;

    [Header("Spawning")]
    public float minSpawnDist = 30f;
    public float maxSpawnDist = 60f;
    public int maxAlive = 50;
    public int spawnBatchSize = 5;
    public float spawnInterval = 0.25f;
    public bool debugMode = true;

    [Header("Wave")]
    public int startWave = 1;
    public float timeBetweenWaves = 15f;
    public int baseZombiesPerWave = 5;
    public AnimationCurve waveCountCurve = AnimationCurve.Linear(0, 1, 15, 12);
    public float baseHealth = 50f;

    [Header("Rush Phase")]
    public bool enableRush = true;
    public float rushTriggerPercent = 0.4f;
    public int rushCount = 5;
    public float rushSpawnInterval = 0.1f;
    public float rushSpeedMultiplier = 1.8f;

    [Header("UI")]
    public Text waveText;
    public Text aliveText;
    public Text countdownText;
    public Text killsText;
    public GameObject announceObj;
    public Text announceText;
    public float announceDuration = 3f;
    public Image screenFlash;

    [Header("Audio")]
    public AudioClip waveStartSound;
    public AudioClip rushSound;
    public AudioSource musicSource;

    private Transform player;
    private int currentWave;
    private int aliveCount;
    public int AliveCount => aliveCount;
    private int totalKills;
    public int TotalKills => totalKills;
    private List<GameObject> activeZombies = new List<GameObject>();
    private bool navMeshAvailable;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("[ZombieSpawner] No GameObject tagged 'Player' found! Tag your FPSController as 'Player'.");
            enabled = false;
            return;
        }

        if (zombiePrefab == null)
        {
            Debug.LogError("[ZombieSpawner] zombiePrefab is not assigned! Run Zombies → Create Zombie Prefab or assign it manually.");
            enabled = false;
            return;
        }

        navMeshAvailable = NavMesh.SamplePosition(player.position, out NavMeshHit _, 1f, NavMesh.AllAreas);
        Debug.Log($"[ZombieSpawner] NavMesh baked: {navMeshAvailable}");

        currentWave = startWave - 1;
        if (announceObj != null) announceObj.SetActive(false);
        if (killsText != null) killsText.text = "KILLS: 0";
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWave++;
            Debug.Log($"[ZombieSpawner] Wave {currentWave} starting!");
            if (waveText != null) waveText.text = $"WAVE {currentWave}";
            yield return Announce($"WAVE {currentWave}");
            if (waveStartSound != null && musicSource != null)
                musicSource.PlayOneShot(waveStartSound);
            yield return RunWave(currentWave);
            if (aliveText != null) aliveText.text = "0";
            yield return Countdown();
        }
    }

    IEnumerator RunWave(int waveNum)
    {
        int totalZombies = GetZombieCount(waveNum);
        Debug.Log($"[ZombieSpawner] Wave {waveNum} spawning {totalZombies} zombies");
        int spawned = 0;
        aliveCount = 0;
        float healthMult = 1f + (waveNum - 1) * 0.15f;
        float speedMult = 1f + Mathf.Min((waveNum - 1) * 0.04f, 0.4f);
        bool rushTriggered = false;

        while (spawned < totalZombies || aliveCount > 0)
        {
            CleanList();

            if (!rushTriggered && enableRush && aliveCount > 0 &&
                (float)aliveCount / totalZombies <= rushTriggerPercent)
            {
                rushTriggered = true;
                Debug.Log("[ZombieSpawner] RUSH PHASE triggered!");
                StartCoroutine(RushSpawn(rushCount, healthMult, speedMult * rushSpeedMultiplier));
            }

            if (spawned < totalZombies && aliveCount < maxAlive)
            {
                int batch = Mathf.Min(spawnBatchSize, totalZombies - spawned, maxAlive - aliveCount);
                float angleStep = 360f / batch;
                float baseAngle = Random.Range(0f, 360f);

                for (int i = 0; i < batch; i++)
                {
                    float angle = baseAngle + i * angleStep + Random.Range(-15f, 15f);
                    SpawnZombie(angle, healthMult, speedMult);
                    spawned++;
                }

                if (aliveText != null) aliveText.text = aliveCount.ToString();
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RushSpawn(int count, float healthMult, float speedMult)
    {
        if (rushSound != null && musicSource != null)
            musicSource.PlayOneShot(rushSound);

        if (screenFlash != null)
        {
            screenFlash.color = new Color(0.6f, 0, 0, 0.3f);
            StartCoroutine(FadeFlash());
        }

        for (int i = 0; i < count && aliveCount < maxAlive; i++)
        {
            float angle = Random.Range(0f, 360f);
            SpawnZombie(angle, healthMult, speedMult);
            yield return new WaitForSeconds(rushSpawnInterval);
        }
    }

    void SpawnZombie(float angle, float healthMult, float speedMult)
    {
        if (player == null) return;

        Vector3 pos = GetSpawnPos(angle);
        if (pos == Vector3.zero)
        {
            Debug.LogWarning("[ZombieSpawner] Could not find valid spawn position, skipping");
            return;
        }

        if (debugMode)
            Debug.Log($"[ZombieSpawner] Spawning zombie at {pos}, dist from player: {Vector3.Distance(pos, player.position):F1}m");

        GameObject obj = Instantiate(zombiePrefab, pos, Quaternion.identity);
        activeZombies.Add(obj);
        aliveCount++;

        Zombie z = obj.GetComponent<Zombie>();
        if (z != null)
        {
            z.health = baseHealth * healthMult;
            z.walkSpeed *= speedMult;
            z.rushSpeed *= speedMult;
        }
        else
        {
            Debug.LogWarning("[ZombieSpawner] Spawned object has no Zombie component! Make sure the prefab has Zombie.cs attached.");
            Target t = obj.GetComponent<Target>();
            if (t != null) t.health = baseHealth * healthMult;
        }

        BloodEffect.SpawnBloodSplatter(pos, 12);
    }

    Vector3 GetSpawnPos(float angleDeg)
    {
        float dist = Random.Range(minSpawnDist, maxSpawnDist);
        Vector3 dir = new Vector3(Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0, Mathf.Cos(angleDeg * Mathf.Deg2Rad));
        Vector3 candidate = player.position + dir * dist;

        Vector3 groundPos = ProjectToGround(candidate);
        if (groundPos != Vector3.zero)
            return groundPos;

        candidate = player.position + dir * minSpawnDist;
        groundPos = ProjectToGround(candidate);
        if (groundPos != Vector3.zero)
            return groundPos;

        return player.position + dir * minSpawnDist;
    }

    Vector3 ProjectToGround(Vector3 pos)
    {
        if (navMeshAvailable)
        {
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                return hit.position;
        }

        RaycastHit hitInfo;
        if (Physics.Raycast(new Vector3(pos.x, pos.y + 50f, pos.z), Vector3.down, out hitInfo, 200f))
        {
            Vector3 result = hitInfo.point;
            result.y += 0.05f;
            return result;
        }

        return pos;
    }

    IEnumerator FadeFlash()
    {
        float t = 0.4f;
        while (t > 0)
        {
            t -= Time.deltaTime * 2f;
            if (screenFlash != null)
            {
                Color c = screenFlash.color;
                c.a = t * 0.3f;
                screenFlash.color = c;
            }
            yield return null;
        }
        if (screenFlash != null)
            screenFlash.color = Color.clear;
    }

    IEnumerator Countdown()
    {
        float t = timeBetweenWaves;
        while (t > 0)
        {
            if (countdownText != null)
                countdownText.text = $"NEXT WAVE: {t:F0}s";
            yield return new WaitForSeconds(1f);
            t--;
        }
        if (countdownText != null) countdownText.text = "";
    }

    IEnumerator Announce(string text)
    {
        if (announceObj != null && announceText != null)
        {
            announceText.text = text;
            announceObj.SetActive(true);
            yield return new WaitForSeconds(announceDuration);
            announceObj.SetActive(false);
        }
    }

    void CleanList()
    {
        activeZombies.RemoveAll(z => z == null);
        int count = 0;
        foreach (var z in activeZombies)
        {
            if (z != null)
            {
                Target t = z.GetComponent<Target>();
                if (t != null && !t.isDead) count++;
            }
        }

        int newlyDead = aliveCount - count;
        if (newlyDead > 0)
        {
            totalKills += newlyDead;
            if (killsText != null) killsText.text = $"KILLS: {totalKills}";
        }

        aliveCount = count;
    }

    int GetZombieCount(int wave)
    {
        return Mathf.RoundToInt(baseZombiesPerWave * waveCountCurve.Evaluate(wave - 1));
    }

    void OnDrawGizmosSelected()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        Gizmos.color = new Color(1, 0, 0, 0.08f);
        Gizmos.DrawWireSphere(player.position, maxSpawnDist);
        Gizmos.color = new Color(0, 1, 0, 0.12f);
        Gizmos.DrawWireSphere(player.position, minSpawnDist);

        if (debugMode && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (var z in activeZombies)
            {
                if (z != null)
                    Gizmos.DrawWireSphere(z.transform.position, 0.5f);
            }
        }
    }
}
