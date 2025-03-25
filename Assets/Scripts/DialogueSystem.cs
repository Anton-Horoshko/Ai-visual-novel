using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI textComponent; // Поле для отображения текста
    public TMP_InputField inputField; // Поле для ввода текста пользователем
    public AIChat aiChat; // Ссылка на AIChat
    public float textSpeed; // Скорость появления текста

    private string currentText = ""; // Текущий текст AI
    private bool isTyping = false; // Флаг анимации печати

    void Start()
    {
        textComponent.text = "Привет! Задай мне вопрос."; // Начальный текст
        inputField.onSubmit.AddListener(SendToAI); // Добавляем обработчик ввода
    }

    // Метод для обработки ввода пользователя
    public void SendToAI(string userInput)
    {
        if (!isTyping && !string.IsNullOrWhiteSpace(userInput))
        {
            textComponent.text = "Думаю...";
            aiChat.SendMessageToAI(userInput, OnAIResponse); // Отправляем текст AI
            inputField.text = "";
        }
    }

    // Вызывается, когда AI присылает ответ
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
        inputField.Select(); // Переводим фокус на поле ввода
        inputField.ActivateInputField();
    }
}
