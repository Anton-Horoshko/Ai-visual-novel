using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class ChatSaveManager : MonoBehaviour
{
    private string savePath;
    public AIChat aiChat;
    public DialogueSystem dialogueSystem;
    public TextMeshProUGUI infoText;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "chat_history.json");
    }

    public void SaveChatHistory(List<Message> chatHistory)
    {
        MessageListWrapper wrapper = new MessageListWrapper { messages = chatHistory };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, json);
        StartCoroutine(ShowInfo("История переписки сохранена в: " + savePath));
    }

    public List<Message> LoadChatHistory()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            MessageListWrapper wrapper = JsonUtility.FromJson<MessageListWrapper>(json);
            StartCoroutine(ShowInfo("История загружена."));
            return wrapper.messages;
        }

        StartCoroutine(ShowInfo("Файл истории не найден."));
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

    IEnumerator ShowInfo(string info)
    {
        infoText.text = info;
        yield return new WaitForSeconds(5f);
        infoText.text = (" ");
    }

}



[System.Serializable]
public class MessageListWrapper
{
    public List<Message> messages;
}