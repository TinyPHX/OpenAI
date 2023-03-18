using System.Linq;
using UnityEngine;
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
        
        public async void ReplaceText()
        {
            ShowPlaceholderText();
            OpenAiApi openai = new OpenAiApi();
            var completion = await openai.CreateCompletion(prompt, model);
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
    }
}