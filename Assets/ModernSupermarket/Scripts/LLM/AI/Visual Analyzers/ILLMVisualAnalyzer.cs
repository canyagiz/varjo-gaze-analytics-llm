using System;
using System.Collections;
using UnityEngine;

/*
 An interface to support polymorphism for analyzing via different api endpoints.
 */
public interface ILLMVisualAnalyzer
{
    string ServiceName { get; }
    IEnumerator AnalyzeScene(Action<string> onComplete);

    void SetTargetObject(LookableObject lookable);
    void SetViewDirection(Vector3 direction);


}