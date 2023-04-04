using System;
using System.Linq;
using UnityEngine;

namespace OpenAi
{
    public class OpenAiMonoBehaviour : MonoBehaviour
    {
        [HideInInspector] public string startPrompt;
        [HideInInspector] public EditArray editsArray = new EditArray();

        public void CreateEdit(string script, string editPrompt, string editedScript)
        {
            editsArray.edits = editsArray.edits.Append(new Edit(script, editPrompt, editedScript)).ToArray();
        }

        [Serializable]
        public class EditArray
        {
            [SerializeField] public Edit[] edits = new Edit[] { };
        }

        [Serializable]
        public class Edit
        {
            public string script;
            public string editPrompt;
            public string editedScript;

            public Edit(string script, string editPrompt, string editedScript)
            {
                this.script = script;
                this.editPrompt = editPrompt;
                this.editedScript = editedScript;
            }
        }
    }
}
