using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls all UI screens for "Klątwa Skarbnika":
/// 1. Menu Screen (Title logo, Start Button, High Score)
/// 2. In-game HUD (Current Score, Timer, High Score, Scarabs caught)
/// 3. Game Over Screen (Egyptian stone portal with score framed in stone tablet, new record badge, restart & menu buttons)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;

    [Header("Menu Elements")]
    public Image titleImage;
    public Button startButton;
    public Text menuHighScoreText;

    [Header("HUD Elements")]
    public Text scoreText;
    public Text timerText;
    public Text highScoreText;
    public Text caughtCountText;

    [Header("Game Over Elements")]
    public Image gameOverPortalImage;
    public Text finalScoreText;
    public Text newRecordBannerText;
    public Text finalHighScoreText;
    public Text statsText;
    public Button restartButton;
    public Button menuButton;

    [Header("Sprites from Projekt")]
    public Sprite titleSprite;        // Nazwa_gra2 ("KLĄTWA SKARBNIKA")
    public Sprite startButtonSprite;  // Rozpocznij_gra2 ("ROZPOCZNIJ")
    public Sprite gameOverSprite;     // Koniec_gra2 ("KONIEC GRY! TWÓJ WYNIK:")

    private Canvas canvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadSpritesIfMissing();
        EnsureUIHierarchyExists();
    }

    private void Start()
    {
        BindButtonEvents();
    }

    public void BindButtonEvents()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuClicked);
        }
    }

    public void ShowMenuPanel(int highScore)
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (menuHighScoreText != null)
        {
            menuHighScoreText.text = $"NAJLEPSZY WYNIK: {highScore}";
        }
    }

    public void ShowHUD()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"PUNKTY: {score}";
        }
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"REKORD: {highScore}";
        }
    }

    public void UpdateStats(int caughtCount)
    {
        if (caughtCountText != null)
        {
            caughtCountText.text = $"ZŁAPANE: {caughtCount}";
        }
    }

    public void UpdateTime(float secondsRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
            int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
            timerText.text = $"CZAS: {minutes:00}:{seconds:00}";

            if (secondsRemaining <= 10f)
            {
                timerText.color = new Color(1f, 0.25f, 0.25f);
            }
            else
            {
                timerText.color = new Color(1f, 0.92f, 0.55f);
            }
        }
    }

    public void ShowGameOverPanel(int score, int highScore, bool isNewRecord, int totalHits)
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text = $"{score}";
        }

        if (finalHighScoreText != null)
        {
            finalHighScoreText.text = $"NAJLEPSZY REKORD: {highScore}";
        }

        if (newRecordBannerText != null)
        {
            newRecordBannerText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                newRecordBannerText.text = "★ NOWY REKORD FARAONA! ★";
            }
        }

        if (statsText != null)
        {
            statsText.text = $"Złapane skarabeusze: {totalHits}";
        }
    }

    public void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void OnMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowMenu();
        }
    }

    /// <summary>
    /// Builds and connects the complete responsive Canvas UI hierarchy with transparent Egyptian sprites.
    /// </summary>
    public void EnsureUIHierarchyExists()
    {
        if (menuPanel != null && hudPanel != null && gameOverPanel != null)
        {
            EnsureEventSystemModule();
            return;
        }

        // 1. Find or create Canvas
#if UNITY_2023_1_OR_NEWER
        canvas = FindAnyObjectByType<Canvas>();
#else
        canvas = FindObjectOfType<Canvas>();
#endif
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Portrait standard
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Ensure modern InputSystem EventSystem
        EnsureEventSystemModule();

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 3. MENU PANEL
        if (menuPanel == null)
        {
            GameObject mPanel = new GameObject("MenuPanel");
            mPanel.transform.SetParent(canvas.transform, false);
            RectTransform mRect = mPanel.AddComponent<RectTransform>();
            mRect.anchorMin = Vector2.zero;
            mRect.anchorMax = Vector2.one;
            mRect.sizeDelta = Vector2.zero;

            // Title Logo (Clean transparent emblem)
            GameObject titleObj = new GameObject("TitleLogo");
            titleObj.transform.SetParent(mPanel.transform, false);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 0.68f);
            tRect.anchorMax = new Vector2(0.5f, 0.68f);
            tRect.pivot = new Vector2(0.5f, 0.5f);
            tRect.sizeDelta = new Vector2(850, 595);

            titleImage = titleObj.AddComponent<Image>();
            titleImage.preserveAspect = true;
            if (titleSprite != null) titleImage.sprite = titleSprite;

            // Start Button (Clean carved stone button)
            GameObject startBtnObj = new GameObject("StartButton");
            startBtnObj.transform.SetParent(mPanel.transform, false);
            RectTransform btnRect = startBtnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.28f);
            btnRect.anchorMax = new Vector2(0.5f, 0.28f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = new Vector2(680, 265);

            Image btnImg = startBtnObj.AddComponent<Image>();
            btnImg.preserveAspect = true;
            if (startButtonSprite != null) btnImg.sprite = startButtonSprite;

            startButton = startBtnObj.AddComponent<Button>();
            ColorBlock cb = startButton.colors;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            startButton.colors = cb;

            // Menu High Score Text
            GameObject hsObj = CreateTextObject("MenuHighScore", mPanel.transform, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), Vector2.zero, new Vector2(800, 70), TextAnchor.MiddleCenter);
            menuHighScoreText = hsObj.GetComponent<Text>();
            menuHighScoreText.font = defaultFont;
            menuHighScoreText.fontSize = 38;
            menuHighScoreText.fontStyle = FontStyle.Bold;
            menuHighScoreText.color = new Color(1f, 0.90f, 0.45f);

            menuPanel = mPanel;
        }

        // 4. HUD PANEL
        if (hudPanel == null)
        {
            GameObject hPanel = new GameObject("HUDPanel");
            hPanel.transform.SetParent(canvas.transform, false);
            RectTransform hRect = hPanel.AddComponent<RectTransform>();
            hRect.anchorMin = Vector2.zero;
            hRect.anchorMax = Vector2.one;
            hRect.sizeDelta = Vector2.zero;

            // Score (Top Left)
            GameObject scoreObj = CreateTextObject("ScoreText", hPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40, -50), new Vector2(500, 80), TextAnchor.UpperLeft);
            scoreText = scoreObj.GetComponent<Text>();
            scoreText.font = defaultFont;
            scoreText.fontSize = 44;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.color = new Color(1f, 0.90f, 0.40f);
            scoreText.text = "PUNKTY: 0";

            // Timer (Top Right)
            GameObject timerObj = CreateTextObject("TimerText", hPanel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40, -50), new Vector2(400, 80), TextAnchor.UpperRight);
            timerText = timerObj.GetComponent<Text>();
            timerText.font = defaultFont;
            timerText.fontSize = 44;
            timerText.fontStyle = FontStyle.Bold;
            timerText.color = new Color(1f, 0.92f, 0.55f);
            timerText.text = "CZAS: 00:45";

            // High Score (Below Score)
            GameObject hsObj = CreateTextObject("HighScoreText", hPanel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40, -125), new Vector2(450, 60), TextAnchor.UpperLeft);
            highScoreText = hsObj.GetComponent<Text>();
            highScoreText.font = defaultFont;
            highScoreText.fontSize = 32;
            highScoreText.color = new Color(0.85f, 0.85f, 0.85f);
            highScoreText.text = "REKORD: 0";

            // Scarabs Caught Count (Below Timer)
            GameObject cntObj = CreateTextObject("CaughtCountText", hPanel.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40, -125), new Vector2(400, 60), TextAnchor.UpperRight);
            caughtCountText = cntObj.GetComponent<Text>();
            caughtCountText.font = defaultFont;
            caughtCountText.fontSize = 32;
            caughtCountText.color = new Color(0.35f, 0.95f, 1f);
            caughtCountText.text = "ZŁAPANE: 0";

            hudPanel = hPanel;
            hudPanel.SetActive(false);
        }

        // 5. GAME OVER PANEL
        if (gameOverPanel == null)
        {
            GameObject goPanel = new GameObject("GameOverPanel");
            goPanel.transform.SetParent(canvas.transform, false);
            RectTransform goRect = goPanel.AddComponent<RectTransform>();
            goRect.anchorMin = Vector2.zero;
            goRect.anchorMax = Vector2.one;
            goRect.sizeDelta = Vector2.zero;

            // Semi-transparent atmospheric dark backdrop
            Image goBg = goPanel.AddComponent<Image>();
            goBg.color = new Color(0f, 0f, 0f, 0.75f);

            // Egyptian Stone Portal Image ("KONIEC GRY! TWÓJ WYNIK:")
            GameObject portalObj = new GameObject("PortalFrame");
            portalObj.transform.SetParent(goPanel.transform, false);
            RectTransform pRect = portalObj.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.58f);
            pRect.anchorMax = new Vector2(0.5f, 0.58f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(900, 605);

            gameOverPortalImage = portalObj.AddComponent<Image>();
            gameOverPortalImage.preserveAspect = true;
            if (gameOverSprite != null) gameOverPortalImage.sprite = gameOverSprite;

            // Score displayed directly inside the portal's stone tablet shelf
            GameObject scoreShelfObj = CreateTextObject("FinalScoreText", portalObj.transform, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(440, 80), TextAnchor.MiddleCenter);
            finalScoreText = scoreShelfObj.GetComponent<Text>();
            finalScoreText.font = defaultFont;
            finalScoreText.fontSize = 54;
            finalScoreText.fontStyle = FontStyle.Bold;
            finalScoreText.color = new Color(0.18f, 0.12f, 0.05f);
            finalScoreText.text = "0";

            // New Record Banner
            GameObject bannerObj = CreateTextObject("NewRecordBanner", goPanel.transform, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(750, 65), TextAnchor.MiddleCenter);
            newRecordBannerText = bannerObj.GetComponent<Text>();
            newRecordBannerText.font = defaultFont;
            newRecordBannerText.fontSize = 40;
            newRecordBannerText.fontStyle = FontStyle.Bold;
            newRecordBannerText.color = new Color(1f, 0.88f, 0.2f);
            newRecordBannerText.text = "★ NOWY REKORD FARAONA! ★";

            // Final High Score & Stats
            GameObject hsFinalObj = CreateTextObject("FinalHighScoreText", goPanel.transform, new Vector2(0.5f, 0.31f), new Vector2(0.5f, 0.31f), Vector2.zero, new Vector2(750, 50), TextAnchor.MiddleCenter);
            finalHighScoreText = hsFinalObj.GetComponent<Text>();
            finalHighScoreText.font = defaultFont;
            finalHighScoreText.fontSize = 32;
            finalHighScoreText.color = new Color(0.9f, 0.9f, 0.9f);
            finalHighScoreText.text = "NAJLEPSZY REKORD: 0";

            GameObject statsObj = CreateTextObject("StatsText", goPanel.transform, new Vector2(0.5f, 0.265f), new Vector2(0.5f, 0.265f), Vector2.zero, new Vector2(750, 50), TextAnchor.MiddleCenter);
            statsText = statsObj.GetComponent<Text>();
            statsText.font = defaultFont;
            statsText.fontSize = 28;
            statsText.color = new Color(0.8f, 0.8f, 0.8f);
            statsText.text = "Złapane skarabeusze: 0";

            // Restart Button (uses the carved stone button sprite)
            GameObject restartBtnObj = new GameObject("RestartButton");
            restartBtnObj.transform.SetParent(goPanel.transform, false);
            RectTransform rBtnRect = restartBtnObj.AddComponent<RectTransform>();
            rBtnRect.anchorMin = new Vector2(0.5f, 0.16f);
            rBtnRect.anchorMax = new Vector2(0.5f, 0.16f);
            rBtnRect.pivot = new Vector2(0.5f, 0.5f);
            rBtnRect.sizeDelta = new Vector2(550, 214);

            Image rBtnImg = restartBtnObj.AddComponent<Image>();
            rBtnImg.preserveAspect = true;
            if (startButtonSprite != null) rBtnImg.sprite = startButtonSprite;

            restartButton = restartBtnObj.AddComponent<Button>();
            ColorBlock rcb = restartButton.colors;
            rcb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            rcb.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            restartButton.colors = rcb;

            // Menu Button (Return to main screen)
            GameObject menuBtnObj = new GameObject("MenuButton");
            menuBtnObj.transform.SetParent(goPanel.transform, false);
            RectTransform mBtnRect = menuBtnObj.AddComponent<RectTransform>();
            mBtnRect.anchorMin = new Vector2(0.5f, 0.05f);
            mBtnRect.anchorMax = new Vector2(0.5f, 0.05f);
            mBtnRect.pivot = new Vector2(0.5f, 0.5f);
            mBtnRect.sizeDelta = new Vector2(360, 60);

            Image mBtnImg = menuBtnObj.AddComponent<Image>();
            mBtnImg.color = new Color(0.2f, 0.16f, 0.12f, 0.85f);

            menuButton = menuBtnObj.AddComponent<Button>();

            GameObject mBtnTxtObj = CreateTextObject("Text", menuBtnObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 60), TextAnchor.MiddleCenter);
            Text mBtnTxt = mBtnTxtObj.GetComponent<Text>();
            mBtnTxt.font = defaultFont;
            mBtnTxt.fontSize = 28;
            mBtnTxt.fontStyle = FontStyle.Bold;
            mBtnTxt.color = new Color(1f, 0.85f, 0.5f);
            mBtnTxt.text = "MENU GŁÓWNE";

            gameOverPanel = goPanel;
            gameOverPanel.SetActive(false);
        }

        BindButtonEvents();
    }

    public static void EnsureEventSystemModule()
    {
#if UNITY_2023_1_OR_NEWER
        var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
#else
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
#endif
        if (es == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        // Remove legacy StandaloneInputModule to avoid InvalidOperationException with New Input System
        var legacyModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (legacyModule != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(legacyModule);
            else
                Destroy(legacyModule);
#else
            Destroy(legacyModule);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        var inputSystemModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputSystemModule == null)
        {
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
#endif
    }

    private GameObject CreateTextObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Text txt = obj.AddComponent<Text>();
        txt.alignment = alignment;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.85f);
        shadow.effectDistance = new Vector2(2.5f, -2.5f);

        return obj;
    }

    private void LoadSpritesIfMissing()
    {
        if (titleSprite == null)
            titleSprite = Resources.Load<Sprite>("Sprites/Nazwa_gra2");
        if (startButtonSprite == null)
            startButtonSprite = Resources.Load<Sprite>("Sprites/Rozpocznij_gra2");
        if (gameOverSprite == null)
            gameOverSprite = Resources.Load<Sprite>("Sprites/Koniec_gra2");
    }
}
