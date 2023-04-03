using System.Linq;
using MyBox;
using OpenAI.AiModels;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace OpenAi
{
    public class OpenAiReplaceText : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        public Text text;
        [TextArea(1,20)]
        public string prompt;
        public Models.Text modelName = Models.Text.GPT_3;
        public bool stream = true;
        [TextArea(1,20), ReadOnly]
        public string response;
            
        [SerializeField, HideInInspector]
        private bool componentsInitialized = false;
        
        public void ReplaceText()
        {
            ShowPlaceholderText();
            OpenAiApi openai = new OpenAiApi();
            openai.TextCompletion(prompt, modelName, stream: stream, callback: completion =>
            {
                response = completion.choices[0].text;
                if (textMesh != null)
                {
                    textMesh.text = response;
                }
                if (text != null)
                {
                    text.text = response;
                }
            });
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
                text = GetComponent<Text>();
            }
        }
    }
}