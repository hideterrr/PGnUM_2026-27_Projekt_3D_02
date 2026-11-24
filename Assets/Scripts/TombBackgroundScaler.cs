using UnityEngine;

/// <summary>
/// Scales the Egyptian tomb background sprite so it dynamically covers the full viewport
/// across any aspect ratio (standard 9:16 portrait, ultrawide mobile screens 9:20, or tablets).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class TombBackgroundScaler : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera cam;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        FitToScreen();
    }

    private void LateUpdate()
    {
        FitToScreen();
    }

    public void FitToScreen()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        if (cam == null) cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float worldScreenHeight = cam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector2 spriteSize = sr.sprite.rect.size / sr.sprite.pixelsPerUnit;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

        // Cover whole viewport without any black bars (aspect fill / cover)
        float scaleY = worldScreenHeight / spriteSize.y;
        float scaleX = worldScreenWidth / spriteSize.x;
        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
    }
}
