using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool inputEnabled;

    public System.Action<Vector2, float> onMovementAxisMove;
    public Vector2 ivMove { get; private set; }
    public float valueMovementAxis => ivMove.magnitude;

    protected InputButton[] buttons;
    protected PlayerInputActions inputActions;

    public bool movementAxisMoving => valueMovementAxis > 0;

    protected virtual void Awake()
    {
        inputActions = new PlayerInputActions();
        buttons = new InputButton[]
        {
            new InputButton("ButtonSouth", inputActions.Player.ButtonSouth),
            new InputButton("ButtonWest", inputActions.Player.ButtonWest),
            new InputButton("ButtonNorth", inputActions.Player.ButtonNorth),
            new InputButton("RightBumper", inputActions.Player.RightBumper),
            new InputTrigger("RightTrigger", inputActions.Player.RightTrigger),
            new InputButton("D-PadL", inputActions.Player.PadL),
            new InputButton("D-PadR", inputActions.Player.PadR),
        };
    }
    protected virtual void OnEnable()
    {
        inputActions.Player.Move.Enable();
        buttons.ForEach(b => b.inputAction.Enable());
    }
    protected virtual void OnDisable()
    {
        inputActions.Disable();
    }
    protected virtual void Start()
    {
        buttons.ForEach(b => 
        {
            b.Initialize();
            b.outsideEnabled = () => inputEnabled;
        }
        );
    }
    protected void Update()
    {
        MoveAxis();
        buttons.ForEach(b => b.Update());
    }

    #region Input Bridge Methods
    protected virtual void MoveAxis ()
    {
        if (inputEnabled)
        {
            ivMove = inputActions.Player.Move.ReadValue<Vector2>();
        }
        else
        {
            ivMove = Vector2.zero;
            return;
        }

        Vector3 direction = ivMove.normalized;

        if (valueMovementAxis > 0.0f)
        {
            onMovementAxisMove?.Invoke(direction, valueMovementAxis);
        }
    }
    #endregion

    public InputButton GetButton (string name)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name == name)
                return buttons[i];
        }

        return null;
    }
}
