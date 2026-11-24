using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns animated floating text in world space indicating points earned (+100, +250) or penalty (-100).
/// </summary>
public class ScorePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private float moveSpeed = 2.4f;
    private float duration = 0.8f;

    public static void Create(Vector3 worldPos, int amount)
    {
        GameObject popupObj = new GameObject("ScorePopup");
        popupObj.transform.position = worldPos + new Vector3(0, 0.35f, 0);

        ScorePopup popup = popupObj.AddComponent<ScorePopup>();
        popup.Setup(amount);
    }

    public void Setup(int amount)
    {
        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.characterSize = 0.17f;
        textMesh.fontSize = 46;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 25;

        if (amount > 0)
        {
            textMesh.text = $"+{amount}";
            // Golden yellow for bonus, bright cyan-gold for normal
            textMesh.color = (amount >= 200) ? new Color(1f, 0.88f, 0.15f) : new Color(0.35f, 0.95f, 1f);
        }
        else
        {
            textMesh.text = $"{amount}";
            textMesh.color = new Color(1f, 0.25f, 0.25f);
        }

        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Color initialColor = textMesh.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = startPos + new Vector3(0, t * moveSpeed, 0);

            Color c = initialColor;
            c.a = 1f - Mathf.Pow(t, 2f);
            textMesh.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
