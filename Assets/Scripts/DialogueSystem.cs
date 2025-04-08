using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
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
            textComponent.text = "Думаю...";
            aiChat.SendMessageToAI(userInput, OnAIResponse);
            inputField.text = "";
        }
    }

    private void OnAIResponse(string aiText)
    {
        currentText = aiText;
        StartCoroutine(TypeLine());
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
