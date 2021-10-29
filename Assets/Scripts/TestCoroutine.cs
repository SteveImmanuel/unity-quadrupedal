using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CoroutineHandler
{
    private float duration;
    private float completionBeforeCallback;
    private Action callback;

    private bool isActive;
    private IEnumerator enumerator;

    public CoroutineHandler(float duration, float completionBeforeCallback, Action callback)
    {
        this.duration = duration;
        this.completionBeforeCallback = completionBeforeCallback;
        this.callback = callback;
        enumerator = CoroutineFunc();
    }

    public IEnumerator GetEnumerator()
    {
        return enumerator;
    }

    public void SetCallback(Action callback)
    {
        this.callback = callback;
    }

    public bool IsActive()
    {
        return isActive;
    }

    private IEnumerator CoroutineFunc()
    {
        isActive = true;
        bool hasInvokedCallback = false;

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float currentStep = elapsedTime / duration;

            elapsedTime += Time.deltaTime;

            if (currentStep >= completionBeforeCallback && !hasInvokedCallback)
            {
                hasInvokedCallback = true;
                callback?.Invoke();
            }

            yield return null;
        }

        isActive = false;
    }
}
public class TestCoroutine : MonoBehaviour
{
    private CoroutineHandler handler;

    void Start()
    {
        handler = new CoroutineHandler(1f, 0.5f, null);
        StartCoroutine(handler.GetEnumerator());
        for(int i = 0; i < 10000; i++) { }
        handler.SetCallback(() => { Debug.Log("New Callback"); });
    }

    private void Update()
    {
        Debug.Log(handler.IsActive());
    }
}
