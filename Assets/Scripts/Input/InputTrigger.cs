using UnityEngine;
using UnityEngine.InputSystem;

public class InputTrigger : InputButton
{
    public float pressedThreshold;
    public System.Action<float> onChange;
    public System.Action<float> onRealising;
    public System.Action<float> onPressing;

    protected float currentValue;

    public InputTrigger(string name, InputAction inputAction, float pressedThreshold = 0.5f) : base(name, inputAction)
    {
        this.pressedThreshold = pressedThreshold;
    }

    public override void Initialize ()
    {
        inputAction.performed += OnChange;
        enabled = true;
    }

    protected virtual void OnChange (InputAction.CallbackContext ctx)
    {
        if (!enabled || !outsideEnabled()) return;

        float val = ctx.ReadValue<float>();
        float delta = currentValue - val;
        currentValue = val;

        if (delta > 0f)
        {
            if (currentValue <= pressedThreshold && pressed)
            {
                pressed = false;
                timeReleased = Time.time;
                frameReleased = Time.frameCount;

                onRelease?.Invoke(Time.time - timePressed);
            }

            onRealising?.Invoke(currentValue);
        }
        else
        {
            if (currentValue > pressedThreshold && !pressed)
            {
                pressed = true;
                timePressed = Time.time;
                framePressed = Time.frameCount;

                onPressedDown?.Invoke();
            }

            onPressing?.Invoke(currentValue);
        }

        onChange?.Invoke(currentValue);
    }
}
