using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace OpenAi
{
    public class EditorWidowOrInspector<T> : Editor
    {
        // public delegate void Callback<T>();
        public delegate void Callback(T response=default);

        public Callback OnUpdate = null;
        
        public Object InternalTarget
        {
            set
            {
                // Hack to set target since Editor class doesn't provide an interface to do so. 
                var targetField = typeof(Editor).GetField("m_Targets", BindingFlags.Instance | BindingFlags.NonPublic);
                if (targetField != null)
                {
                    targetField.SetValue(this, new []{value});
                }
            }
            get
            {
                try
                {
                    return target;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}