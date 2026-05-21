using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panel Juego")]
    public Image imgPokemon;
    public Button[] answerButtons;

    [Header("Panel Resultado")]
    public GameObject panelResult;
    public TextMeshProUGUI txtResult;
    public TextMeshProUGUI txtReveal;

    [Header("Colores feedback")]
    public Color colorCorrect = Color.green;
    public Color colorWrong = Color.red;
    public Color colorDefault = Color.white;

    [Header("Tiempo entre rondas (segundos)")]
    public float resultDisplayTime = 2f;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowRound(Sprite sprite, PokeData[] options, int correctIndex)
    {
        // Mostrar sprite en silueta (negro)
        imgPokemon.sprite = sprite;
        imgPokemon.color = Color.black;

        // Desactivar panel resultado por si estaba activo
        panelResult.SetActive(false);

        // Configurar botones
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capture = i;
            answerButtons[i].interactable = true;
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                CapitalizeFirst(options[i].name);
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => GameManager.Instance.OnAnswerSelected(capture));

            // Reset color
            answerButtons[i].GetComponent<Image>().color = colorDefault;
        }
    }

    public void ShowResult(bool correct, string pokemonName)
    {
        // Bloquear botones
        foreach (var btn in answerButtons)
            btn.interactable = false;

        // Revelar sprite
        imgPokemon.color = Color.white;

        // Mostrar panel resultado
        panelResult.SetActive(true);
        txtResult.text = correct ? "¡Correcto!" : "Incorrecto";
        txtResult.color = correct ? colorCorrect : colorWrong;
        txtReveal.text = CapitalizeFirst(pokemonName);

        StartCoroutine(NextRoundOrEnd());
    }

    private IEnumerator NextRoundOrEnd()
    {
        yield return new WaitForSeconds(resultDisplayTime);

        if (GameManager.Instance.IsLastRound())
            ShowScore();
        else
            GameManager.Instance.StartNewRound();
    }

    private void ShowScore()
    {
        // Lo completamos en el Commit 6
        Debug.Log($"Partida terminada. Puntuación: {GameManager.Instance.GetScore()}/{GameManager.Instance.GetTotalRounds()}");
    }

    private string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}