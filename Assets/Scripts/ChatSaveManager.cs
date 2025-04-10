using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChatSaveManager : MonoBehaviour
{
    private string savePath;
    public AIChat aiChat;
    public DialogueSystem dialogueSystem;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "chat_history.json");
    }

    public void SaveChatHistory(List<Message> chatHistory)
    {
        MessageListWrapper wrapper = new MessageListWrapper { messages = chatHistory };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, json);
        Debug.Log("История переписки сохранена в: " + savePath);
    }

    public List<Message> LoadChatHistory()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            MessageListWrapper wrapper = JsonUtility.FromJson<MessageListWrapper>(json);
            Debug.Log("История загружена.");
            return wrapper.messages;
        }

        Debug.LogWarning("Файл истории не найден. Возвращаем пустой список.");
        return new List<Message>();
    }

    public void SaveChatManually()
    {
        SaveChatHistory(aiChat.GetChatHistory());
    }

    public void LoadChatManually()
    {
        var loaded = LoadChatHistory();
        aiChat.SetChatHistory(loaded);
        dialogueSystem.DisplayChatHistory(loaded);
    }


}

[System.Serializable]
public class MessageListWrapper
{
    public List<Message> messages;
}