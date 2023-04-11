using OpenAiMonoBehaviour = OpenAi.OpenAiMonoBehaviour;
using UnityEngine;

public class HelloWorld : OpenAiMonoBehaviour
{
    private float _timePassed = 0;
    private float _previousTime = 0;

    void Start()
    {
        Debug.LogWarning("Hello World!");
    }

    private void Update() 
    {
        _timePassed = Time.time - _previousTime;
        Debug.LogWarning("Time Passed: " + _timePassed);
        _previousTime = Time.time;
    }
}