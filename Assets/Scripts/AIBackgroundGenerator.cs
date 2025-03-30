using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class AIBackgroundGenerator : MonoBehaviour
{
    public Image targetImage; // UI Image в Canvas
    private string apiKey = "sk-9jQd4GBCG8BDk6DEzbqsvLkMrIczmkFRPxgvW4qEO555Glh4";
    private string apiUrl = "https://api.stability.ai/v2beta/stable-image/generate/sd3";

    // Карта контекста промпта
    private Dictionary<string, string> contextPrompts = new Dictionary<string, string>()
    {
        { "forest_day", "A beautiful forest with sunlight and birds" },
        { "forest_night", "A dark and mysterious forest with glowing fireflies" },
        { "city_day", "A bustling cyberpunk city with neon signs and flying cars" },
        { "city_night", "A futuristic city at night, with glowing skyscrapers" },
        { "castle", "A medieval castle on a hill surrounded by mist" },
        { "beach_sunset", "A peaceful beach at sunset with waves and golden sky" }
    };

    private string currentContext = "forest_day"; // Начальный контекст

    void Start()
    {
        UpdateBackground(currentContext);
    }

    public void UpdateBackground(string newContext)
    {
        if (contextPrompts.ContainsKey(newContext))
        {
            currentContext = newContext;
            StartCoroutine(GenerateImageCoroutine(contextPrompts[newContext]));
        }
        else
        {
            Debug.LogError($"Контекст {newContext} не найден!");
        }
    }

    IEnumerator GenerateImageCoroutine(string prompt)
    {
        string boundary = "----UnityBoundary" + System.DateTime.Now.Ticks.ToString("x");
        byte[] formData = CreateMultipartFormData(boundary, prompt);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(formData);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + boundary);
            request.SetRequestHeader("Accept", "image/*");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] imageData = request.downloadHandler.data;
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);

                // Создаём спрайт из текстуры
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Назначаем спрайт в UI Image
                targetImage.sprite = newSprite;

                // Сохраняем изображение
                File.WriteAllBytes(Application.persistentDataPath + "/background.jpg", imageData);
                Debug.Log($"Фон обновлён! ({currentContext})");
            }
            else
            {
                Debug.LogError($"Ошибка генерации: {request.responseCode} - {request.error}");
                Debug.LogError("Ответ сервера: " + request.downloadHandler.text);
            }
        }
    }

    byte[] CreateMultipartFormData(string boundary, string prompt)
    {
        string formData =
            $"--{boundary}\r\n" +
            "Content-Disposition: form-data; name=\"prompt\"\r\n\r\n" +
            $"{prompt}\r\n" +
            $"--{boundary}\r\n" +
            "Content-Disposition: form-data; name=\"output_format\"\r\n\r\n" +
            "jpeg\r\n" +
            $"--{boundary}--\r\n";

        return Encoding.UTF8.GetBytes(formData);
    }
}
