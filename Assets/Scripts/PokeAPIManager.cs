using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PokeAPIManager : MonoBehaviour
{
    private const string BASE_URL = "https://pokeapi.co/api/v2/pokemon/";
    private const int MAX_POKEMON_ID = 151; // Gen 1 únicamente

    public void GetPokemon(int id, Action<PokeData> onSuccess, Action onError)
    {
        StartCoroutine(FetchPokemon(id, onSuccess, onError));
    }

    private IEnumerator FetchPokemon(int id, Action<PokeData> onSuccess, Action onError)
    {
        string url = BASE_URL + id;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching Pokemon {id}: {request.error}");
                onError?.Invoke();
                yield break;
            }

            PokeData data = JsonUtility.FromJson<PokeData>(request.downloadHandler.text);
            onSuccess?.Invoke(data);
        }
    }

    public void GetSprite(string url, Action<Sprite> onSuccess, Action onError)
    {
        StartCoroutine(FetchSprite(url, onSuccess, onError));
    }

    private IEnumerator FetchSprite(string url, Action<Sprite> onSuccess, Action onError)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching sprite: {request.error}");
                onError?.Invoke();
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            onSuccess?.Invoke(sprite);
        }
    }

    public static int GetRandomID() => UnityEngine.Random.Range(1, MAX_POKEMON_ID + 1);
}