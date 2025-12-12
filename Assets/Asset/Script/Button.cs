using System;
using System.Data.Common;
using UnityEngine;

public class Button : MonoBehaviour
{
    public int id;
    public event EventHandler<ButtonEventArgs> eventHandler;
    public class ButtonEventArgs
    {
        public int id;
    }
    private void OnMouseDown()
    {
        eventHandler.Invoke(this, new() { id = id });
    }
}
