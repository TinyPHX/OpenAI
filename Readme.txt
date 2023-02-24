OpenAI Unity Integration

Transform your Unity project into an intelligent, language-aware application with OpenAI Unity Integration. With just a few lines of code, you can integrate OpenAI's powerful text completion models directly into your Unity project, allowing you to generate natural language text in real time.

Out-of-the-Box Components

OpenAI Unity Asset includes three components for integrating OpenAI APIs into Unity games:

OpenAiApiExample for both text completion and image generation
OpenAiImageReplace for replacing sprites with AI-generated images
OpenAiTextReplace for replacing text objects with AI-generated text.

Get Started

Requirements: Unity 2021.1.4f1 or later

Installation

- Obtain an OpenAI API key by following the instructions in the OpenAI documentation.
- Import the OpenAIUnityIntegration.unitypackage into your Unity project.
- Add the OpenAiApi script to an empty GameObject in your scene.
- Set the ApiKey and Organization fields in the inspector with your OpenAI API key and organization ID.

Usage

Here's an example of how you can create a text completion request and image generation request in Unity using the OpenAI Unity Integration:

Generate Text
------------------------------
using OpenAi;

var configuration = new Configuration(apiKey, organization);
var openai = new OpenAiApi(configuration, this);

openai.CreateCompletion("Hello world", "text-davinci-003", completion =>
{
Debug.Log(completion.choices[0].text);
});
------------------------------

Generate Images
------------------------------
using OpenAi;

var configuration = new Configuration(apiKey, organization);
var openai = new OpenAiApi(configuration, this);

openai.CreateImage("pixelated cat for game about evil space cats", image => {
    Texture texture = image.data[0].texture;
});
------------------------------

Review

A reputable reviewer had this to say about the asset:

"Overall, the code seems to be well-organized and follows good coding practices such as encapsulation and modularization."

ChatGPT
Documentation
For more information on how to use OpenAI's APIs, refer to the OpenAI documentation.