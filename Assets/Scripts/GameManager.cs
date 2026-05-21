using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Configuración")]
    public int totalRounds = 10;

    [Header("Referencias")]
    public PokeAPIManager pokeAPIManager;

    // Estado de la partida
    private int currentRound = 0;
    private int score = 0;
    private int correctAnswerIndex;
    private PokeData currentPokemon;

    // Datos de la ronda actual
    private PokeData[] roundOptions = new PokeData[4];
    private int pendingFetches;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        currentRound++;
        roundOptions = new PokeData[4];
        pendingFetches = 4;
        correctAnswerIndex = Random.Range(0, 4);

        int correctID = PokeAPIManager.GetRandomID();
        List<int> usedIDs = new List<int> { correctID };

        // Lanzar fetch del Pokémon correcto
        pokeAPIManager.GetPokemon(correctID, (data) =>
        {
            roundOptions[correctAnswerIndex] = data;
            currentPokemon = data;
            OnFetchComplete();
        },
        () => Debug.LogError("Error cargando Pokémon correcto"));

        // Lanzar fetch de los 3 decoys
        int decoySlot = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i == correctAnswerIndex) continue;

            int decoyID;
            do { decoyID = PokeAPIManager.GetRandomID(); }
            while (usedIDs.Contains(decoyID));
            usedIDs.Add(decoyID);

            int slotCapture = i; // captura para la lambda
            pokeAPIManager.GetPokemon(decoyID, (data) =>
            {
                roundOptions[slotCapture] = data;
                OnFetchComplete();
            },
            () => Debug.LogError("Error cargando decoy"));

            decoySlot++;
        }
    }

    private void OnFetchComplete()
    {
        pendingFetches--;
        if (pendingFetches > 0) return;

        // Todos los datos listos — cargar sprite y actualizar UI
        pokeAPIManager.GetSprite(currentPokemon.sprites.front_default, (sprite) =>
        {
            UIManager.Instance.ShowRound(sprite, roundOptions, correctAnswerIndex);
        },
        () => Debug.LogError("Error cargando sprite"));
    }

    public void OnAnswerSelected(int index)
    {
        bool correct = index == correctAnswerIndex;
        if (correct) score++;

        UIManager.Instance.ShowResult(correct, currentPokemon.name);
    }

    public bool IsLastRound() => currentRound >= totalRounds;

    public int GetScore() => score;
    public int GetTotalRounds() => totalRounds;
}