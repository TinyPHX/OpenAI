using OpenAiMonoBehaviour = OpenAi.OpenAiMonoBehaviour;
using UnityEngine;

public class HelloWorld : OpenAiMonoBehaviour
{
    private float _ellapsed = 0;
    private float _previousTime = 0;

    void Start()
    {
        Debug.LogWarning("Hello World!");
    }

    private void Update() 
    {
        _ellapsed = Time.time - _previousTime;
        Debug.LogWarning("Time Passed: " + _ellapsed);
        _previousTime = Time.time;
    }
}