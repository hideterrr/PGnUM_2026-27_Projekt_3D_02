using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns scarabs across the upper viewport of the Egyptian tomb.
/// Dynamically scales frequency and difficulty based on score.
/// </summary>
public class ScarabSpawner : MonoBehaviour
{
    public static ScarabSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public float minSpawnInterval = 0.7f;
    public float maxSpawnInterval = 1.25f;
    public float spawnY = 5.8f;
    public float minX = -2.1f;
    public float maxX = 2.1f;

    [Header("Scarab Variations")]
    public float minSpeed = 2.5f;
    public float maxSpeed = 4.0f;
    public float minScale = 0.28f;
    public float maxScale = 0.38f;

    [Header("Chances (%)")]
    [Range(0, 100)] public int classicChance = 70;
    [Range(0, 100)] public int goldenChance = 20; // +250 Bonus
    // Remaining % is Cursed (-100 Penalty)

    [Header("Visual Asset")]
    public Sprite scarabSprite;
    public GameObject scarabPrefab;

    private Coroutine spawnCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartSpawning()
    {
        StopSpawning();
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ClearAllScarabs()
    {
#if UNITY_6000_0_OR_NEWER
        Scarab[] scarabs = FindObjectsByType<Scarab>();
#elif UNITY_2023_1_OR_NEWER
        Scarab[] scarabs = FindObjectsByType<Scarab>(FindObjectsSortMode.None);
#else
        Scarab[] scarabs = FindObjectsOfType<Scarab>();
#endif
        foreach (var s in scarabs)
        {
            if (s != null) Destroy(s.gameObject);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.35f);

        while (true)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
            {
                SpawnSingleScarab();
            }

            // Gently increase tempo as score grows
            float progressFactor = 1f;
            if (GameManager.Instance != null)
            {
                progressFactor = Mathf.Clamp(1f - (GameManager.Instance.CurrentScore / 5000f) * 0.3f, 0.65f, 1f);
            }

            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval) * progressFactor;
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void SpawnSingleScarab()
    {
        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        int roll = Random.Range(0, 100);
        ScarabType type = ScarabType.Classic;
        if (roll < classicChance)
        {
            type = ScarabType.Classic;
        }
        else if (roll < classicChance + goldenChance)
        {
            type = ScarabType.Golden;
        }
        else
        {
            type = ScarabType.Cursed;
        }

        GameObject scarabObj;
        if (scarabPrefab != null)
        {
            scarabObj = Instantiate(scarabPrefab, pos, Quaternion.identity);
        }
        else
        {
            scarabObj = new GameObject($"Scarab_{type}");
            scarabObj.transform.position = pos;

            SpriteRenderer sr = scarabObj.AddComponent<SpriteRenderer>();
            sr.sprite = scarabSprite;
            sr.sortingOrder = 5;

            CircleCollider2D col = scarabObj.AddComponent<CircleCollider2D>();
            col.radius = 2.1f; // proportional to the 500px scarab sprite

            scarabObj.AddComponent<Scarab>();
        }

        float speed = Random.Range(minSpeed, maxSpeed);
        float scale = Random.Range(minScale, maxScale);

        Scarab scarabComp = scarabObj.GetComponent<Scarab>();
        if (scarabComp != null)
        {
            scarabComp.Init(type, scarabSprite, speed, scale);
        }
    }
}
