using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI historyTextComponent;
    public TMP_InputField inputField;
    public AIChat aiChat;
    public float textSpeed = 0.02f;
    private string currentText = "";
    private List<string> textChunks;
    private int chunkIndex = 0;
    private bool isTyping = false;
    private bool awaitingNextChunk = false;

    void Start()
    {
        textComponent.text = "Привет! Как начнем нашу историю?";
        inputField.onSubmit.AddListener(SendToAI);
    }

    void Update()
    {
        if (awaitingNextChunk && Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextChunk();
        }
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

        textChunks = SmartSplit(currentText);

        chunkIndex = 0;
        ShowNextChunk();
    }

    List<string> SmartSplit(string inputText, int maxLength = 300)
    {
        List<string> result = new List<string>();
        string[] sentences = inputText.Split(new[] { ". ", "? ", "! ", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        StringBuilder currentChunk = new StringBuilder();
        foreach (string sentence in sentences)
        {
            string trimmed = sentence.Trim();

            bool isDialogue = trimmed.StartsWith("«") || trimmed.StartsWith("\"") || trimmed.StartsWith("—");

            if (trimmed.Contains("\n\n"))
            {
                if (currentChunk.Length > 0)
                {
                    result.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }
                result.Add(trimmed);
                continue;
            }

            if (isDialogue || trimmed.Length > maxLength)
            {
                if (currentChunk.Length > 0)
                {
                    result.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }
                result.Add(trimmed + ".");
                continue;
            }

            if (currentChunk.Length + trimmed.Length < maxLength)
            {
                currentChunk.Append(trimmed + ". ");
            }
            else
            {
                result.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentChunk.Append(trimmed + ". ");
            }
        }

        if (currentChunk.Length > 0)
        {
            result.Add(currentChunk.ToString().Trim());
        }

        return result;
    }


    void ShowNextChunk()
    {
        if (chunkIndex < textChunks.Count)
        {
            StartCoroutine(TypeLine(textChunks[chunkIndex].Trim() + (currentText.Contains(". ") ? "." : "")));
            chunkIndex++;
        }
        else
        {
            awaitingNextChunk = false;
            inputField.Select();
            inputField.ActivateInputField();
        }
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
            currentText = lastAIMessage.content;
            textChunks = new List<string>(currentText.Split(new[] { "\n", ". ", "? ", "! " }, System.StringSplitOptions.RemoveEmptyEntries));
            chunkIndex = 0;
            ShowNextChunk();
        }
        else
        {
            textComponent.text = "Привет! Как начнем нашу историю?";
        }


    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        awaitingNextChunk = false;
        textComponent.text = "";

        foreach (char c in line)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;

        if (chunkIndex < textChunks.Count)
        {
            awaitingNextChunk = true;
            textComponent.text += "\n<color=grey><size=70%>нажмите Enter...</size></color>";
        }
        else
        {
            awaitingNextChunk = false;
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

}
