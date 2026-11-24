using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum GameState
{
    Menu,
    Playing,
    GameOver
}

/// <summary>
/// Master Game Controller for "Klątwa Skarbnika".
/// Coordinates game loop, countdown timer, scoring, player input (touch & mouse),
/// and persistence of top scores (PlayerPrefs).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Mode Configuration")]
    [Tooltip("Total round duration in seconds.")]
    public float gameDuration = 45.0f;

    [Header("Current State")]
    public GameState CurrentState { get; private set; } = GameState.Menu;
    public int CurrentScore { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;
    public float TimeRemaining { get; private set; }
    public bool IsGameActive => CurrentState == GameState.Playing;

    [Header("Statistics")]
    public int TotalCaught = 0;
    public int BonusCaught = 0;
    public int CursedHit = 0;
    public int Missed = 0;

    private const string HIGH_SCORE_KEY = "KlatwaSkarbnika_HighScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void Start()
    {
        ShowMenu();
    }

    public void ShowMenu()
    {
        CurrentState = GameState.Menu;
        if (ScarabSpawner.Instance != null)
        {
            ScarabSpawner.Instance.StopSpawning();
            ScarabSpawner.Instance.ClearAllScarabs();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMenuPanel(HighScore);
        }
    }

    public void StartGame()
    {
        CurrentScore = 0;
        TotalCaught = 0;
        BonusCaught = 0;
        CursedHit = 0;
        Missed = 0;
        TimeRemaining = gameDuration;
        CurrentState = GameState.Playing;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowHUD();
            UIManager.Instance.UpdateScore(CurrentScore);
            UIManager.Instance.UpdateHighScore(HighScore);
            UIManager.Instance.UpdateTime(TimeRemaining);
            UIManager.Instance.UpdateStats(TotalCaught);
        }

        if (ScarabSpawner.Instance != null)
        {
            ScarabSpawner.Instance.ClearAllScarabs();
            ScarabSpawner.Instance.StartSpawning();
        }
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndGame();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTime(TimeRemaining);
            }

            ProcessTouchAndClickInput();
        }
    }

    private void ProcessTouchAndClickInput()
    {
        // Ignore world clicks when interacting with UI buttons
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        bool tapped = false;
        Vector2 screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        try
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapped = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapped = true;
                screenPos = Mouse.current.position.ReadValue();
            }
        }
        catch
        {
            // Fallback gracefully if InputSystem package device is temporarily unavailable
        }
#endif

        if (!tapped)
        {
            try
            {
                if (Input.GetMouseButtonDown(0))
                {
                    tapped = true;
                    screenPos = Input.mousePosition;
                }
                else if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
                {
                    tapped = true;
                    screenPos = Input.GetTouch(0).position;
                }
            }
            catch
            {
                // Silently ignore if legacy input is restricted
            }
        }

        if (tapped)
        {
            PerformRaycastHit(screenPos);
        }
    }

    private void PerformRaycastHit(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPoint = cam.ScreenToWorldPoint(screenPos);
        Vector2 point2D = new Vector2(worldPoint.x, worldPoint.y);

        Collider2D col = Physics2D.OverlapPoint(point2D);
        if (col != null)
        {
            Scarab scarab = col.GetComponent<Scarab>();
            if (scarab != null)
            {
                scarab.Hit();
            }
        }
    }

    public void OnScarabHit(Scarab scarab, Vector3 hitPos)
    {
        if (!IsGameActive) return;

        TotalCaught++;
        if (scarab.scarabType == ScarabType.Golden) BonusCaught++;
        if (scarab.scarabType == ScarabType.Cursed) CursedHit++;

        CurrentScore = Mathf.Max(0, CurrentScore + scarab.scoreValue);

        ScorePopup.Create(hitPos, scarab.scoreValue);

        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(CurrentScore);
            UIManager.Instance.UpdateHighScore(HighScore);
            UIManager.Instance.UpdateStats(TotalCaught);
        }
    }

    public void OnScarabMissed(Scarab scarab)
    {
        if (!IsGameActive) return;
        Missed++;
    }

    public void EndGame()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;

        if (ScarabSpawner.Instance != null)
        {
            ScarabSpawner.Instance.StopSpawning();
        }

        bool isNewRecord = false;
        int savedHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (CurrentScore > savedHighScore)
        {
            isNewRecord = true;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, CurrentScore);
            PlayerPrefs.Save();
            HighScore = CurrentScore;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverPanel(CurrentScore, HighScore, isNewRecord, TotalCaught);
        }
    }

    public void RestartGame()
    {
        StartGame();
    }
}
