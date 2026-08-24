using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Rotates this GameObject to face a camera. Each rotation axis can be enabled
/// independently, so you can make a full billboard or a Y-only "look at" that
/// keeps the object upright. Runs in edit mode as well as play mode.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rompe Oros/Look At Camera")]
public class LookAtCamera : MonoBehaviour
{
    public enum FacingMode
    {
        /// <summary>+Z points at the camera (meshes, arrows, 3D props).</summary>
        Forward,
        /// <summary>-Z points at the camera (world-space UI, quads, sprites).</summary>
        Backward
    }

    [Header("Target")]
    [Tooltip("Camera to look at. Leave empty to use Camera.main (and the Scene view camera while in edit mode).")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Face the camera's position instead of aligning with its view plane. " +
             "Off = classic billboard (all objects share the camera's orientation, no distortion at screen edges).")]
    [SerializeField] private bool lookAtPosition = true;

    [Header("Axes To Follow")]
    [SerializeField] private bool rotateX = true;
    [SerializeField] private bool rotateY = true;
    [SerializeField] private bool rotateZ = true;

    [Header("Options")]
    [SerializeField] private FacingMode facing = FacingMode.Forward;

    [Tooltip("Up vector used to build the rotation. Usually world up (0,1,0), or the camera's up for a true screen-aligned billboard.")]
    [SerializeField] private bool useCameraUp;

    [Tooltip("Degrees per second. 0 = snap instantly.")]
    [SerializeField, Min(0f)] private float rotationSpeed;

    [Header("Editor")]
    [Tooltip("Keep updating in the Scene view while not playing.")]
    [SerializeField] private bool updateInEditMode = true;

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (!Application.isPlaying)
            EditorApplication.update += EditorTick;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorTick;
    }

    private void EditorTick()
    {
        if (this == null || Application.isPlaying || !updateInEditMode || !isActiveAndEnabled)
            return;

        Apply();
    }
#endif

    private void LateUpdate()
    {
        if (!Application.isPlaying)
            return;

        Apply();
    }

    private void Apply()
    {
        if (!rotateX && !rotateY && !rotateZ)
            return;

        Camera cam = ResolveCamera();
        if (cam == null)
            return;

        Vector3 direction = lookAtPosition
            ? cam.transform.position - transform.position
            : -cam.transform.forward;

        if (facing == FacingMode.Backward)
            direction = -direction;

        if (direction.sqrMagnitude < 1e-8f)
            return;

        Vector3 up = useCameraUp ? cam.transform.up : Vector3.up;
        if (Mathf.Abs(Vector3.Dot(direction.normalized, up.normalized)) > 0.9999f)
            up = cam.transform.up; // avoid a degenerate LookRotation when looking straight up/down

        Quaternion target = Quaternion.LookRotation(direction, up);

        // Keep the current angle on any axis that is switched off.
        Vector3 current = transform.rotation.eulerAngles;
        Vector3 wanted = target.eulerAngles;
        target = Quaternion.Euler(
            rotateX ? wanted.x : current.x,
            rotateY ? wanted.y : current.y,
            rotateZ ? wanted.z : current.z);

        if (rotationSpeed > 0f && Application.isPlaying)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = target;
        }
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null)
            return targetCamera;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null && view.camera != null)
                return view.camera;
        }
#endif
        return Camera.main;
    }
}
