using UnityEngine;

/// <summary>
/// Simulates an elastic float that moves toward a target value
/// with customizable speed (magnetic force) and elasticity (damping).
/// External forces can be applied to make it "kick" away and bounce back.
/// </summary>
[System.Serializable]
public class ElasticFloat
{
    [Range(-10f, 10f)]
    public float currentValue;
    public float currentVelocity;
    public float snappingThreshold = 0.01f;

    public float target;
    public float magneticForce;  // Pull toward target
    public float damping;      // 0–1, how quickly it loses energy

    public float Value => currentValue;
    public bool IsActive => currentValue == target;

    public ElasticFloat(float startValue = 0f)
    {
        currentValue = startValue;
    }

    /// <summary>
    /// Applies an external force impulse (like pushing or pulling the value).
    /// Positive pushes upward, negative downward.
    /// </summary>
    public void ApplyForce(float force)
    {
        currentVelocity += force; // Adds to momentum
    }

    /// <summary>
    /// Update spring motion toward target.
    /// </summary>
    public void Update(float deltaTime)
    {
        // This is a classic damped spring integrator.
        // acceleration = -k * (x - target) - c * v

        float displacement = currentValue - target;

        if (Mathf.Abs(displacement) + Mathf.Abs(currentVelocity) < snappingThreshold)
        {
            displacement = 0f;
            currentVelocity = 0f;
            currentValue = target;
        }

        float springForce = -magneticForce * displacement;
        float dampingForce = -2f * Mathf.Sqrt(magneticForce) * damping * currentVelocity;

        float acceleration = springForce + dampingForce;

        currentVelocity += acceleration * deltaTime;
        currentValue += currentVelocity * deltaTime;
    }
}
