using UnityEngine;
using OpenAi;

public class SampleScript : MonoBehaviour {
    async void Start() {
        var openai = new OpenAiApi();
        AiImage image = await openai.CreateImage("Hello cat");
        Texture2D texture = image.Texture;
    }
}
