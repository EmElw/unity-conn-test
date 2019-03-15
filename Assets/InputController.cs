using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour, IInputListener, IInputController
{
    public Slider horizontal;
    public Slider vertical;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private readonly List<IInputListener> _listeners = new List<IInputListener>();

    public void OnCannonFire()
    {
        Debug.Log("OnCannonFire in Controller");
        foreach (var i in _listeners)
        {
            i.OnCannonFire();
        }
    }

    public void OnHorizontal(float value)
    {
        foreach (var i in _listeners)
        {
            i.OnHorizontal(horizontal.value);
        }
    }

    public void OnVertical(float value)
    {
        foreach (var i in _listeners)
        {
            i.OnVertical(vertical.value);
        }
    }

    public void AddListener(IInputListener listener)
    {
        _listeners.Add(listener);
    }
}

public interface IInputListener
{
    void OnCannonFire();
    void OnHorizontal(float value);
    void OnVertical(float value);
}

public interface IInputController
{
    void AddListener(IInputListener listener);
}