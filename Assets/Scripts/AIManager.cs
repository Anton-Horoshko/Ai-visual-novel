using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using TMPro;

public class AIChat : MonoBehaviour
{
    private string apiKey = "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"; // API
    private string apiURL = "https://openrouter.ai/api/v1/chat/completions";

    private List<Message> chatHistory = new List<Message>();

    void Start()
    {
        chatHistory.Add(new Message
        {
            role = "system",
            content = "Ты рассказчик в интерактивной визуальной новелле. " +
                      "Ты создаёшь живой, атмосферный и эмоциональный рассказ, описываешь окружающий мир, поведение и мысли персонажей. " +
                      "В нужные моменты ты также пишешь диалоги и внутренние монологи от лица персонажей, строго в их характере и с учётом контекста. " +
                      "Игрок это главный герой. Ты никогда не выходишь из повествования, не упоминаешь, что ты ИИ, не говоришь мета-комментариев. " +
                      "Твоя задача — погрузить игрока в мир, описывать всё так, будто это визуальная новелла: Краткие описания сцен(от 1 до 3 абзацев). " +
                      "Эмоциональные реакции героев (без \"он сказал с грустью\", а с более литературным подходом). " +
                      "Диалоги оформлены с именами персонажей и в кавычках. При необходимости — мысли героя(внутренний монолог)." +
                      "Не забывай двигать сюжет, вести диалоги с игроком."
        });
    }

    public List<Message> GetChatHistory()
    {
        return chatHistory;
    }

    public void SetChatHistory(List<Message> history)
    {
        chatHistory = history;
    }

    public void SendMessageToAI(string userInput, System.Action<string> callback)
    {
        chatHistory.Add(new Message { role = "user", content = userInput });

        StartCoroutine(SendRequest(callback));
    }

    private string EscapeJsonString(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in str)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '\"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                default:
                    if (char.IsControl(c))
                    {
                        sb.AppendFormat("\\u{0:X4}", (int)c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private IEnumerator SendRequest(System.Action<string> callback, int attempt = 0)
    {
        const int maxRetries = 5;
        const float retryDelay = 2f;

        if (chatHistory == null)
            chatHistory = new List<Message>();

        string messagesJson = "\"messages\": [";
        foreach (var msg in chatHistory)
        {
            string safeContent = EscapeJsonString(msg.content);
            messagesJson += $"{{\"role\": \"{msg.role}\", \"content\": \"{safeContent}\"}},";
        }
        messagesJson = messagesJson.TrimEnd(',') + "]";

        string jsonBody = $"{{\"model\": \"deepseek/deepseek-chat-v3-0324:free\", {messagesJson}}}";

        using (UnityWebRequest www = new UnityWebRequest(apiURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Authorization", "Bearer " + apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 30;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("Ответ AI: " + responseText);

                try
                {
                    AIResponse response = JsonUtility.FromJson<AIResponse>(responseText);
                    if (response != null && response.choices.Length > 0)
                    {
                        string aiText = response.choices[0].message.content;
                        chatHistory.Add(new Message { role = "assistant", content = aiText });
                        callback(aiText);
                    }
                    else
                    {
                        Debug.LogError("[AIChat] Пустой или некорректный ответ.");
                        callback("Ошибка обработки ответа AI.");
                    }
                }
                catch
                {
                    Debug.LogError("[AIChat] Ошибка при парсинге JSON.");
                    callback("Ошибка при чтении ответа AI.");
                }
            }
            else
            {
                Debug.LogWarning($"[AIChat] Попытка #{attempt + 1} неудачна: {www.error}");

                if (attempt < maxRetries)
                {
                    yield return new WaitForSeconds(retryDelay);
                    StartCoroutine(SendRequest(callback, attempt + 1));
                }
                else
                {
                    callback("Ошибка соединения с AI после нескольких попыток.");
                }
            }
        }
    }
}


    [System.Serializable]
public class AIResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}