using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeHUD : MonoBehaviour
{
    [SerializeField] private LifeTilemapNoPreview engine;

    [SerializeField] private Button playBtn;
    [SerializeField] private Button randomBtn;
    [SerializeField] private Button clearBtn;
    [SerializeField] private Button stepBtn;
    [SerializeField] private Button changePlayerBtn;
    

    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text speedValue;

    [SerializeField] private TMP_Text scoreText;

    private int currentPlayer = 1;

    void Start()
    {

        speedSlider.minValue = engine.MinTick;
        speedSlider.maxValue = engine.MaxTick;
        speedSlider.value = engine.TickSeconds;
        if (speedValue) speedValue.text = ToMs(engine.TickSeconds);
        speedSlider.onValueChanged.AddListener(v => {
            engine.TickSeconds = v;
            if (speedValue) speedValue.text = ToMs(v);
        });

        
        playBtn.onClick.AddListener(TogglePlay);
        randomBtn.onClick.AddListener(() => { engine.DoRandomize(); RefreshScore(); });
        clearBtn.onClick.AddListener(() => { engine.DoClear(); RefreshScore(); });
        if (stepBtn) stepBtn.onClick.AddListener(() => { engine.DoStep(); RefreshScore(); });
        if (changePlayerBtn) changePlayerBtn.onClick.AddListener(ChangePlayer);


        InvokeRepeating(nameof(RefreshScore), 0.1f, 0.1f);

        UpdatePlayLabel();
        UpdateChangePlayerLabel();
    }

    private void TogglePlay()
    {
        if (engine.IsRunning) engine.Pause();
        else engine.Play();

        UpdatePlayLabel();
    }

    private void UpdatePlayLabel()
    {
        var t = playBtn.GetComponentInChildren<TMP_Text>();
        if (t) t.text = engine.IsRunning ? "Pause" : "Play";
    }

    private void RefreshScore()
    {
        if (!scoreText || engine == null) return;

        var (t1, s1, t2, s2) = engine.GetStats();

        bool showTokens = !engine.IsRunning;

        string tokens1 = showTokens ? $"(T:{t1})" : "";
        string tokens2 = showTokens ? $"(T:{t2})" : "";

        scoreText.text = $"P1 {s1} {tokens1}  |  P2 {s2} {tokens2}";
    }

    private void ChangePlayer()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        engine.SelectPlayer(currentPlayer);
        UpdateChangePlayerLabel();
    }

    private void UpdateChangePlayerLabel()
    {
        if (!changePlayerBtn) return;
        var t = changePlayerBtn.GetComponentInChildren<TMP_Text>();
        if (t) t.text = $"Player {currentPlayer}";
    }

    private string ToMs(float s) => $"{Mathf.RoundToInt(s * 1000f)} ms";
}
