using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AIChat : MonoBehaviour
{
    private string apiKey = "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"; // �������� �� API-����
    private string apiURL = "https://openrouter.ai/api/v1/chat/completions";

    // ����� ��� �������� ��������� AI
    public void SendMessageToAI(string userInput, System.Action<string> callback)
    {
        StartCoroutine(SendRequest(userInput, callback));
    }

    private IEnumerator SendRequest(string userInput, System.Action<string> callback)
    {
        string jsonBody = "{\"model\": \"deepseek/deepseek-chat-v3-0324:free\", \"messages\": [{\"role\": \"user\", \"content\": \"" + userInput + "\"}]}";

        UnityWebRequest www = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();

        // ������������� ���������
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;
            Debug.Log("����� AI: " + responseText);

            // �������������� JSON-������
            AIResponse response = JsonUtility.FromJson<AIResponse>(responseText);
            if (response != null && response.choices.Length > 0)
            {
                callback(response.choices[0].message.content);
            }
            else
            {
                callback("������ ��������� ������ AI.");
            }
        }
        else
        {
            Debug.LogError("������ API: " + www.error);
            callback("������ ���������� � AI.");
        }
    }
}

// ������ ��� �������� JSON-������
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
