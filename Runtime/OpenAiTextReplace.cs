using System;
using System.Net.Mime;
using UnityEngine;
using OpenAi;
using TMPro;

namespace OpenAi
{
    public class OpenAiTextReplace : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        [TextAreaAttribute(1,20)]
        public string prompt;
        public OpenAiApi.Model model;
        [TextAreaAttribute(1,20)]
        public string response;
        
        public  void ReplaceText()
        {
            OpenAiApi openai = new OpenAiApi();
            // Completion completion = await openai.CreateCompletion(prompt, model);
            // if (textMesh != null)
            // {
            //     textMesh.text = completion.choices[0].text;
            // }
            
            openai.CreateCompletion(prompt, model, completion =>
            {
                if (textMesh != null)
                {
                    textMesh.text = completion.choices[0].text;
                }
            });
        }
    }
}