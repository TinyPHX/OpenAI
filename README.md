OpenAI Unity Integration
========================

Transform your Unity project into an intelligent, language-aware application with OpenAI Unity Integration. With just a few lines of code, you can integrate OpenAI's powerful text completion models directly into your Unity project, allowing you to generate natural language text in real time.


Get Started
-----------

### Requirements

-   Unity 2021.3 or later
-   [OpenAI API from Unity Asset Store](https://assetstore.unity.com/packages/slug/247238)

### Setup

1. Create an [OpenAI Account](https://platform.openai.com/signup)  
2. Get your `Organization ID` from the [Settings page](https://beta.openai.com/docs/api-reference/authentication)
3. Create an `API Key` on the [API Key page](https://platform.openai.com/account/api-keys) 
4. Copy the `Organization ID` and `API Key` into a file named `auth.js`


```json
{
  "private_api_key":"YOUR-API-KEY",
  "organization":"YOUR-ORG-ID"
}
```

5. Place the file on you computer in `C:\Users\uname\.openai\auth.json`
    ![](https://i.imgur.com/VyqUK0r.png)

6. Add one of the built in components to your scene:
7. Add the `OpenAiImageReplace` or `OpenAiTextReplace` example components to any GameObject in your scene.
8. Add a prompt and click `Generate Image` or `Generate Text`


Out-of-the-Box Components
-------------------------

OpenAI Unity Asset includes three components for integrating OpenAI APIs into Unity games:

-   `OpenAiApiExample` for both text completion and image generation
-   `OpenAiImageReplace` for replacing sprites with AI-generated images
-   `OpenAiTextReplace` for replacing text objects with AI-generated text.


Scritping Interface
-------------------------

Here's an example of how you can create a text completion request and image generation request in Unity using the OpenAI Unity Integration:

#### Generate Text

Simple text generation
```csharp
using UnityEngine;
using OpenAi;

public class SampleScript : MonoBehaviour {
    async void Start() {
        var openai = new OpenAiApi(this);
        Completion completion = await openai.CreateCompletion("Hello world");
        Debug.Log("OpenAI Response: " + completion.Text);
    }
}
```

Using a callback instead async/await
```csharp
openai.CreateCompletion("Hello world", completion =>
{
    Debug.Log("OpenAI Response: " + completion.Text);
});
```

#### Generate Images

Simple image generation
```csharp
using UnityEngine;
using OpenAi;

public class SampleScript : MonoBehaviour {
    async void Start() {
        var openai = new OpenAiApi( this);
        Image image = await openai.CreateImage("Hello cat");
        Texture2D texture = image.Texture2d;
    }
}
```

Using a callback instead async/await
```csharp
openai.CreateImage("Hello world", completion =>
{
    Debug.Log("OpenAI Response: " + image.Text);
});
```

Review
------

A reputable reviewer had this to say about the asset:

"Overall, the code seems to be well-organized and follows good coding practices such as encapsulation and modularization."

-   ChatGPT

Documentation
-------------
For more information on how to use OpenAI's APIs, refer to the [OpenAI documentation](https://beta.openai.com/docs).