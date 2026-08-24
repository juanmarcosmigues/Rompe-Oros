# Prose and code listings for the "Cylinder vs oriented box" section.
# Kept apart from gen_obb_section.py so the diagram code stays readable.


def pages(dia_tiers, dia_projection):
    P1 = """<h1>Cylinder vs oriented box</h1>
<p>A <code>UnityEngine.BoxCollider</code> is only axis-aligned when nothing above it in the hierarchy
is rotated, which in practice means ramps, tilted platforms and every yawed wall fall outside
<code>CylinderVsAabb</code>. The triangle path handles them correctly &mdash; a box surface genuinely
is 12 triangles &mdash; but it is the wrong tool here, for three reasons.</p>
<ul>
<li><b>Internal-edge ghosts.</b> A quad split into two triangles has a diagonal seam that is not a
real edge. A cylinder sliding across it generates a contact whose normal points along the seam, and
the character catches on geometry that is visually flat. This is the classic character-controller
bug, and it is entirely an artifact of triangulating.</li>
<li><b>Contact count.</b> Up to 12 contacts per box, most of them redundant, all of them competing
inside the relaxation loop. A direct test emits exactly one.</li>
<li><b>Grounding quality.</b> <code>ClassifyGrounding</code> reads contact normals. One clean face
normal classifies a ramp correctly; a fan of triangle normals plus seam normals does not.</li>
</ul>
<p>So <code>UnityWorldProbe</code> should dispatch <code>BoxCollider</code> to a dedicated test and
leave the triangle path to <code>MeshCollider</code>, where it is genuinely needed.</p>
<div class="note">The test below is exact for every axis-aligned box, exact for every box yawed about
world Y &mdash; which together is most of a hand-built level &mdash; and a bounded, safe-direction
approximation only for genuinely tilted boxes. The last page says precisely where the bound
sits.</div>"""

    P1B = """<h1>Building the box</h1>
<p>Take the rotation and the extents separately. The frame must stay <b>orthonormal</b>: if scale
leaks into the axes, the transform below stops being rigid and the cylinder arrives in box space as
an elliptic cylinder, which none of the math that follows is written for.</p>
<pre><span class="k">public struct</span> Obb
{
    <span class="k">public</span> Vector3 Center;                 <span class="c">// world</span>
    <span class="k">public</span> Vector3 AxisX, AxisY, AxisZ;    <span class="c">// orthonormal, rotation only</span>
    <span class="k">public</span> Vector3 HalfExtents;            <span class="c">// world units; scale lives here</span>
}

<span class="k">public static</span> Obb FromUnity(UBoxCollider b)
{
    Transform t = b.transform;
    Quaternion q = t.rotation;
    Vector3 s = t.lossyScale;
    <span class="k">return new</span> Obb {
        Center = t.TransformPoint(b.center),
        AxisX = q * Vector3.right, AxisY = q * Vector3.up, AxisZ = q * Vector3.forward,
        HalfExtents = 0.5f * <span class="k">new</span> Vector3(b.size.x * Mathf.Abs(s.x),
                                          b.size.y * Mathf.Abs(s.y),
                                          b.size.z * Mathf.Abs(s.z)),
    };
}</pre>
<div class="note">A rotated child under a non-uniformly scaled parent is a sheared box, and neither
this struct nor PhysX's own <code>BoxCollider</code> can represent shear &mdash; Unity approximates it
the same way. The approximation is inherited rather than introduced, but it is worth knowing before
someone scales a rotated platform on one axis and wonders why the collision sits slightly off the
render mesh.</div>

<h2>Into box space</h2>
<p>Because the frame is orthonormal, moving into it is a rigid motion: the box becomes an AABB at the
origin, and the cylinder stays a cylinder, merely tilted.</p>
<pre>Vector3 dW = c.Center - box.Center;
Vector3 p  = <span class="k">new</span> Vector3(Vector3.Dot(dW, box.AxisX),      <span class="c">// cylinder centre, box space</span>
                        Vector3.Dot(dW, box.AxisY),
                        Vector3.Dot(dW, box.AxisZ));

<span class="c">// World +Y expressed in box space. This is a row of the rotation matrix, not a</span>
<span class="c">// transform: dot(worldY, AxisX) is just AxisX.y. Unit length for free.</span>
Vector3 u = <span class="k">new</span> Vector3(box.AxisX.y, box.AxisY.y, box.AxisZ.y);</pre>
<p><code>u</code> is the whole problem in one vector: it is where the cylinder's axis ends up once the
box has been straightened out.</p>"""

    P2 = """<h1>Two tiers, decided by one number</h1>
<div class="fig">""" + dia_tiers + """</div>
<p>A cylinder is a surface of revolution about its own axis, so <b>rotating the box about world Y
changes nothing about the cylinder</b>. If <code>|u.y| &#8776; 1</code> then the box's own Y axis is
world Y (up to a flip, which an AABB is symmetric under), the cylinder is still upright in box space,
and the exact closed-form <code>CylinderVsAabb</code> applies unchanged. Only the normal has to be
rotated back out.</p>
<pre><span class="k">const float</span> TILT_EPSILON = 1e-4f;   <span class="c">// |u.y| this close to 1 == no tilt</span>

<span class="k">static bool</span> YawOnly(<span class="k">in</span> Cylinder c, <span class="k">in</span> Obb box, Vector3 p, <span class="k">out</span> Contact contact)
{
    Cylinder local = c; local.Center = p;
    Aabb aabb = <span class="k">new</span> Aabb { Center = Vector3.zero, HalfExtents = box.HalfExtents };
    <span class="k">if</span> (!CylinderVsAabb(local, aabb, <span class="k">out</span> contact)) <span class="k">return false</span>;
    contact.Normal = ToWorld(contact.Normal, box);
    contact.Point  = box.Center + ToWorld(contact.Point, box);
    <span class="k">return true</span>;
}

<span class="k">static</span> Vector3 ToWorld(<span class="k">in</span> Vector3 v, <span class="k">in</span> Obb b)
    =&gt; v.x * b.AxisX + v.y * b.AxisY + v.z * b.AxisZ;</pre>
<p>This is worth more than it looks. Rotated walls, pillars, doorframes and yawed platforms are the
overwhelming majority of non-axis-aligned boxes in a hand-built level, and every one of them keeps
the exact test at the cost of one extra transform. Only genuine tilt &mdash; a ramp, a fallen slab
&mdash; reaches the general path.</p>"""

    P3 = """<h1>The projection radius</h1>
<div class="fig">""" + dia_projection + """</div>
<p>Once the cylinder is tilted, the disc-times-interval factorization is gone and there is no minimum
translation vector to read straight off. What survives is the fact the whole narrowphase is built on:
<b>a cylinder is a segment Minkowski-summed with a disc</b>, and extents along an axis add under
Minkowski sum. So for any unit axis <code>n</code> the cylinder's half-extent stays closed form
&mdash; the segment contributes <code>h&#183;|n&#183;u|</code>, the disc contributes
<code>r&#183;&#8730;(1&#8722;(n&#183;u)&#178;)</code>:</p>
<pre>R_cyl(n) = h&#183;|n&#183;u| + r&#183;&#8730;(1 &#8722; (n&#183;u)&#178;)      <span class="c">// cylinder, any orientation</span>
R_box(n) = e.x&#183;|n.x| + e.y&#183;|n.y| + e.z&#183;|n.z|   <span class="c">// an AABB in this frame</span></pre>
<p>which makes <code>n</code> a separating axis exactly when
<code>|p&#183;n| &gt; R_box(n) + R_cyl(n)</code>. Both endpoints sanity-check: at
<code>n&#183;u = &#177;1</code> the cylinder's extent is <code>h</code>, and at
<code>n&#183;u = 0</code> it is <code>r</code>.</p>
<div class="note">Note what is <i>not</i> being claimed. The identity is exact for every axis. What
follows is a finite <i>choice</i> of axes, and that choice is where the approximation enters.</div>"""

    P4 = """<h1>The axis set</h1>
<p>Separating-axis testing needs every axis normal to a face of the configuration-space obstacle
<code>box &#8853; cylinder</code>. Four families are finite and cheap:</p>
<table>
<tr><th style="width:30%">family</th><th class="m" style="width:27%">axes</th><th>what it catches, and how it simplifies</th></tr>
<tr><td>box face normals</td><td class="m">X, Y, Z</td><td>face contact; <code>R_box</code> collapses to one extent</td></tr>
<tr><td>cylinder caps</td><td class="m">u</td><td>standing on the box; <code>n&#183;u = 1</code>, so <code>R_cyl = h</code></td></tr>
<tr><td>box edge &#215; side wall</td><td class="m">u&#215;X, u&#215;Y, u&#215;Z</td><td>edge vs the curved wall; <code>n&#183;u = 0</code>, so <code>R_cyl = r</code></td></tr>
<tr><td>nearest-feature azimuth</td><td class="m">perp&#8337;(q &#8722; p)</td><td>the completion below; also <code>n&#183;u = 0</code></td></tr>
</table>
<p>The fourth family is not optional, and the reason is easy to miss. The side wall's normals form a
<b>continuous</b> family &mdash; every direction perpendicular to <code>u</code> &mdash; and the first
three families sample roughly three azimuths of it. A box sitting at 45&#176; between two box axes
then has no sampled axis pointing at it:</p>
<div class="note warn">Cylinder <code>r = 1, h = 0.5</code> at the origin; a small box centred at
<code>(0.742, 0.4, 0.742)</code>. Its radial distance is <code>1.049 &gt; r</code>, so the two are
genuinely apart &mdash; yet <code>X</code> and <code>Z</code> each see only <code>0.742</code> against
a wall extent of <code>1</code>, <code>u</code> sees <code>0.4</code> against a cap extent of
<code>0.5</code>, and the three cross products merely reproduce <code>X</code> and <code>Z</code>.
Seven axes, no separation found, phantom contact. The completion axis separates it by
<code>0.048</code>.</div>
<p>The fix is one axis: clamp the cylinder's centre to the box to get the nearest point, then take
the component of that offset perpendicular to <code>u</code>. That is the azimuth the wall actually
needs, and it is the same clamp the AABB test already performs.</p>
<pre>Vector3 q = <span class="k">new</span> Vector3(Mathf.Clamp(p.x, -e.x, e.x),   <span class="c">// nearest point on the box</span>
                       Mathf.Clamp(p.y, -e.y, e.y),
                       Mathf.Clamp(p.z, -e.z, e.z));
Vector3 w = q - p;
Vector3 radial = w - u * Vector3.Dot(w, u);              <span class="c">// strip the axial part</span></pre>"""

    P5 = """<h1>One axis, one test</h1>
<p>Bundling the invariants into a small struct keeps the axis list on the next page to one line per
axis, and keeps it allocation-free &mdash; this runs inside the substep loop.</p>
<pre><span class="k">struct</span> Sat
{
    <span class="k">public</span> Vector3 P;        <span class="c">// cylinder centre, box space</span>
    <span class="k">public</span> Vector3 E;        <span class="c">// box half extents</span>
    <span class="k">public</span> Vector3 U;        <span class="c">// world +Y in box space</span>
    <span class="k">public float</span>   R, H;
    <span class="k">public float</span>   BestDepth;
    <span class="k">public</span> Vector3 BestAxis;
}

<span class="c">// Returns false the moment this axis separates the pair. Otherwise it keeps the</span>
<span class="c">// running minimum, which on exit is the minimum translation vector.</span>
<span class="k">static bool</span> Test(<span class="k">ref</span> Sat s, Vector3 n)
{
    <span class="k">float</span> lenSq = n.sqrMagnitude;
    <span class="k">if</span> (lenSq &lt; EpsilonSq) <span class="k">return true</span>;   <span class="c">// degenerate: carries no information,</span>
    n /= Mathf.Sqrt(lenSq);                  <span class="c">// and normalizing it would be pure noise</span>

    <span class="k">float</span> nu   = Vector3.Dot(n, s.U);
    <span class="k">float</span> rCyl = s.H * Mathf.Abs(nu) + s.R * Mathf.Sqrt(Mathf.Max(0f, 1f - nu * nu));
    <span class="k">float</span> rBox = s.E.x * Mathf.Abs(n.x) + s.E.y * Mathf.Abs(n.y) + s.E.z * Mathf.Abs(n.z);

    <span class="k">float</span> dist  = Vector3.Dot(s.P, n);
    <span class="k">float</span> depth = (rBox + rCyl) - Mathf.Abs(dist);
    <span class="k">if</span> (depth &lt;= 0f) <span class="k">return false</span>;         <span class="c">// separating axis -- disjoint, done</span>

    <span class="k">if</span> (depth &lt; s.BestDepth)
    {
        s.BestDepth = depth;
        s.BestAxis  = dist &gt;= 0f ? n : -n;   <span class="c">// B toward A, per the Contact convention</span>
    }
    <span class="k">return true</span>;
}</pre>
<p>The <code>Mathf.Max(0f, ...)</code> inside the square root is not decoration. For a near-parallel
axis <code>nu</code> can land a few ulps past 1, and <code>Sqrt</code> of a small negative is
<code>NaN</code> propagating straight into a contact normal and from there into the solver, where it
poisons a body's position permanently.</p>"""

    P6 = """<h1>Assembling the contact</h1>
<pre><span class="k">public static bool</span> CylinderVsObb(<span class="k">in</span> Cylinder c, <span class="k">in</span> Obb box, <span class="k">out</span> Contact contact)
{
    contact = <span class="k">default</span>;

    Vector3 dW = c.Center - box.Center;
    Vector3 p  = <span class="k">new</span> Vector3(Vector3.Dot(dW, box.AxisX),
                            Vector3.Dot(dW, box.AxisY),
                            Vector3.Dot(dW, box.AxisZ));
    Vector3 u  = <span class="k">new</span> Vector3(box.AxisX.y, box.AxisY.y, box.AxisZ.y);

    <span class="k">if</span> (1f - Mathf.Abs(u.y) &lt; TILT_EPSILON)
        <span class="k">return</span> YawOnly(c, box, p, <span class="k">out</span> contact);          <span class="c">// the exact path</span>

    Sat s = <span class="k">new</span> Sat { P = p, E = box.HalfExtents, U = u,
                      R = c.Radius, H = c.HalfHeight, BestDepth = <span class="k">float</span>.MaxValue };

    <span class="c">// 1. box face normals</span>
    <span class="k">if</span> (!Test(<span class="k">ref</span> s, Vector3.right) || !Test(<span class="k">ref</span> s, Vector3.up)
                                   || !Test(<span class="k">ref</span> s, Vector3.forward)) <span class="k">return false</span>;
    <span class="c">// 2. the cylinder's own axis</span>
    <span class="k">if</span> (!Test(<span class="k">ref</span> s, u)) <span class="k">return false</span>;
    <span class="c">// 3. box edge x side-wall generator; u x X is (0, u.z, -u.y), and so on</span>
    <span class="k">if</span> (!Test(<span class="k">ref</span> s, <span class="k">new</span> Vector3(0f, u.z, -u.y)) ||
        !Test(<span class="k">ref</span> s, <span class="k">new</span> Vector3(-u.z, 0f, u.x)) ||
        !Test(<span class="k">ref</span> s, <span class="k">new</span> Vector3(u.y, -u.x, 0f))) <span class="k">return false</span>;
    <span class="c">// 4. the nearest-feature azimuth</span>
    Vector3 q = <span class="k">new</span> Vector3(Mathf.Clamp(p.x, -s.E.x, s.E.x),
                           Mathf.Clamp(p.y, -s.E.y, s.E.y),
                           Mathf.Clamp(p.z, -s.E.z, s.E.z));
    Vector3 w = q - p;
    <span class="k">if</span> (!Test(<span class="k">ref</span> s, w - u * Vector3.Dot(w, u))) <span class="k">return false</span>;

    <span class="c">// Nothing separated them, so BestAxis is the shortest exit we found.</span>
    contact.A = c.Body;
    contact.B = <span class="k">null</span>;                                  <span class="c">// Unity world: infinite mass</span>
    contact.Normal = ToWorld(s.BestAxis, box);
    contact.Depth  = s.BestDepth;
    contact.Point  = box.Center + ToWorld(q, box);        <span class="c">// debug visualisation only</span>
    <span class="k">return true</span>;
}</pre>
<p>The <code>||</code> chains short-circuit, which is exactly right: once one axis separates the pair
the remaining axes carry no information and the function is already returning <code>false</code>.</p>"""

    P7 = """<h1>What this is exact about, and what it is not</h1>
<p>The four families cover every <i>flat</i> face of the configuration-space obstacle. What they do
not cover are the curved patches it grows where the cylinder's <b>rim circles</b> sweep against box
vertices and edges. Those carry a continuum of normals, and no finite axis list reaches them.</p>
<p>The consequence is bounded, and bounded in the safe direction. A missing axis can only cause SAT
to <b>fail to find</b> a separation that exists &mdash; never the reverse.</p>
<div class="cols"><div>
<h3>Can happen</h3>
<ul><li>a phantom contact when a cap rim passes very near a tilted box's edge</li>
<li>a depth slightly larger than the true minimum</li>
<li>tilted-box edges that feel faintly rounded</li></ul>
</div><div>
<h3>Cannot happen</h3>
<ul><li>a missed overlap</li>
<li>tunnelling</li>
<li>the character sinking into a box</li></ul>
</div></div>
<p>If it ever becomes visible, the vertex half closes exactly and still without iterating: for each
box vertex <code>v</code>, add the axis from <code>v</code> to its nearest point on the near rim
circle &mdash; and the nearest point on a circle to a point is a plain radial projection, closed
form. Eight more axes. Only rim-versus-box-<i>edge</i> resists, because that one is a genuine
quartic. It is the single place in Phym where the no-iteration rule would have to bend, and it is not
worth bending for a character 0.32 units wide.</p>

<h2>Wiring it in</h2>
<ul>
<li><code>UnityWorldProbe.Generate</code> dispatches <code>UBoxCollider</code> here instead of to the
12-triangle path. <code>MeshCollider</code> is unchanged.</li>
<li>Build order step 6 shrinks: boxes no longer wait on <code>MeshTriangleCache</code>, so this can
land alongside step 4 and give the solver tilted geometry to chew on much earlier.</li>
<li>The broadphase already did the cheap reject &mdash; <code>Physics.OverlapBoxNonAlloc</code>
handed us this collider &mdash; so there is no AABB pre-test to add here.</li>
<li><code>Contact.B == null</code> keeps the infinite-mass path, and the normal convention is the
same as everywhere else: from B toward A.</li>
</ul>

<h2>Tests worth writing</h2>
<ol>
<li>Box yawed 45&#176; about Y: assert the fast path is taken, and that the contact matches
<code>CylinderVsAabb</code> run in box space to within float noise.</li>
<li>The counterexample above, as a regression test: assert <code>CylinderVsObb</code> returns
<code>false</code>. Delete the fourth axis family and watch it fail.</li>
<li>Stand on a 20&#176; ramp: one contact, normal equal to the ramp's up axis,
<code>IsGrounded</code> true. Then 60&#176;: still one contact, <code>IsGrounded</code> false, the
character slides.</li>
<li>Walk the length of a tilted slab: the normal must stay constant. Any flicker means competing axes
are winning intermittently &mdash; the failure the triangle path produced by construction.</li>
<li>Drop a cylinder onto a tilted box's top edge and let it settle. It must come to rest rather than
buzz between the face axis and the edge axis.</li>
<li>A box scaled to near zero on one axis: <code>Test</code> must reject the degenerate axes rather
than emit <code>NaN</code>.</li>
</ol>"""

    return [P1, P1B, P2, P3, P4, P5, P6, P7]
