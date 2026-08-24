using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    const float EPSILON = 1e-8f;
    const int VELOCITY_LAYERS = 8;
    const int MAX_COLLISION_CHECKS = 3;

    [Header("General")]
    [Range(0.001f, 0.1f)]
    public float skinWidth = 0.01f;
    public float mass = 1f;
    public float drag = 0.1f;
    public float maxSlope = 45f;
    [Tooltip("Small downward pull while grounded to maintain contact.")]
    public float groundStick = -2f;
    public LayerMask collisionMask = ~0;
    [Tooltip("Off = infinite mass: never displaced, the other side clears the whole overlap.")]
    public bool pushable = true;


    [Header("Locomotion")]
    public float moveSpeed;
    public float moveAcceleration;
    public float moveDeceleration;

    [Header("Gravity")]
    public float fallGravity = -10f;
    public float riseGravity = -8f;
    public float maxGravity = -50f;

    [Header("Ground Probe")]
    [Tooltip("How far below the feet we'll snap down (stairs/slopes)")]
    public float groundProbeDistance = 0.3f;
    [Tooltip("How far the flat foot may snap UP (ledge-sink fix + tiny steps)")]
    public float maxStepUp = 0.3f;           
    public int groundRingRays = 8;           
    [Range(0.1f, 1f)] public float groundRingScale = 0.9f;

    public bool IsGrounded => _isGrounded;

    // Components
    CapsuleCollider _capsule;
    Rigidbody _rigidBody;

    // Solicited
    float _solicitedJumpForce = default;
    Vector3 _solicitedlocomotion = default;
    Vector3 _solicitedImpulse = default;

    // Engine
    Vector3 _locomotionVelocity = default;
    Vector3 _forceVelocity = default;
    Vector3 _movingSurfaceVelocity = default;
    Vector3[] _customVelocity = new Vector3[VELOCITY_LAYERS];

    // Result
    Vector3 _vVelocity;
    Vector3 _hVelocity;
    Vector3 _velocity;

    bool _lastGrounded = false;
    bool _isGrounded = false;
    Vector3 _groundNormal = default;

    // Character collision
    static readonly Dictionary<Collider, Character> _registry = new Dictionary<Collider, Character>();
    static readonly Collider[] _overlapBuffer = new Collider[16];
    static readonly RaycastHit[] _castBuffer = new RaycastHit[16];

    private void Awake()
    {
        _capsule = GetComponent<CapsuleCollider>();
        _rigidBody = GetComponent<Rigidbody>();

        Setup();
    }

    private void OnEnable()
    {
        if (_capsule != null) _registry[_capsule] = this;
    }
    private void OnDisable()
    {
        if (_capsule != null) _registry.Remove(_capsule);
    }

    void Setup ()
    {
        _rigidBody.isKinematic = true;
        _capsule.isTrigger = false;
    }

    public void Move (Vector3 direction, float factor)
    {
        _solicitedlocomotion = direction.normalized * factor * moveSpeed;
    }
    public void Jump (float force)
    {
        _solicitedJumpForce += force;
    }
    public void Impulse (Vector3 velocity)
    {
        _solicitedImpulse += velocity;
    }
    public void CustomVelocity (Vector3 velocity, int layer)
    {
        _customVelocity[layer] = velocity;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 pos = _rigidBody.position;

        // Step 1: clear grounding state and other stale refs.
        _lastGrounded = _isGrounded;
        _isGrounded = false;

        // Step 2: Fix any overlaps that might have happen during the last physics frame.
        pos = SeparateFromCharacters(pos);
        pos = Depenetrate(pos);

        // Step 3: Integrate velocities.
        IntegrateVelocities();

        // Step 4: Horizontal pass.
        _hVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
        pos += CollideAndSlide(_hVelocity * dt, pos, 0, false, _hVelocity * dt);

        // Step 5: Vertical pass & grounding
        bool rising = _velocity.y > 0f;
        if (!rising)
        {
            // Reach covers the normal snap plus this frame's fall, so fast drops still catch.
            float fall = Mathf.Max(0f, -_velocity.y * dt);
            float reach = Mathf.Max(groundProbeDistance, fall + skinWidth);

            if (ProbeGround(pos, reach, out float groundY, out Vector3 gN))
            {
                _isGrounded = true;
                _groundNormal = gN;

                float delta = groundY - FeetY(pos); // + = ground above feet (lift), - = below (snap down)
                delta = Mathf.Clamp(delta, -reach, maxStepUp);
                pos.y += delta;

                if (_velocity.y < 0f) _velocity.y = 0f;
            }
            else
            {
                // Nothing under the foot -> fall with the real capsule (handles edges/ceilings).
                _vVelocity = new Vector3(0f, _velocity.y, 0f);
                pos += CollideAndSlide(_vVelocity * dt, pos, 0, true, _vVelocity * dt);
            }
        }
        else
        {
            // Rising -> jump / ceiling bonk with the real capsule.
            _vVelocity = new Vector3(0f, _velocity.y, 0f);
            pos += CollideAndSlide(_vVelocity * dt, pos, 0, true, _vVelocity * dt);
        }

        // Step 6: Consume vertical velocity if grounded.
        if (_isGrounded && _velocity.y < 0f) _velocity.y = 0f;

        // Step 7: Apply.
        _rigidBody.MovePosition(pos);
    }

    void IntegrateVelocities ()
    {
        _velocity.x = 0f;
        _velocity.z = 0f;

        float a = _solicitedlocomotion.sqrMagnitude > _locomotionVelocity.sqrMagnitude
            ? moveAcceleration : moveDeceleration;
        _locomotionVelocity = Vector3.MoveTowards
            (_locomotionVelocity, _solicitedlocomotion, a * Time.fixedDeltaTime);
        _locomotionVelocity.y = 0f;

        _velocity += _locomotionVelocity;

        _forceVelocity += _solicitedImpulse;
        _forceVelocity = Vector3.MoveTowards(_forceVelocity, Vector3.zero, drag * Time.fixedDeltaTime);

        _velocity += _forceVelocity;

        for (int i = 0; i < _customVelocity.Length; i++)
        {
            _velocity += _customVelocity[i];
        }

        if (_lastGrounded && _velocity.y <= 0f)
            _velocity.y = groundStick;
        else
            _velocity.y += _velocity.y > 0 ? 
                riseGravity * Time.fixedDeltaTime : fallGravity * Time.fixedDeltaTime;

        if (_solicitedJumpForce > 0)
            _velocity.y = _solicitedJumpForce;

        _solicitedJumpForce = 0;
        _solicitedImpulse = Vector3.zero;
        _solicitedlocomotion = Vector3.zero;
    }

    #region Collision Handleling

    private Vector3 CollideAndSlide(Vector3 vel, Vector3 pos, int depth,
                                    bool gravityPass, Vector3 velInit)
    {
        if (depth >= MAX_COLLISION_CHECKS || vel.sqrMagnitude < EPSILON)
            return Vector3.zero;

        float dist = vel.magnitude + skinWidth;
        GetCapsulePoints(pos, out Vector3 p1, out Vector3 p2);

        if (SweepEnvironment(p1, p2, vel.normalized, dist, out RaycastHit hit))
        {
            Vector3 snap = vel.normalized * (hit.distance - skinWidth);
            Vector3 leftover = vel - snap;

            // If advance is tinier than skin width dont advance just slide.
            if (snap.magnitude <= skinWidth) snap = Vector3.zero;

            float angle = Vector3.Angle(Vector3.up, hit.normal);

            // Ground
            if (angle <= maxSlope)
            {
                if (gravityPass)
                {
                    _isGrounded = true;
                    _groundNormal = hit.normal;
                    return snap;
                }

                leftover = ProjectAndScale(leftover, hit.normal);
            }
            // Wall
            else
            {
                // Reduce speed for head-on hits (kills interior-corner boosting).
                float scale = 1f - Vector3.Dot(
                    Flatten(hit.normal).normalized,
                    -Flatten(velInit).normalized);

                // If grounded dont slide up.
                if (_lastGrounded && !gravityPass)
                {
                    leftover = ProjectAndScale(Flatten(leftover),
                                               Flatten(hit.normal).normalized);
                }
                // Otherwise do.
                else
                {
                    leftover = ProjectAndScale(leftover, hit.normal);
                }

                leftover *= scale;
            }

            // Next depth.
            return snap + CollideAndSlide(leftover, pos + snap, depth + 1,
                                          gravityPass, velInit);
        }

        return vel; // No Hit.
    }

    private Vector3 Depenetrate(Vector3 pos)
    {
        const int iterations = 3;
        for (int i = 0; i < iterations; i++)
        {
            GetCapsulePoints(pos, out Vector3 p1, out Vector3 p2);
            Collider[] hits = Physics.OverlapCapsule(p1, p2, _capsule.radius,
                                                     collisionMask,
                                                     QueryTriggerInteraction.Ignore);
            bool resolved = false;
            foreach (Collider other in hits)
            {
                if (other == _capsule) continue;
                if (IsCharacter(other)) continue;   // handled by SeparateFromCharacters

                if (Physics.ComputePenetration(
                        _capsule, pos, transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 dir, out float d))
                {
                    pos += dir * d;
                    resolved = true;
                }
            }

            if (!resolved) break;
        }
        return pos;
    }

    private bool ProbeGround(Vector3 pos, float reach, out float groundY, out Vector3 normal)
    {
        GetCapsulePoints(pos, out _, out Vector3 p2);
        float r = _capsule.radius;

        // Start slightly above the equator; cast down past the feet by 'reach'.
        Vector3 originC = p2 + Vector3.up * skinWidth;
        float rayLen = r + reach + skinWidth;

        float bestDist = float.PositiveInfinity;   // smallest distance == highest ground
        Vector3 nAccum = Vector3.zero;
        int hits = 0;

        CastFootRay(originC, rayLen, ref bestDist, ref nAccum, ref hits);   // center
        for (int i = 0; i < groundRingRays; i++)
        {
            float a = (i / (float)groundRingRays) * Mathf.PI * 2f;
            Vector3 off = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * groundRingScale);
            CastFootRay(originC + transform.rotation * off, rayLen, ref bestDist, ref nAccum, ref hits);
        }

        if (hits == 0) { groundY = 0f; normal = Vector3.up; return false; }

        groundY = originC.y - bestDist;
        normal = nAccum.sqrMagnitude > EPSILON ? (nAccum / hits).normalized : Vector3.up;
        return true;
    }

    private void CastFootRay(Vector3 origin, float len, ref float bestDist, ref Vector3 nAccum, ref int hits)
    {
        if (RaycastEnvironment(origin, Vector3.down, len, out RaycastHit hit))
        {
            if (Vector3.Angle(Vector3.up, hit.normal) <= maxSlope)   // walkable only
            {
                if (hit.distance < bestDist) bestDist = hit.distance;
                nAccum += hit.normal;
                hits++;
            }
        }
    }
    private bool SweepEnvironment(Vector3 p1, Vector3 p2, Vector3 dir, float dist, out RaycastHit hit)
    {
        int count = Physics.CapsuleCastNonAlloc(p1, p2, _capsule.radius, dir, _castBuffer,
                                                dist, collisionMask, QueryTriggerInteraction.Ignore);
        return ClosestValidHit(count, out hit);
    }

    private bool RaycastEnvironment(Vector3 origin, Vector3 dir, float len, out RaycastHit hit)
    {
        int count = Physics.RaycastNonAlloc(origin, dir, _castBuffer, len,
                                            collisionMask, QueryTriggerInteraction.Ignore);
        return ClosestValidHit(count, out hit);
    }

    private bool ClosestValidHit(int count, out RaycastHit hit)
    {
        hit = default;
        float best = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            if (_castBuffer[i].distance <= 0f) continue;
            if (_castBuffer[i].normal.sqrMagnitude < EPSILON) continue;

            Collider col = _castBuffer[i].collider;
            if (col == _capsule || IsCharacter(col)) continue;
            if (_castBuffer[i].distance >= best) continue;

            best = _castBuffer[i].distance;
            hit = _castBuffer[i];
        }

        return best < float.PositiveInfinity;
    }

    #endregion

    #region Character Collision

    private Vector3 SeparateFromCharacters(Vector3 pos)
    {
        if (!pushable) return pos;

        GetCapsulePoints(pos, out Vector3 p1, out Vector3 p2);
        int count = Physics.OverlapCapsuleNonAlloc(p1, p2, _capsule.radius, _overlapBuffer,
                                                   collisionMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider col = _overlapBuffer[i];
            if (col == _capsule) continue;
            if (!TryGetCharacter(col, out Character other) || other == this) continue;

            float share = SeparationShare(other);
            if (share <= 0f) continue;

            if (!Physics.ComputePenetration(
                    _capsule, pos, transform.rotation,
                    col, col.transform.position, col.transform.rotation,
                    out Vector3 dir, out float depth))
                continue;

            dir = Flatten(dir);

            if (dir.sqrMagnitude < EPSILON) dir = FallbackSeparationDir(pos, col.transform.position);
            else dir.Normalize();

            pos += dir * (depth * share);
        }

        return pos;
    }

    private float SeparationShare(Character other)
    {
        if (!pushable) return 0f;          
        if (!other.pushable) return 1f;   

        float total = mass + other.mass;
        return total > EPSILON ? other.mass / total : 0.5f;
    }

    // Avoid stacking characters one on top of another.
    private Vector3 FallbackSeparationDir(Vector3 pos, Vector3 otherPos)
    {
        Vector3 d = Flatten(pos - otherPos);
        if (d.sqrMagnitude > EPSILON) return d.normalized;

        float a = (GetInstanceID() & 0xFF) / 255f * Mathf.PI * 2f;
        return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
    }

    private static bool TryGetCharacter(Collider col, out Character character)
    {
        if (_registry.TryGetValue(col, out character)) return true;

        Rigidbody rb = col.attachedRigidbody;
        return rb != null && rb.TryGetComponent(out character);
    }

    private static bool IsCharacter(Collider col) => TryGetCharacter(col, out _);

    #endregion

    #region Utils

    private void GetCapsulePoints(Vector3 pos, out Vector3 p1, out Vector3 p2)
    {
        Vector3 center = pos + transform.rotation * _capsule.center;
        float half = Mathf.Max(_capsule.height * 0.5f - _capsule.radius, 0f);
        Vector3 up = transform.up;
        p1 = center + up * half;
        p2 = center - up * half;
    }
    private Vector3 ProjectAndScale(Vector3 v, Vector3 normal)
    {
        float mag = v.magnitude;
        v = Vector3.ProjectOnPlane(v, normal).normalized;
        return v * mag;
    }
    private Vector3 Flatten(Vector3 v) => new Vector3(v.x, 0f, v.z);
    private float FeetY(Vector3 pos)
    {
        GetCapsulePoints(pos, out _, out Vector3 p2);   // p2 = bottom sphere center
        return p2.y - _capsule.radius;
    }

    #endregion

#if UNITY_EDITOR

    #region Gizmos

    static Mesh _probeMesh;
    static int _probeMeshSides;

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider capsule = _capsule != null ? _capsule : GetComponent<CapsuleCollider>();
        if (capsule == null) return;

        // Same origin and reach as ProbeGround, at rest (a fast fall stretches it further).
        Vector3 center = transform.position + transform.rotation * capsule.center;
        float half = Mathf.Max(capsule.height * 0.5f - capsule.radius, 0f);
        Vector3 origin = center - transform.up * half + Vector3.up * skinWidth;
        float height = capsule.radius + groundProbeDistance + skinWidth;
        float radius = capsule.radius * groundRingScale;

        Matrix4x4 restore = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, new Vector3(radius, height, radius));

        // Unit prism: radius 1, y from 0 (origin) down to -1 (deepest the rays reach).
        if (groundRingRays >= 3)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawMesh(ProbeMesh(groundRingRays));

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            for (int i = 0; i < groundRingRays; i++)
            {
                Vector3 a = RingPoint(i, groundRingRays);
                Vector3 b = RingPoint(i + 1, groundRingRays);

                Gizmos.DrawLine(a, a + Vector3.down);   // the ring ray itself
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(a + Vector3.down, b + Vector3.down);
            }
        }

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawLine(Vector3.zero, Vector3.down);    // centre ray

        Gizmos.matrix = restore;
    }

    private static Vector3 RingPoint(int i, int sides)
    {
        float a = (i % sides / (float)sides) * Mathf.PI * 2f;
        return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
    }

    private static Mesh ProbeMesh(int sides)
    {
        if (_probeMesh != null && _probeMeshSides == sides) return _probeMesh;

        if (_probeMesh == null)
            _probeMesh = new Mesh { name = "GroundProbeGizmo", hideFlags = HideFlags.HideAndDontSave };

        _probeMeshSides = sides;

        Vector3[] verts = new Vector3[sides * 2 + 2];
        for (int i = 0; i < sides; i++)
        {
            Vector3 dir = RingPoint(i, sides);
            verts[i] = dir;                          // top ring
            verts[sides + i] = dir + Vector3.down;   // bottom ring
        }

        int topCenter = sides * 2;
        int botCenter = sides * 2 + 1;
        verts[topCenter] = Vector3.zero;
        verts[botCenter] = Vector3.down;

        List<int> tris = new List<int>(sides * 12);
        for (int i = 0; i < sides; i++)
        {
            int n = (i + 1) % sides;

            tris.Add(i); tris.Add(n); tris.Add(sides + n);           // side
            tris.Add(i); tris.Add(sides + n); tris.Add(sides + i);
            tris.Add(topCenter); tris.Add(n); tris.Add(i);           // caps
            tris.Add(botCenter); tris.Add(sides + i); tris.Add(sides + n);
        }

        // Mirrored winding so the volume stays filled when the camera is inside it.
        for (int i = tris.Count - 3; i >= 0; i -= 3)
        {
            tris.Add(tris[i]); tris.Add(tris[i + 2]); tris.Add(tris[i + 1]);
        }

        _probeMesh.Clear();
        _probeMesh.vertices = verts;
        _probeMesh.triangles = tris.ToArray();
        _probeMesh.RecalculateNormals();
        return _probeMesh;
    }

    #endregion

#endif
}
