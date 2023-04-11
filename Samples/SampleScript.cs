using UnityEngine;
using OpenAi;
using OpenAI.AiModels;
using TMPro;
using UnityEngine.UI;

public class SampleScript : MonoBehaviour
{
    public string apikey = "your-api-key-here-for-testing";
    
    public InputField apikeyText;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public Image image1; 
    public Image image2; 
    public Image image3;

    public Texture2D imageToEdit;
    public Texture2D imageMask;
    
    public Texture2D imageToVary;
    
    private OpenAiApi openai;
    
    void Start()
    {
        apikeyText.text = apikey;
        apikeyText.onEndEdit.AddListener(InputFieldUpdated);
        openai = new OpenAiApi(new Configuration(apikey, ""));
    }

    private void InputFieldUpdated(string update)
    {
        apikey = update;
        openai = new OpenAiApi(new Configuration(apikey, ""));
    }

    public void TextCompletion()
    {
        openai.TextCompletion("How much wood can a wood chuck chuck? ",
            Models.Text.GPT_3,
            n: 4,
            temperature: 1f,
            max_tokens: 400,
            stream: true,
            callback: aiText =>
            {
                text1.text = aiText.choices[0].text;
            });
    }

    public void ChatCompletion()
    {
        openai.ChatCompletion(new[] { new Message("How much wood can a wood chuck chuck? ") },
            Models.Chat.GPT_4,
            n: 6,
            temperature: 0.2f,
            stream: true,
            callback: aiChat =>
            {
                text2.text = aiChat.choices[0].message.content;
            });
    }

    public void CreateImage()
    {
        openai.CreateImage("Wood chuck", aiImage =>
            {
                Texture2D texture = aiImage.data[0].texture;
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image1.sprite = newSprite;
            });
    }

    public void CreateImageEdit()
    {
        openai.CreateImageEdit(imageToEdit, imageMask, "extend the bounds of this banana skateboarding", aiImage =>
            {
                Texture2D texture = aiImage.data[0].texture;
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image2.sprite = newSprite;
            });
    }

    public void CreateImageVariant()
    {
        openai.CreateImageVariant(imageToVary, aiImage =>
            {
                Texture2D texture = aiImage.data[0].texture;
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image3.sprite = newSprite;
            });
    }
}
