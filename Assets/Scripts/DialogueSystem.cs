using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI historyTextComponent;
    public TMP_InputField inputField;
    public AIChat aiChat;
    public float textSpeed;
    private string currentText = "";
    private bool isTyping = false;

    void Start()
    {
        textComponent.text = "Привет! Как начнем нашу историю?";
        inputField.onSubmit.AddListener(SendToAI);
    }

    public void SendToAI(string userInput)
    {
        if (!isTyping && !string.IsNullOrWhiteSpace(userInput))
        {
            AddToHistory($"<b>Вы:</b> {userInput}");
            textComponent.text = "Думаю...";
            aiChat.SendMessageToAI(userInput, OnAIResponse);
            inputField.text = "";
        }
    }

    private void OnAIResponse(string aiText)
    {
        currentText = aiText;
        AddToHistory($"<b>AI:</b> {aiText}");
        StartCoroutine(TypeLine());
    }

    void AddToHistory(string line)
    {
        if (historyTextComponent != null)
        {
            historyTextComponent.text += line + "\n\n";
        }
    }

    public void DisplayChatHistory(List<Message> history)
    {
        historyTextComponent.text = "";

        foreach (Message msg in history)
        {
            if (msg.role == "user")
                historyTextComponent.text += "Ты: " + msg.content + "\n";
            else if (msg.role == "assistant")
                historyTextComponent.text += "AI: " + msg.content + "\n";
        }

        Message lastAIMessage = history.FindLast(m => m.role == "assistant");

        if (lastAIMessage != null)
        {
            textComponent.text = lastAIMessage.content;
        }
        else
        {
            textComponent.text = "Привет! Как начнем нашу историю?";
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in currentText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        inputField.Select();
        inputField.ActivateInputField();
    }
}
