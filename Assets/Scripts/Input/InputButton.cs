using UnityEngine;
using UnityEngine.InputSystem;

public class InputButton
{
    public string name;
    public bool enabled;
    public System.Action onPressedDown;
    public System.Action<float> onHolding, onRelease;
    public EnabledCondition outsideEnabled;

    public delegate bool EnabledCondition();

    public float buttonTimePressed => pressed ? Time.time - timePressed : 0f;
    public int framesPressed => pressed ? Time.frameCount - framePressed : 0;
    public bool buttonIsPressed => pressed;
    public InputAction inputAction { get; protected set; }

    protected bool pressed;
    protected float timePressed;
    protected int framePressed;
    protected float timeReleased;
    protected int frameReleased;

    public InputButton (string name, InputAction inputAction)
    {
        this.name = name;
        this.inputAction = inputAction;
    }

    public virtual void Initialize ()
    {
        inputAction.started += c => ButtonPressed();
        inputAction.canceled += c => ButtonReleased();
        enabled = true;
    }

    public void Update ()
    {
        if (!enabled || !outsideEnabled())
        {
            pressed = false;
            return;
        }

        if (pressed && framePressed < Time.frameCount)
        {
            onHolding?.Invoke(Time.time - timePressed);
        }
    }

    protected virtual void ButtonPressed ()
    {
        if (!enabled || !outsideEnabled()) return;

        pressed = true;
        timePressed = Time.time;
        framePressed = Time.frameCount;

        onPressedDown?.Invoke();
    }
    protected virtual void ButtonReleased ()
    {
        if (!enabled || !outsideEnabled()) return;

        pressed = false;
        timeReleased = Time.time;
        frameReleased = Time.frameCount;

        onRelease?.Invoke(Time.time - timePressed);    
    }
}
