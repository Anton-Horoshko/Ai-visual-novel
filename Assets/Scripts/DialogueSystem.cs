using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using System.Linq;

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
        MatchCollection matches = Regex.Matches(inputText, @"[^.!?\n]+[.!?\n]*", RegexOptions.Multiline);

        StringBuilder currentChunk = new StringBuilder();

        foreach (Match match in matches)
        {
            string sentence = match.Value.Trim();
            if (string.IsNullOrWhiteSpace(sentence)) continue;

            bool isDialogue = sentence.StartsWith("«") || sentence.StartsWith("\"") || sentence.StartsWith("—");

            if (sentence.Contains("\n\n"))
            {
                if (currentChunk.Length > 0)
                {
                    result.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }

                result.Add(sentence);
                continue;
            }

            bool WouldSplitTag(string chunk, string nextSentence)
            {
                string combined = chunk + nextSentence;

                int asterisks = combined.Count(c => c == '*');
                int underscores = combined.Count(c => c == '_');
                int openTags = Regex.Matches(combined, @"<[^/][^>]*>").Count;
                int closeTags = Regex.Matches(combined, @"</[^>]+>").Count;

                return (asterisks % 2 != 0 || underscores % 2 != 0 || openTags != closeTags);
            }

            if (currentChunk.Length + sentence.Length < maxLength || WouldSplitTag(currentChunk.ToString(), sentence))
            {
                currentChunk.Append(sentence + " ");
            }
            else
            {
                result.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentChunk.Append(sentence + " ");
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
            string chunk = textChunks[chunkIndex].Trim();
            StartCoroutine(TypeLine(chunk));
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
            string formattedLine = TextFormatter.ApplyRichTextFormatting(line);
            historyTextComponent.text += formattedLine + "\n\n";
        }
    }

    public void DisplayChatHistory(List<Message> history)
    {
        historyTextComponent.text = "";

        foreach (Message msg in history)
        {
            if (msg.role == "user")
            {
                historyTextComponent.text += "<b>Вы:</b> " + TextFormatter.ApplyRichTextFormatting(msg.content) + "\n\n";
            }
            else if (msg.role == "assistant")
            {
                historyTextComponent.text += "<b>AI:</b> " + TextFormatter.ApplyRichTextFormatting(msg.content) + "\n\n";
            }
        }

        Message lastAIMessage = history.FindLast(m => m.role == "assistant");

        if (lastAIMessage != null)
        {
            currentText = lastAIMessage.content;
            textChunks = SmartSplit(currentText);
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

        string formattedLine = TextFormatter.ApplyRichTextFormatting(line);
        textComponent.text = "";

        var builder = new StringBuilder();
        int i = 0;

        while (i < formattedLine.Length)
        {
            if (formattedLine[i] == '<')
            {
                int tagEnd = formattedLine.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    builder.Append(formattedLine.Substring(i, tagEnd - i + 1));
                    i = tagEnd + 1;
                    continue;
                }
            }

            builder.Append(formattedLine[i]);
            textComponent.text = builder.ToString();
            yield return new WaitForSeconds(textSpeed);
            i++;
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
