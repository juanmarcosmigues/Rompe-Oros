# Generates Phym-CylinderVsCylinder.pdf — a visual walkthrough of the
# cylinder-vs-cylinder narrowphase math. All geometry is computed from the
# same world-space values the labels quote, so the picture cannot drift
# from the math it is illustrating.

import math, os

# ---- example shape values (symbolic in the labels, concrete for drawing) ----
rA, hA = 0.35, 0.40
rB, hB = 0.25, 0.25
rSum   = rA + rB          # 0.60  CSO radius
hSum   = hA + hB          # 0.65  CSO half-height

S = 150.0                 # px per world unit

# ---- ink ----
INK   = "#1c1917"
MUTED = "#78716c"
RULE  = "#d6d3d1"
A_C   = "#2563eb"
B_C   = "#ea580c"
CSO_C = "#7c3aed"
OK_C  = "#15803d"
BAD_C = "#dc2626"

LBL  = "font-family:'Segoe UI',system-ui,sans-serif"
MONO = "font-family:Consolas,'Cascadia Mono',monospace"


def defs():
    m = []
    for name, col in (("a", A_C), ("b", B_C), ("c", CSO_C), ("k", INK),
                      ("g", OK_C), ("r", BAD_C), ("m", MUTED)):
        m.append(
            f'<marker id="ar-{name}" viewBox="0 0 10 10" refX="9" refY="5" '
            f'markerWidth="6" markerHeight="6" orient="auto-start-reverse">'
            f'<path d="M0,0 L10,5 L0,10 z" fill="{col}"/></marker>')
    return "<defs>" + "".join(m) + "</defs>"


def txt(x, y, s, size=13, col=INK, anchor="middle", style=LBL, weight=400,
        italic=False, halo=False):
    it = ";font-style:italic" if italic else ""
    # halo = white outline painted under the glyphs, so labels stay legible
    # where they have to sit on top of a filled region
    hl = (";paint-order:stroke;stroke:#ffffff;stroke-width:3.5px;"
          "stroke-linejoin:round") if halo else ""
    return (f'<text x="{x:.1f}" y="{y:.1f}" text-anchor="{anchor}" '
            f'style="{style};font-size:{size}px;fill:{col};font-weight:{weight}{it}{hl}">{s}</text>')


def band(cx, half_w, cy, h, col, fill_op=0.14, w=2.0):
    """A vertical extent drawn as a band rather than a bare line."""
    return (f'<rect x="{cx-half_w:.1f}" y="{cy-h:.1f}" width="{2*half_w:.1f}" '
            f'height="{2*h:.1f}" fill="{col}" fill-opacity="{fill_op}" '
            f'stroke="{col}" stroke-width="{w}"/>')


def arrow(x1, y1, x2, y2, col=INK, mk="k", w=1.6, dash=None, both=False):
    d = f' stroke-dasharray="{dash}"' if dash else ""
    st = f' marker-start="url(#ar-{mk})"' if both else ""
    return (f'<line x1="{x1:.1f}" y1="{y1:.1f}" x2="{x2:.1f}" y2="{y2:.1f}" '
            f'stroke="{col}" stroke-width="{w}"{d} marker-end="url(#ar-{mk})"{st}/>')


def line(x1, y1, x2, y2, col=RULE, w=1.2, dash=None):
    d = f' stroke-dasharray="{dash}"' if dash else ""
    return (f'<line x1="{x1:.1f}" y1="{y1:.1f}" x2="{x2:.1f}" y2="{y2:.1f}" '
            f'stroke="{col}" stroke-width="{w}"{d}/>')


def dot(x, y, col=INK, r=3.5):
    return f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{r}" fill="{col}"/>'


def circ(x, y, r, col, fill_op=0.10, w=2.0, dash=None):
    d = f' stroke-dasharray="{dash}"' if dash else ""
    return (f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{r:.1f}" fill="{col}" '
            f'fill-opacity="{fill_op}" stroke="{col}" stroke-width="{w}"{d}/>')


def cylinder(cx, cy, r, h, col, ry=None, fill_op=0.10):
    """Upright cylinder in elevation: back cap dashed, body, front arc, top cap."""
    ry = ry if ry is not None else max(6.0, r * 0.30)
    top, bot = cy - h, cy + h
    o = []
    # hidden back half of the bottom cap
    o.append(f'<path d="M {cx-r:.1f} {bot:.1f} A {r:.1f} {ry:.1f} 0 0 1 {cx+r:.1f} {bot:.1f}" '
             f'fill="none" stroke="{col}" stroke-width="1.2" stroke-dasharray="3 3" opacity="0.55"/>')
    # body: sides + visible front half of the bottom cap
    o.append(f'<path d="M {cx-r:.1f} {top:.1f} L {cx-r:.1f} {bot:.1f} '
             f'A {r:.1f} {ry:.1f} 0 0 0 {cx+r:.1f} {bot:.1f} L {cx+r:.1f} {top:.1f} Z" '
             f'fill="{col}" fill-opacity="{fill_op}" stroke="{col}" stroke-width="2"/>')
    # top cap
    o.append(f'<ellipse cx="{cx:.1f}" cy="{top:.1f}" rx="{r:.1f}" ry="{ry:.1f}" '
             f'fill="{col}" fill-opacity="{fill_op+0.06:.2f}" stroke="{col}" stroke-width="2"/>')
    return "".join(o)


def tick(x, y, horiz=True, half=5, col=INK, w=1.6):
    if horiz:
        return line(x - half, y, x + half, y, col, w)
    return line(x, y - half, x, y + half, col, w)


def brace_dim(x1, y1, x2, y2, label, col=INK, off=0, size=13, italic=True):
    """Double-headed dimension arrow with a label at its midpoint."""
    o = [arrow(x1, y1, x2, y2, col, "m" if col == MUTED else "k", 1.4, both=True)]
    mx, my = (x1 + x2) / 2, (y1 + y2) / 2
    if abs(y2 - y1) > abs(x2 - x1):     # vertical dimension -> label to the side
        o.append(txt(mx + off, my + 4, label, size, col, "middle", italic=italic))
    else:
        o.append(txt(mx, my - 8 + off, label, size, col, "middle", italic=italic))
    return "".join(o)


# =============================================================================
# PAGE 1 — an upright cylinder factors into a disc and an interval
# =============================================================================
def page1():
    o = [defs()]
    r, h, ry = 52.0, 62.0, 15.0

    # --- panel 1: the cylinder in 3D ---
    cx, cy = 108, 132
    o.append(line(cx, cy - h - ry - 20, cx, cy + h + ry + 20, MUTED, 1.2, "4 4"))
    o.append(cylinder(cx, cy, r, h, INK, ry, 0.05))
    o.append(dot(cx, cy))
    o.append(brace_dim(cx, cy, cx, cy - h, "h", INK, off=12))
    o.append(arrow(cx, cy - h, cx + r, cy - h, INK, "k", 1.4))
    # clear of the top cap's ellipse, which the label otherwise sits on top of
    o.append(txt(cx + r / 2, cy - h - ry - 7, "r", 13, INK, italic=True, halo=True))
    o.append(txt(cx - 12, cy + 5, "c", 13, INK, "end", italic=True))
    o.append(txt(cx, 236, "an upright cylinder", 12.5, MUTED))

    o.append(txt(228, 138, "=", 30, RULE))

    # --- panel 2: the XZ disc ---
    dx, dy = 352, 132
    o.append(circ(dx, dy, r, A_C, 0.12))
    o.append(dot(dx, dy, A_C))
    o.append(arrow(dx, dy, dx + r, dy, A_C, "a", 1.4))
    o.append(txt(dx + r / 2, dy - 8, "r", 13, A_C, italic=True))
    # little axis cross
    o.append(arrow(dx - 82, dy + 74, dx - 52, dy + 74, MUTED, "m", 1.2))
    o.append(arrow(dx - 82, dy + 74, dx - 82, dy + 44, MUTED, "m", 1.2))
    o.append(txt(dx - 45, dy + 78, "x", 11, MUTED, "middle", italic=True))
    o.append(txt(dx - 86, dy + 42, "z", 11, MUTED, "end", italic=True))
    o.append(txt(dx, 236, "XZ plane — a disc", 12.5, MUTED))

    o.append(txt(474, 138, "×", 26, RULE))

    # --- panel 3: the Y interval ---
    ix, iy = 594, 132
    o.append(line(ix, iy - h - 26, ix, iy + h + 26, MUTED, 1.2, "4 4"))
    o.append(line(ix, iy - h, ix, iy + h, B_C, 4.5))
    o.append(tick(ix, iy - h, True, 11, B_C, 2.5))
    o.append(tick(ix, iy + h, True, 11, B_C, 2.5))
    o.append(dot(ix, iy, B_C))
    o.append(brace_dim(ix + 34, iy, ix + 34, iy - h, "h", B_C, off=12))
    o.append(brace_dim(ix + 34, iy, ix + 34, iy + h, "h", B_C, off=12))
    o.append(txt(ix - 16, iy + 5, "c.y", 12, B_C, "end", italic=True))
    o.append(arrow(ix - 44, iy + 30, ix - 44, iy - 30, MUTED, "m", 1.2))
    o.append(txt(ix - 48, iy - 34, "y", 11, MUTED, "end", italic=True))
    o.append(txt(ix, 236, "Y axis — an interval", 12.5, MUTED))

    return f'<svg viewBox="0 0 700 250" width="100%">{"".join(o)}</svg>'


# =============================================================================
# PAGE 2 — Minkowski sum: two cylinders collapse into one
# =============================================================================
def page2():
    o = [defs()]

    ang = math.radians(25)
    dxz = 0.48                       # |d.xz|, less than rSum so they overlap
    dY  = 0.30                       # d.y,   less than hSum so they overlap
    px, py = dxz * S * math.cos(ang), -dxz * S * math.sin(ang)

    # ---------------- row 1: the XZ plane ----------------
    y1 = 118
    o.append(txt(38, y1 - 88, "XZ", 12, MUTED, "start", weight=600))

    bx, by = 152, y1
    ax, ay = bx + px, by + py
    o.append(circ(bx, by, rB * S, B_C, 0.12))
    o.append(circ(ax, ay, rA * S, A_C, 0.12))
    o.append(dot(bx, by, B_C)); o.append(dot(ax, ay, A_C))
    o.append(arrow(bx, by, ax, ay, INK, "k", 2.0))
    # label sits perpendicular to the arrow so it clears both circle outlines
    o.append(txt((bx + ax) / 2 - 7, (by + ay) / 2 - 13, "d", 14, INK,
                 italic=True, weight=600, halo=True))
    o.append(txt(ax + 4, ay - rA * S - 9, "A", 13, A_C, weight=600))
    o.append(txt(bx - rB * S - 10, by + 5, "B", 13, B_C, "end", weight=600))

    o.append(arrow(300, y1, 372, y1, RULE, "m", 1.6))
    o.append(txt(336, y1 - 14, "shrink A to a point,", 11, MUTED))
    o.append(txt(336, y1 + 26, "grow B by A", 11, MUTED))

    cx, cy = 528, y1
    o.append(circ(cx, cy, rSum * S, CSO_C, 0.10))
    o.append(circ(cx, cy, rB * S, B_C, 0.0, 1.2, "3 3"))
    o.append(dot(cx, cy, CSO_C))
    o.append(dot(cx + px, cy + py, INK, 4.5))
    o.append(arrow(cx, cy, cx + px, cy + py, INK, "k", 2.0))
    o.append(txt(cx + px + 14, cy + py - 8, "d", 14, INK, italic=True, weight=600))
    o.append(arrow(cx, cy, cx + rSum * S, cy, CSO_C, "c", 1.4))
    o.append(txt(cx + rSum * S / 2, cy + 19, "r&#8320; + r&#8321;", 12.5, CSO_C,
                 italic=True, halo=True))

    o.append(line(40, 238, 660, 238, RULE, 1))

    # ---------------- row 2: the Y axis ----------------
    # Extents drawn as bands, not bare lines, so the two rows read as the same
    # kind of object seen in two projections.
    y2 = 352
    o.append(txt(38, y2 - 96, "Y", 12, MUTED, "start", weight=600))

    ivA_x, ivB_x, hw = 132, 208, 21
    ayc = y2 - dY * S

    o.append(band(ivA_x, hw, ayc, hA * S, A_C))
    o.append(dot(ivA_x, ayc, A_C))
    o.append(txt(ivA_x - hw - 10, ayc + 4, "A", 12.5, A_C, "end", weight=600))

    o.append(band(ivB_x, hw, y2, hB * S, B_C))
    o.append(dot(ivB_x, y2, B_C))
    o.append(txt(ivB_x, y2 - hB * S - 10, "B", 12.5, B_C, weight=600))

    # centre lines + the d.y offset between them
    o.append(line(ivA_x - hw - 4, ayc, 262, ayc, A_C, 1, "4 3"))
    o.append(line(ivB_x - hw - 4, y2, 262, y2, B_C, 1, "4 3"))
    o.append(brace_dim(258, y2, 258, ayc, "d.y", INK, off=22, size=12))

    o.append(arrow(300, y2, 372, y2, RULE, "m", 1.6))
    o.append(txt(336, y2 - 14, "same trick,", 11, MUTED))
    o.append(txt(336, y2 + 26, "one dimension", 11, MUTED))

    o.append(band(cx, hw + 4, y2, hSum * S, CSO_C, 0.12))
    o.append(dot(cx, y2, CSO_C))
    o.append(dot(cx, ayc, INK, 4.5))
    o.append(arrow(cx, y2, cx, ayc, INK, "k", 2.0))
    o.append(txt(cx + hw + 12, ayc + 4, "d.y", 13, INK, "start",
                 italic=True, weight=600, halo=True))
    o.append(brace_dim(cx - hw - 30, y2, cx - hw - 30, y2 - hSum * S,
                       "h&#8320; + h&#8321;", CSO_C, off=-36))

    return f'<svg viewBox="0 0 700 486" width="100%">{"".join(o)}</svg>'


# =============================================================================
# PAGE 3 — the test collapses to point-in-cylinder
# =============================================================================
def page3():
    o = [defs()]
    ox, oy = 350, 212                      # CSO centre (the origin of d)
    R, H = rSum * S, hSum * S              # 90, 97.5
    fx0, fx1, fy0, fy1 = 62, 638, 40, 384  # diagram frame

    o.append(f'<rect x="{fx0}" y="{fy0}" width="{fx1-fx0}" height="{fy1-fy0}" '
             f'fill="{BAD_C}" fill-opacity="0.05" stroke="{RULE}" stroke-width="1"/>')
    o.append(f'<rect x="{ox-R:.1f}" y="{oy-H:.1f}" width="{2*R:.1f}" height="{2*H:.1f}" '
             f'fill="{CSO_C}" fill-opacity="0.13" stroke="{CSO_C}" stroke-width="2"/>')

    o.append(line(fx0, oy, fx1, oy, MUTED, 1, "4 4"))
    o.append(line(ox, fy0, ox, fy1, MUTED, 1, "4 4"))
    o.append(dot(ox, oy, CSO_C))
    o.append(txt(ox - 9, oy + 15, "0", 11.5, MUTED, "end"))

    o.append(txt(fx1 - 8, oy + 20, "radial distance in XZ  →", 11.5, MUTED, "end",
                 italic=True, halo=True))
    o.append(txt(ox + 10, fy0 + 16, "↑  d.y", 11.5, MUTED, "start", italic=True))

    # axis ticks: radial labels below the axis, height labels to the RIGHT of it,
    # which keeps them clear of the two rejected sample points on the left/top.
    o.append(tick(ox + R, oy, False, 6, CSO_C, 2))
    o.append(txt(ox + R, oy + 20, "r&#8320;+r&#8321;", 11.5, CSO_C, italic=True, halo=True))
    o.append(tick(ox - R, oy, False, 6, CSO_C, 2))
    o.append(txt(ox - R, oy + 20, "−(r&#8320;+r&#8321;)", 11.5, CSO_C, italic=True, halo=True))
    o.append(tick(ox, oy - H, True, 6, CSO_C, 2))
    o.append(txt(ox + 12, oy - H - 8, "h&#8320;+h&#8321;", 11.5, CSO_C, "start",
                 italic=True, halo=True))
    o.append(tick(ox, oy + H, True, 6, CSO_C, 2))
    o.append(txt(ox + 12, oy + H + 17, "−(h&#8320;+h&#8321;)", 11.5, CSO_C, "start",
                 italic=True, halo=True))

    o.append(txt(ox - R + 12, oy + H - 30, "the CSO", 13, CSO_C, "start", weight=600))
    o.append(txt(ox - R + 12, oy + H - 14, "cylinder, in cross-section", 11, CSO_C, "start"))

    # sample points
    p_in  = (ox + 0.33 * S, oy - 0.28 * S)
    p_rad = (ox + 1.05 * S, oy - 0.62 * S)
    p_ver = (ox - 0.55 * S, oy - 0.98 * S)

    o.append(arrow(ox, oy, *p_in, OK_C, "g", 1.8))
    o.append(dot(*p_in, OK_C, 5))
    o.append(txt(p_in[0] + 14, p_in[1] - 14, "d", 13, OK_C, "start",
                 italic=True, weight=600, halo=True))
    o.append(txt(p_in[0] + 28, p_in[1] + 4, "inside both → contact", 11.5, OK_C,
                 "start", weight=600, halo=True))

    o.append(dot(*p_rad, BAD_C, 5))
    o.append(txt(p_rad[0] - 12, p_rad[1] + 5, "d", 13, BAD_C, "end", italic=True, weight=600))
    o.append(txt(p_rad[0], p_rad[1] - 28, "past the side wall", 11.5, BAD_C))
    o.append(txt(p_rad[0], p_rad[1] - 12, "distXZSq &#8805; radiusSum&#178;", 11, BAD_C, style=MONO))

    o.append(dot(*p_ver, BAD_C, 5))
    o.append(txt(p_ver[0] + 12, p_ver[1] + 5, "d", 13, BAD_C, "start", italic=True, weight=600))
    o.append(txt(p_ver[0], p_ver[1] + 26, "past the cap", 11.5, BAD_C))
    o.append(txt(p_ver[0], p_ver[1] + 42, "penY &#8804; 0", 11, BAD_C, style=MONO))

    o.append(txt(fx0 + 12, fy1 - 14, "outside the CSO — no overlap, return false",
                 11, BAD_C, "start"))
    return f'<svg viewBox="0 0 700 396" width="100%">{"".join(o)}</svg>'


# =============================================================================
# PAGE 4 — the minimum translation vector: whichever exit is shorter
# =============================================================================
def page4():
    o = [defs()]
    ox, oy = 170, 180
    R, H = rSum * S, hSum * S

    rho, yy = 0.42, 0.15                # a point where the side exit is shorter
    penXZ, penY = rSum - rho, hSum - yy # 0.18 and 0.50
    dx_, dy_ = ox + rho * S, oy - yy * S

    o.append(f'<rect x="{ox-R:.1f}" y="{oy-H:.1f}" width="{2*R:.1f}" height="{2*H:.1f}" '
             f'fill="{CSO_C}" fill-opacity="0.10" stroke="{CSO_C}" stroke-width="2"/>')
    o.append(line(ox - R - 26, oy, ox + R + 26, oy, MUTED, 1, "4 4"))
    o.append(line(ox, oy - H - 26, ox, oy + H + 26, MUTED, 1, "4 4"))
    o.append(dot(ox, oy, CSO_C, 3))

    o.append(dot(dx_, dy_, INK, 5))
    o.append(txt(dx_ - 10, dy_ + 16, "d", 14, INK, "end", italic=True, weight=600))

    # the two real exits, plus the rim that never wins
    o.append(arrow(dx_, dy_, ox + R, dy_, OK_C, "g", 2.4))
    o.append(txt(ox + R + 8, dy_ + 4, "penXZ", 12, OK_C, "start", weight=600, style=MONO))
    o.append(arrow(dx_, dy_, dx_, oy - H, B_C, "b", 2.0))
    o.append(txt(dx_ - 9, (dy_ + oy - H) / 2, "penY", 12, B_C, "end",
                 weight=600, style=MONO, halo=True))
    o.append(arrow(dx_, dy_, ox + R, oy - H, MUTED, "m", 1.4, dash="4 3"))
    o.append(txt(dx_ + 8, dy_ - 46, "rim", 11, MUTED, "start", italic=True, halo=True))

    o.append(txt(ox, oy + H + 32, "shortest way out of the CSO", 12, INK, weight=600))
    o.append(txt(ox, oy + H + 49, "= the exact minimum translation vector", 11.5, MUTED))
    o.append(txt(ox, oy + H + 69, "the rim is √(penXZ² + penY²) — never shorter than either leg",
                 10.5, MUTED, italic=True))

    # ---- outcome insets ----
    o.append(line(372, 36, 372, 330, RULE, 1))

    # side contact (top view)
    sx_, sy_ = 470, 120
    o.append(txt(400, 50, "penXZ &lt; penY  →  side contact", 12, OK_C, "start",
                 weight=600, style=MONO))
    o.append(circ(sx_, sy_, 38, B_C, 0.12))
    o.append(circ(sx_ + 58, sy_ - 14, 46, A_C, 0.12))
    o.append(dot(sx_, sy_, B_C, 3)); o.append(dot(sx_ + 58, sy_ - 14, A_C, 3))
    o.append(arrow(sx_, sy_, sx_ + 58, sy_ - 14, OK_C, "g", 2.2))
    o.append(txt(590, sy_ - 8, "normal is radial,", 11, MUTED, "start"))
    o.append(txt(590, sy_ + 7, "flat in XZ", 11, MUTED, "start"))
    o.append(txt(590, sy_ + 24, "they slide past", 11, MUTED, "start", italic=True))

    # cap contact (elevation)
    o.append(txt(400, 186, "penY &lt; penXZ  →  cap contact", 12, B_C, "start",
                 weight=600, style=MONO))
    o.append(cylinder(470, 274, 40, 24, B_C, 11, 0.10))
    o.append(cylinder(476, 232, 34, 20, A_C, 10, 0.10))
    o.append(arrow(545, 268, 545, 214, B_C, "b", 2.2))
    o.append(txt(590, 228, "normal is ±Y", 11, MUTED, "start"))
    o.append(txt(590, 243, "one stands on", 11, MUTED, "start"))
    o.append(txt(590, 260, "the other's head", 11, MUTED, "start", italic=True))

    return f'<svg viewBox="0 0 700 348" width="100%">{"".join(o)}</svg>'


# =============================================================================
CSS = """
@page { size: Letter; margin: 0.5in 0.6in; }
* { box-sizing: border-box; }
body { margin:0; color:#1c1917; font-family:'Segoe UI',system-ui,sans-serif;
       font-size:10.5pt; line-height:1.5; -webkit-print-color-adjust:exact;
       print-color-adjust:exact; }
.page { page-break-after: always; height: 10in; display:flex; flex-direction:column; }
.page:last-child { page-break-after: auto; }
.kicker { font-size:8.5pt; letter-spacing:.14em; text-transform:uppercase;
          color:#a8a29e; font-weight:600; margin-bottom:.28rem; }
h1 { font-size:17pt; margin:0 0 .1rem; font-weight:600; letter-spacing:-.01em; }
.sub { color:#78716c; font-size:10pt; margin:0 0 1.1rem; }
.fig { margin: .2rem 0 1.1rem; }
/* In a flex column, `overflow:hidden` sets min-height to 0 and lets an item be
   squeezed below its content height. Pin every block so nothing gets clipped. */
.fig, pre, .cols, p, .note, .eq, h2 { flex-shrink: 0; }
p { margin:0 0 .6rem; }
.lead { font-size:11pt; }
code { font-family:Consolas,'Cascadia Mono',monospace; font-size:9.5pt;
       background:#f5f5f4; padding:.06em .3em; border-radius:3px; }
pre { font-family:Consolas,'Cascadia Mono',monospace; font-size:9pt; line-height:1.55;
      background:#fafaf9; border:1px solid #e7e5e4; border-left:3px solid #7c3aed;
      border-radius:4px; padding:.6rem .8rem; margin:.5rem 0 .7rem; overflow:hidden; }
pre .c { color:#78716c; }
pre .k { color:#7c3aed; }
.note { border-left:3px solid #d6d3d1; padding:.1rem 0 .1rem .8rem; color:#57534e;
        font-size:9.5pt; margin:.6rem 0; }
.cols { display:flex; gap:1.4rem; }
.cols > div { flex:1; }
.foot { margin-top:auto; padding-top:.5rem; border-top:1px solid #e7e5e4;
        font-size:8pt; color:#a8a29e; display:flex; justify-content:space-between; }
h2 { font-size:11pt; margin:.4rem 0 .35rem; font-weight:600; }
.eq { text-align:center; font-family:Consolas,'Cascadia Mono',monospace;
      font-size:10.5pt; color:#7c3aed; margin:.7rem 0 .9rem; }
"""


def page(kicker, title, sub, svg, body, n):
    return f"""<div class="page">
<div class="kicker">{kicker}</div><h1>{title}</h1><p class="sub">{sub}</p>
<div class="fig">{svg}</div>{body}
<div class="foot"><span>Phym · cylinder vs cylinder</span><span>{n} / 4</span></div></div>"""


HTML = f"""<style>{CSS}</style>
{page("Step 1 · the shape", "An upright cylinder is two independent tests",
      "Everything downstream depends on this one property.", page1(), """
<p class="lead">A cylinder whose axis is locked to Y factors into a <b>disc in XZ</b> and an
<b>interval in Y</b>. A point is inside the cylinder if and only if <i>both</i> hold — and the two
tests never consult each other.</p>
<div class="eq">p &#8712; cylinder &#8660; p.xz &#8712; Disc(c.xz, r) &nbsp;<b>and</b>&nbsp; p.y &#8712; Interval(c.y &#177; h)</div>
<p>That independence is the whole reason the rest of this is closed-form. It survives only while the
axis stays upright: tilt either cylinder and the disc smears into an ellipse whose shape depends on
the tilt, the factorization dies, and you are back to iterating with GJK/EPA.</p>
<div class="note">This is why <i>upright cylinders</i> is a locked decision and not a
simplification to revisit later — the exactness of every formula on the following pages is
downstream of it.</div>""", 1)}

{page("Step 2 · the trick", "Minkowski sum: two cylinders become one",
      "Shrink A to a point, grow B by A. Shape-vs-shape becomes point-in-shape.", page2(), """
<p class="lead">Rather than test two moving shapes, collapse A to a single point and inflate B by
A's shape. The inflated shape is the <b>configuration-space obstacle</b>, and the question becomes
whether the centre-to-centre vector <code>d</code> lands inside it.</p>
<div class="eq">(D&#8320; × I&#8320;) &#8853; (D&#8321; × I&#8321;) &nbsp;=&nbsp; (D&#8320; &#8853; D&#8321;) × (I&#8320; &#8853; I&#8321;)</div>
<div class="cols"><div>
<h2>Disc &#8853; disc</h2>
<p>A disc of radius <code>rA + rB</code> — the familiar result that two circles overlap exactly when
their centres are closer than the sum of their radii.</p>
</div><div>
<h2>Interval &#8853; interval</h2>
<p>An interval of half-length <code>hA + hB</code>. Sum the half-heights, keep the axis.</p>
</div></div>
<p>Because the sum distributes over that product, the CSO is <b>another upright cylinder</b>:
radius <code>rA + rB</code>, half-height <code>hA + hB</code>, centred at the origin of
<code>d</code>. No approximation entered anywhere.</p>""", 2)}

{page("Step 3 · the test", "Point in cylinder — two early-outs",
      "The cross-section of the CSO, with d plotted against it.", page3(), """
<pre><span class="c">// Radial test in XZ. Both penetration depths below are exact -- the</span>
<span class="c">// configuration-space obstacle is a cylinder of radius rA+rB and</span>
<span class="c">// height hA+hB, so there is nothing to iterate toward.</span>
float radiusSum = a.Radius + b.Radius;
float distXZSq  = d.x * d.x + d.z * d.z;
<span class="k">if</span> (distXZSq &gt;= radiusSum * radiusSum) <span class="k">return false</span>;   <span class="c">// past the side wall</span>

float penY = (a.HalfHeight + b.HalfHeight) - Mathf.Abs(d.y);
<span class="k">if</span> (penY &lt;= 0f) <span class="k">return false</span>;                          <span class="c">// past the caps</span></pre>
<p>One early-out per factor. <code>Mathf.Abs(d.y)</code> because the CSO is symmetric about zero — it
does not care whether A sits above or below B.</p>
<div class="cols"><div>
<h2>Why compare squares</h2>
<p>&#8730; is monotonic, so <code>&#8730;u &#8805; &#8730;v &#8660; u &#8805; v</code>. Comparing squares
gives the identical answer and skips the square root on the rejection path — the common path. The
<code>Sqrt</code> is paid only after the test passes, where the normal needs it anyway.</p>
</div><div>
<h2>Why <code>&gt;=</code> and not <code>&gt;</code></h2>
<p>Exact touching counts as <i>not</i> colliding. With <code>&gt;</code>, tangent cylinders emit a
contact of depth&nbsp;0 — a constraint that does no work, consumes a solver iteration, and feeds a
zero-length vector into the normalization below.</p>
</div></div>""", 3)}

{page("Step 4 · the answer", "The shortest exit, and why it is exact",
      "Distance from an interior point to the boundary of a cylinder.", page4(), """
<p class="lead">For a point inside at radial distance <i>&#961;</i> and height <i>y</i>, there are only
two candidate exits — and a third that never wins.</p>
<div class="cols"><div>
<p><code>penXZ = R &#8722; &#961;</code> &nbsp;out through the side wall<br>
<code>penY&nbsp; = H &#8722; |y|</code> &nbsp;out through the nearer cap<br>
<span style="color:#78716c">to the rim: <code>&#8730;(penXZ² + penY²)</code></span></p>
</div><div>
<p>The rim distance is a hypotenuse, so it is <b>&#8805; both legs</b> — always. The nearest boundary
point is therefore on the wall or on a cap, never on the rim, and
<code>min(penXZ, penY)</code> <i>is</i> the true minimum translation vector.</p>
</div></div>
<pre><span class="k">if</span> (penY &lt; penXZ) { <span class="c">// cap contact: normal is straight up or down</span>
    contact.Normal = <span class="k">new</span> Vector3(0f, d.y &gt;= 0f ? 1f : -1f, 0f);
    contact.Depth  = penY;
} <span class="k">else if</span> (distXZ &gt; Epsilon) { <span class="c">// side contact: along the horizontal axis</span>
    contact.Normal = <span class="k">new</span> Vector3(d.x / distXZ, 0f, d.z / distXZ);
    contact.Depth  = penXZ;
} <span class="k">else</span> { <span class="c">// centres exactly stacked -- pick a stable arbitrary direction</span>
    contact.Normal = Vector3.right;
    contact.Depth  = penXZ;
}</pre>
<div class="note">This is what GJK/EPA exist to approximate for arbitrary convex shapes: EPA expands a
polytope <i>iteratively</i> toward the CSO surface, converging to within a tolerance. Here the CSO is
a cylinder you can write down, so the depth is a subtraction and the branch is a comparison — no
tolerance, no iteration count, no convergence failure.</div>""", 4)}
"""

out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "cylcyl.html")
with open(out, "w", encoding="utf-8") as f:
    f.write(HTML)
print("wrote", out)
