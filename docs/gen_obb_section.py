# Generates the "Cylinder vs oriented box" section, sized A4 to match
# Phym-Collision-Plan.pdf, then appended to it by merge_plan.py.

import math, os, io

S = 150.0
INK, MUTED, RULE = "#1c1917", "#78716c", "#d6d3d1"
CYL_C, BOX_C, CSO_C = "#2563eb", "#ea580c", "#7c3aed"
OK_C, BAD_C = "#15803d", "#dc2626"
LBL = "font-family:'Segoe UI',system-ui,sans-serif"
MONO = "font-family:Consolas,'Cascadia Mono',monospace"


def defs():
    m = []
    for name, col in (("c", CYL_C), ("b", BOX_C), ("k", INK), ("g", OK_C),
                      ("r", BAD_C), ("m", MUTED), ("v", CSO_C)):
        m.append(f'<marker id="ar-{name}" viewBox="0 0 10 10" refX="9" refY="5" '
                 f'markerWidth="6" markerHeight="6" orient="auto-start-reverse">'
                 f'<path d="M0,0 L10,5 L0,10 z" fill="{col}"/></marker>')
    return "<defs>" + "".join(m) + "</defs>"


def txt(x, y, s, size=13, col=INK, anchor="middle", style=LBL, weight=400,
        italic=False, halo=False):
    it = ";font-style:italic" if italic else ""
    hl = (";paint-order:stroke;stroke:#ffffff;stroke-width:3.5px;"
          "stroke-linejoin:round") if halo else ""
    return (f'<text x="{x:.1f}" y="{y:.1f}" text-anchor="{anchor}" '
            f'style="{style};font-size:{size}px;fill:{col};font-weight:{weight}{it}{hl}">{s}</text>')


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


def rect_rot(cx, cy, w, h, deg, col, fill_op=0.10, sw=2.0):
    return (f'<g transform="rotate({deg:.2f} {cx:.1f} {cy:.1f})">'
            f'<rect x="{cx-w/2:.1f}" y="{cy-h/2:.1f}" width="{w:.1f}" height="{h:.1f}" '
            f'fill="{col}" fill-opacity="{fill_op}" stroke="{col}" stroke-width="{sw}"/></g>')


def cylinder(cx, cy, r, h, col, ry=None, fill_op=0.10, deg=0.0):
    ry = ry if ry is not None else max(6.0, r * 0.30)
    top, bot = cy - h, cy + h
    g = (f'<path d="M {cx-r:.1f} {bot:.1f} A {r:.1f} {ry:.1f} 0 0 1 {cx+r:.1f} {bot:.1f}" '
         f'fill="none" stroke="{col}" stroke-width="1.2" stroke-dasharray="3 3" opacity="0.55"/>'
         f'<path d="M {cx-r:.1f} {top:.1f} L {cx-r:.1f} {bot:.1f} '
         f'A {r:.1f} {ry:.1f} 0 0 0 {cx+r:.1f} {bot:.1f} L {cx+r:.1f} {top:.1f} Z" '
         f'fill="{col}" fill-opacity="{fill_op}" stroke="{col}" stroke-width="2"/>'
         f'<ellipse cx="{cx:.1f}" cy="{top:.1f}" rx="{r:.1f}" ry="{ry:.1f}" '
         f'fill="{col}" fill-opacity="{fill_op+0.06:.2f}" stroke="{col}" stroke-width="2"/>')
    if deg:
        return f'<g transform="rotate({deg:.2f} {cx:.1f} {cy:.1f})">{g}</g>'
    return g


def dim(x1, y1, x2, y2, label, col=INK, off=-9, size=11.5, style=LBL, halo=True):
    o = [arrow(x1, y1, x2, y2, col, "m" if col == MUTED else "k", 1.3, both=True)]
    mx, my = (x1 + x2) / 2, (y1 + y2) / 2
    o.append(txt(mx, my + off, label, size, col, "middle", style=style, halo=halo))
    return "".join(o)


# =============================================================================
# Diagram 1 — the two tiers: yaw reduces exactly, tilt does not
# =============================================================================
def dia_tiers():
    o = [defs()]

    # ---- panel A: top-down, box yawed about Y ----
    o.append(txt(30, 26, "TOP-DOWN · box yawed about world Y", 10.5, MUTED, "start", weight=600))
    ax, ay = 150, 140
    o.append(rect_rot(196, 158, 168, 104, 28, BOX_C, 0.10))
    o.append(circ(ax, ay, 46, CYL_C, 0.12))
    o.append(dot(ax, ay, CYL_C, 3))
    # the circle is invariant under the yaw
    o.append(f'<path d="M {ax-64} {ay-22} A 68 68 0 0 1 {ax-30} {ay-62}" fill="none" '
             f'stroke="{MUTED}" stroke-width="1.4" marker-end="url(#ar-m)"/>')
    o.append(txt(ax - 74, ay - 62, "yaw", 11, MUTED, "middle", italic=True))
    o.append(txt(ax, 250, "a circle is invariant under a spin", 11.5, OK_C, weight=600))
    o.append(txt(ax, 266, "about its own axis", 11.5, OK_C, weight=600))
    o.append(txt(ax, 288, "in box space the cylinder is still", 11, MUTED))
    o.append(txt(ax, 303, "upright → reuse CylinderVsAabb, exactly", 11, MUTED))

    o.append(line(360, 20, 360, 310, RULE, 1))

    # ---- panel B: elevation, box tilted about X or Z ----
    o.append(txt(392, 26, "ELEVATION · box tilted about X or Z", 10.5, MUTED, "start", weight=600))
    bx, by = 500, 150
    o.append(rect_rot(556, 176, 190, 76, -24, BOX_C, 0.10))
    o.append(cylinder(bx, by, 40, 56, CYL_C, 12, 0.12))
    o.append(line(bx, by - 90, bx, by + 92, MUTED, 1.2, "4 4"))
    o.append(arrow(bx, by, bx, by - 74, CYL_C, "c", 1.8))
    o.append(txt(bx - 10, by - 66, "u", 12.5, CYL_C, "end", italic=True, weight=600, halo=True))
    # the box's own up axis, no longer world up
    o.append(arrow(600, 200, 600 + 74 * math.sin(math.radians(-24)),
                   200 - 74 * math.cos(math.radians(-24)), BOX_C, "b", 1.8))
    o.append(txt(566, 132, "box Y", 11.5, BOX_C, "middle", italic=True, halo=True))
    o.append(txt(520, 250, "the two frames disagree", 11.5, BAD_C, weight=600))
    o.append(txt(520, 272, "in box space the cylinder is tilted —", 11, MUTED))
    o.append(txt(520, 287, "the disc × interval factorization is gone,", 11, MUTED))
    o.append(txt(520, 302, "so fall through to SAT", 11, MUTED))

    return f'<svg viewBox="0 0 700 318" width="100%">{"".join(o)}</svg>'


# =============================================================================
# Diagram 2 — the projection radius of a tilted cylinder
# =============================================================================
def dia_projection():
    o = [defs()]
    cx, cy = 250, 142
    r, h, th = 54.0, 78.0, 38.0
    t = math.radians(th)
    su, cu = math.sin(t), math.cos(t)

    cap = (cx + h * su, cy - h * cu)                      # top cap centre
    rim = (cap[0] + r * cu, cap[1] + r * su)              # rightmost silhouette point
    axis_y = 300

    o.append(cylinder(cx, cy, r, h, CYL_C, 15, 0.12, deg=th))
    o.append(dot(cx, cy, CYL_C, 3.5))
    o.append(arrow(cx, cy, *cap, CYL_C, "c", 2.0))
    o.append(txt(cap[0] - 14, cap[1] + 30, "u", 13, CYL_C, "end", italic=True,
                 weight=600, halo=True))
    o.append(dot(*cap, CYL_C, 3.5))
    o.append(dot(*rim, CSO_C, 4.5))

    # the axis we are projecting onto
    o.append(line(140, axis_y, 470, axis_y, MUTED, 1.4))
    o.append(arrow(430, axis_y, 470, axis_y, MUTED, "m", 1.4))
    o.append(txt(478, axis_y + 5, "n", 13, MUTED, "start", italic=True, weight=600))

    for px, py in ((cx, cy), cap, rim):
        o.append(line(px, py, px, axis_y, MUTED, 1, "3 3"))
    o.append(dot(cx, axis_y, INK, 3))
    o.append(dot(cap[0], axis_y, CYL_C, 3))
    o.append(dot(rim[0], axis_y, CSO_C, 3))
    o.append(txt(cx - 12, axis_y - 9, "centre", 10.5, MUTED, "end", italic=True))

    # Each contribution gets its own row. The labels are wider than the spans they
    # measure, so stacking them on one line would overlap them into gibberish.
    o.append(dim(cx, axis_y + 22, cap[0], axis_y + 22, "h |n·u|", CYL_C, -7, 11.5))
    o.append(dim(cap[0], axis_y + 50, rim[0], axis_y + 50,
                 "r √(1−(n·u)²)", CSO_C, -7, 11.5))
    o.append(dim(cx, axis_y + 80, rim[0], axis_y + 80, "R_cyl(n)", INK, -7, 12, MONO))

    # the two pieces, spelled out
    o.append(txt(520, 128, "cylinder", 12, INK, "start", weight=600))
    o.append(txt(520, 148, "= segment ⊕ disc", 12, CSO_C, "start", style=MONO))
    o.append(txt(520, 178, "the segment projects to", 10.5, MUTED, "start"))
    o.append(txt(520, 193, "h |n·u|", 11, CYL_C, "start", style=MONO))
    o.append(txt(520, 218, "the disc projects to", 10.5, MUTED, "start"))
    o.append(txt(520, 233, "r √(1−(n·u)²)", 11, CSO_C, "start", style=MONO))
    o.append(txt(520, 260, "and Minkowski sums", 10.5, MUTED, "start", italic=True))
    o.append(txt(520, 275, "project additively", 10.5, MUTED, "start", italic=True))

    return f'<svg viewBox="0 0 700 400" width="100%">{"".join(o)}</svg>'


CSS = """
@page { size: A4; margin: 0.6in 0.62in; }
* { box-sizing: border-box; }
body { margin:0; color:#1c1917; font-family:'Segoe UI',system-ui,sans-serif;
       font-size:10pt; line-height:1.48; -webkit-print-color-adjust:exact;
       print-color-adjust:exact; }
.page { page-break-after: always; height: 10.3in; display:flex; flex-direction:column; }
.page:last-child { page-break-after: auto; }
.fig, pre, .cols, p, .note, ol, ul, table, h1, h2, h3 { flex-shrink: 0; }
h1 { font-size:16pt; margin:0 0 .5rem; font-weight:600; letter-spacing:-.01em; }
h2 { font-size:11.5pt; margin:.75rem 0 .3rem; font-weight:600; }
h3 { font-size:10pt; margin:.6rem 0 .2rem; font-weight:600; color:#44403c; }
p { margin:0 0 .5rem; }
.fig { margin:.35rem 0 .7rem; }
code { font-family:Consolas,'Cascadia Mono',monospace; font-size:9pt;
       background:#f5f5f4; padding:.05em .28em; border-radius:3px; }
pre { font-family:Consolas,'Cascadia Mono',monospace; font-size:8.1pt; line-height:1.5;
      background:#fafaf9; border:1px solid #e7e5e4; border-left:3px solid #7c3aed;
      border-radius:4px; padding:.5rem .7rem; margin:.4rem 0 .6rem; }
pre .c { color:#78716c; }
pre .k { color:#7c3aed; }
.note { border-left:3px solid #d6d3d1; padding:.1rem 0 .1rem .75rem; color:#57534e;
        font-size:9pt; margin:.5rem 0; }
.warn { border-left-color:#dc2626; }
.cols { display:flex; gap:1.3rem; }
.cols > div { flex:1; }
ol, ul { margin:.3rem 0 .5rem; padding-left:1.1rem; }
li { margin-bottom:.22rem; }
.foot { margin-top:auto; padding-top:.45rem; border-top:1px solid #e7e5e4;
        font-size:7.5pt; color:#a8a29e; display:flex; justify-content:space-between; }
table { border-collapse:collapse; width:100%; font-size:9pt; margin:.3rem 0 .5rem; }
th { text-align:left; font-weight:600; color:#57534e; border-bottom:1px solid #d6d3d1;
     padding:.22rem .4rem .22rem 0; }
td { padding:.22rem .4rem .22rem 0; border-bottom:1px solid #f0efee; vertical-align:top; }
td.m, th.m { font-family:Consolas,monospace; font-size:8.3pt; white-space:nowrap; }
"""

FOOT = "Phym collision engine design plan · Rompe-Oros · Unity 6000.3.8f1"


def page(body, n):
    return (f'<div class="page">{body}'
            f'<div class="foot"><span>{FOOT}</span><span>{n}</span></div></div>')


import obb_content

PAGES = obb_content.pages(dia_tiers(), dia_projection())
HTML = f"<style>{CSS}</style>" + "".join(page(b, 23 + i) for i, b in enumerate(PAGES))

out = os.path.join(os.path.dirname(os.path.abspath(__file__)), "obb_section.html")
with io.open(out, "w", encoding="utf-8") as f:
    f.write(HTML)
print("wrote", out, "with", len(PAGES), "page divs")
