using OpenAiMonoBehaviour = OpenAi.OpenAiMonoBehaviour;
using UnityEngine;

/// <summary>
/// The Start function will log "Hello World!" to the debug console 
/// </summary> 
public class HelloWorld : OpenAiMonoBehaviour
{
    private float _timePassed = 0;
    private float _previousTime = 0;

    /// <summary>
    /// Logs "Hello World!" to the debug console
    /// </summary>
    void Start()
    {
        Debug.LogWarning("Hello World!");
    }

    /// <summary>
    /// Keeps track of the time passed since the previous frame 
    /// </summary>
    private void Update() 
    {
        _timePassed = Time.time - _previousTime;
        Debug.LogWarning("Time Passed: " + _timePassed);
        _previousTime = Time.time;
    }
}