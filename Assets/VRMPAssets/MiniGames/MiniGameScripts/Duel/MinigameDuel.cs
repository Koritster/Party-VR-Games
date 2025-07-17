using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using XRMultiplayer;

public class MiniGameDuel : NetworkBehaviour
{
    // Puntajes sincronizados
    private NetworkVariable<int> localPlayerScore = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> enemyPlayerScore = new(writePerm: NetworkVariableWritePermission.Owner);

    [Header("UI")]
    public Text localScoreText;
    public Text enemyScoreText;
    public GameObject gameOverPanel;
    public Text gameOverText;

    [Header("Configuración")]
    [SerializeField] private int maxScoreToWin = 10;

    public override void OnNetworkSpawn()
    {
        localPlayerScore.OnValueChanged += (_, _) => UpdateScoreUI();
        enemyPlayerScore.OnValueChanged += (_, _) => UpdateScoreUI();

        UpdateScoreUI(); // Mostrar valores iniciales
    }

    private void UpdateScoreUI()
    {
        if (localScoreText != null)
            localScoreText.text = "Tú: " + localPlayerScore.Value;

        if (enemyScoreText != null)
            enemyScoreText.text = "Enemigo: " + enemyPlayerScore.Value;
    }

    public void localPlayerHitTarget(int points)
    {
        if (!IsOwner) return;

        localPlayerScore.Value += points;
        CheckGameEnd();
    }

    public void enemyPlayerHitTarget(int points)
    {
        if (!IsOwner) return;

        enemyPlayerScore.Value += points;
        CheckGameEnd();
    }

    private void CheckGameEnd()
    {
        if (localPlayerScore.Value >= maxScoreToWin)
        {
            ShowGameOver("¡Ganaste!");
        }
        else if (enemyPlayerScore.Value >= maxScoreToWin)
        {
            ShowGameOver("¡Perdiste!");
        }
    }

    private void ShowGameOver(string message)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = message;

        // Aquí puedes desactivar controles si lo deseas
    }
}
