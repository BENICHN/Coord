module Parallelogram

open Coord
open System.Windows
open System.Windows.Media

type plgm = vec2 * vec2 * vec2

let plgmbase ((_, u, v) : plgm) = (norm u, norm v)

let plgmcontains ((o, u, v) : plgm) p =
    let x, y = coordinatesinbase u v (p - o)
    x >= 0.0 && x <= 1.0 && y >= 0.0 && y <= 1.0

let plgmtranslate t ((o, u, v) : plgm) = o + t, u, v
let plgmrotate c a ((o, u, v) : plgm) =
    let o2 = rotate a (o - c)
    let u2, v2 = rotate a u, rotate a v
    (o2 + c, u2, v2)

let plgmminibox ((o, u, v) : plgm) =
    let a, b, c = o + u, o + v, o + u + v
    let xs, xe = if sign a.x * sign b.x < 0 then minmax a.x b.x else minmax o.x c.x
    let ys, ye = if sign a.y * sign b.y < 0 then minmax a.y b.y else minmax o.y c.y
    xs, ys, xe, ye

let plgmcontainstrees p =
    let xs, ys, xe, ye = plgmminibox p
    let res = seq { for x = int (floor xs) + 1 to int (ceil xe) - 1 do
                        for y = int (floor ys) + 1 to int (ceil ye) - 1 do
                            plgmcontains p { x = float x; y = float y} }
    Seq.exists (fun r -> r) res

let plgmwithcsm (csm : ReadOnlyCoordinatesSystemManager) ((o, u, v) : plgm) =
    let o2 = csm.ComputeOutCoordinates(Point(o.x, o.y))
    let u2 = csm.ComputeOutCoordinates(Vector(u.x, u.y))
    let v2 = csm.ComputeOutCoordinates(Vector(v.x, v.y))
    { x = o2.X; y = o2.Y }, { x = u2.X; y = u2.Y }, { x = v2.X; y = v2.Y }

let plgmgeometry ((o, u, v) : plgm) =
    let a, b, c = o + u, o + v, o + u + v
    let sr = StreamGeometry()
    use ctxt = sr.Open()
    ctxt.BeginFigure(Point(o.x, o.y), true, true)
    ctxt.LineTo(Point(a.x, a.y), true, true)
    ctxt.LineTo(Point(c.x, c.y), true, true)
    ctxt.LineTo(Point(b.x, b.y), true, true)
    sr
