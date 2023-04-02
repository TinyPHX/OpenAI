using System.Linq;
using MyBox;
using OpenAI.AiModels;
using UnityEngine;
using TMPro;

namespace OpenAi
{
    public class OpenAiReplaceText : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        [TextArea(1,20)]
        public string prompt;
        public Models.Text modelName = Models.Text.GPT_3;
        [TextArea(1,20), ReadOnly]
        public string response;
            
        [SerializeField, HideInInspector]
        private bool componentsInitialized = false;
        
        public async void ReplaceText()
        {
            ShowPlaceholderText();
            OpenAiApi openai = new OpenAiApi();
            var completion = await openai.TextCompletion(prompt, modelName);
            response = completion.choices[0].text;
            if (textMesh != null)
            {
                textMesh.text = response;
            }
        }

        public void ShowPlaceholderText()
        {
            int lineReturnCount = 0;
            if (response != null)
            {
                lineReturnCount = response.Count(character => character == '\n');
            }

            response = "Generating..." + new string(Enumerable.Repeat('\n', lineReturnCount).ToArray());
        }
            
        public void Update()
        {
            GetComponents();
        }

        public void Reset()
        {
            GetComponents();
        }

        public void GetComponents()
        {
            if (!componentsInitialized)
            {
                componentsInitialized = true;
                textMesh = GetComponent<TextMeshProUGUI>();
            }
        }
    }
}