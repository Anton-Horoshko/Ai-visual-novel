using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class AIBackgroundGenerator : MonoBehaviour
{
    public SpriteRenderer backgroundRenderer;
    private string apiKey = "sk-4UaPQDY6pCaifpIWpIvbQSwrNdsUr4sEJ3kaRs0leDrzG4Je"; // Укажи API-ключ
    private string apiURL = "https://api.openai.com/v1/images/generations";

    public void GenerateBackground(string prompt)
    {
        StartCoroutine(SendRequest(prompt));
    }

    private IEnumerator SendRequest(string prompt)
    {
        string jsonBody = "{\"model\": \"dall-e-3\", \"prompt\": \"" + prompt + "\", \"n\": 1, \"size\": \"1024x1024\"}";

        UnityWebRequest www = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;
            AIImageResponse response = JsonUtility.FromJson<AIImageResponse>(responseText);

            if (response != null && response.data.Length > 0)
            {
                StartCoroutine(LoadImage(response.data[0].url));
            }
        }
        else
        {
            Debug.LogError("Ошибка генерации фона: " + www.error);
        }
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            backgroundRenderer.sprite = sprite;
        }
    }
}

[System.Serializable]
public class AIImageResponse
{
    public AIImageData[] data;
}

[System.Serializable]
public class AIImageData
{
    public string url;
}
