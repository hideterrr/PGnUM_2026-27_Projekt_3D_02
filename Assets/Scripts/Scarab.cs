using System.Collections;
using UnityEngine;

public enum ScarabType
{
    Classic,  // Standard blue/gold scarab (+100 pts)
    Golden,   // Solar bonus scarab (+250 pts, faster, gleaming gold)
    Cursed    // Cursed penalty scarab (-100 pts, crimson/shadow glow)
}

/// <summary>
/// Controls an individual falling Egyptian scarab.
/// Handles downward flutter movement, tap/click detection, scoring, and hit animations.
/// </summary>
public class Scarab : MonoBehaviour
{
    [Header("Scarab Attributes")]
    public ScarabType scarabType = ScarabType.Classic;
    public int scoreValue = 100;
    public float fallSpeed = 3.2f;
    public float wobbleFrequency = 4.0f;
    public float wobbleAmplitude = 0.55f;
    public float deathY = -6.2f;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private bool isHit = false;
    private float startX;
    private float timeOffset;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        timeOffset = Random.Range(0f, 10f);
        startX = transform.position.x;
    }

    public void Init(ScarabType type, Sprite sprite, float speed, float scale)
    {
        scarabType = type;
        fallSpeed = speed;
        transform.localScale = Vector3.one * scale;
        startX = transform.position.x;

        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        switch (scarabType)
        {
            case ScarabType.Classic:
                scoreValue = 100;
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
                break;

            case ScarabType.Golden:
                scoreValue = 250;
                fallSpeed *= 1.35f;
                wobbleFrequency *= 1.3f;
                wobbleAmplitude *= 1.2f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.95f, 0.40f, 1f);
                break;

            case ScarabType.Cursed:
                scoreValue = -100;
                fallSpeed *= 0.95f;
                if (spriteRenderer != null) spriteRenderer.color = new Color(0.95f, 0.35f, 0.45f, 1f);
                break;
        }
    }

    private void Update()
    {
        if (isHit) return;

        // Fluttering downward motion (sinusoidal wing flutter)
        float newY = transform.position.y - fallSpeed * Time.deltaTime;
        float wobbleX = startX + Mathf.Sin((Time.time + timeOffset) * wobbleFrequency) * wobbleAmplitude;
        transform.position = new Vector3(wobbleX, newY, 0f);

        // Despawn once fallen past bottom of tomb
        if (transform.position.y < deathY)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
            {
                GameManager.Instance.OnScarabMissed(this);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Invoked when the player taps or clicks this scarab.
    /// </summary>
    public void Hit()
    {
        if (isHit) return;
        isHit = true;

        if (circleCollider != null) circleCollider.enabled = false;

        if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            GameManager.Instance.OnScarabHit(this, transform.position);
        }

        StartCoroutine(HitFeedbackAndDestroy());
    }

    private void OnMouseDown()
    {
        Hit();
    }

    private IEnumerator HitFeedbackAndDestroy()
    {
        Vector3 origScale = transform.localScale;
        float elapsed = 0f;
        float duration = 0.16f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Pop slightly larger, then shrink to zero
            float s = (1f - t) * 1.3f;
            transform.localScale = origScale * s;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
