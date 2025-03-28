using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class StableDiffusionAPI : MonoBehaviour
{
    // Your API Key
    private string apiKey = "sk-9jQd4GBCG8BDk6DEzbqsvLkMrIczmkFRPxgvW4qEO555Glh4";  // Replace with your API key
    private string apiUrl = "https://api.stability.ai/v2beta/stable-image/generate/sd3";

    // Parameters
    private string prompt = "A futuristic cityscape at sunset";
    private string outputFormat = "jpeg";  // jpeg or png
    private string model = "sd3.5-large";  // Choose the model you want to use
    private string aspectRatio = "16:9";  // Aspect ratio (optional)

    // Optional parameters for image-to-image generation
    public Texture2D inputImage;  // A reference to the image you want to use as input for image-to-image (optional)
    private float strength = 0.75f;  // Control how much influence the input image has (valid for image-to-image mode)

    // Start the request

    void Start()
    {
        StartImageGeneration();
    }

    public void StartImageGeneration()
    {
        StartCoroutine(GenerateImageCoroutine());
    }

    private IEnumerator GenerateImageCoroutine()
    {
        // Prepare the form data for the POST request
        WWWForm form = new WWWForm();
        form.AddField("prompt", prompt);
        form.AddField("output_format", outputFormat);
        form.AddField("model", model);
        form.AddField("aspect_ratio", aspectRatio);

        // If you're using image-to-image, include the image data and strength
        if (inputImage != null)
        {
            // Convert the input image to a byte array (this is for image-to-image generation)
            byte[] imageBytes = inputImage.EncodeToJPG();  // or EncodeToPNG()
            form.AddBinaryData("image", imageBytes, "input_image.jpg", "image/jpeg");  // Add image data to the form
            form.AddField("strength", strength.ToString());
            form.AddField("mode", "image-to-image");  // We are doing image-to-image
        }
        else
        {
            // If it's text-to-image, just set mode
            form.AddField("mode", "text-to-image");
        }

        // Create the UnityWebRequest
        UnityWebRequest www = UnityWebRequest.Post(apiUrl, form);
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);
        www.SetRequestHeader("Accept", "image/*");

        // Send the request and wait for the response
        yield return www.SendWebRequest();

        // Check if the request was successful
        if (www.result == UnityWebRequest.Result.Success)
        {
            byte[] imageBytes = www.downloadHandler.data;  // The image bytes in response

            // Save the image to a file on the disk
            string filePath = Path.Combine(Application.persistentDataPath, "generated_image." + outputFormat);
            File.WriteAllBytes(filePath, imageBytes);
            Debug.Log("Image saved to: " + filePath);

            // Optionally, you can load the image into Unity as a texture
            Texture2D texture = new Texture2D(2, 2);  // Temporary texture size
            texture.LoadImage(imageBytes);  // Load the image bytes into the texture
            GetComponent<Renderer>().material.mainTexture = texture;  // Apply the texture to the object's material (if you want to display it)
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }
    }
}
