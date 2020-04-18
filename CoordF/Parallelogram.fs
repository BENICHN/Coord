module Parallelogram

open Coord
open System.Windows
open System.Windows.Media

type plgm = vec2 * base2

module plgm =

    let fromvertices o a b = (o, (a - o, b - o))

    let normbase ((_, bas) : plgm) = base2.norm bas
    
    let contains p ((o, bas) : plgm) =
        let x, y = base2.coordinates bas (p - o)
        x >= 0.0 && x <= 1.0 && y >= 0.0 && y <= 1.0
    
    let sectorcontains p ((o, (u, v)) : plgm) =
        let op = p - o
        if op .* op <= max (u .* u) (v .* v) then
            let x, y = base2.coordinates (u, v) op
            x >= 0.0 && x <= 1.0 && y >= 0.0 && y <= 1.0
        else false
    
    let translate t ((o, (u, v)) : plgm) = o + t, (u, v)
    let rotate c a ((o, (u, v)) : plgm) =
        let o2 = vec2.rotate a (o - c)
        let u2, v2 = vec2.rotate a u, vec2.rotate a v
        (o2 + c, (u2, v2))
    
    let sortinbase bas ((o, (u, v)) : plgm) =
        let mmx ux vx = match sign ux, sign vx with
                        | -1, -1 -> o + u + v, o
                        | 1, 1 -> o, o + u + v
                        | su, sv -> if su < sv then o + u, o + v else o + v, o + u
        let ux, uy = base2.coordinates bas u
        let vx, vy = base2.coordinates bas v
        let l, r = mmx ux vx
        let b, t = mmx uy vy
        l, b, r, t
        
    let sort = sortinbase (base2.ij)
    
    let minibox ((o, (u, v)) : plgm) =
        let l, b, r, t = sort (o, (u, v))
        l.x, b.y, r.x, t.y
    
    let treesarroundplgm data =
        let xs, ys, xe, ye = minibox data
        seq { for x = int (floor xs) + 1 to int (ceil xe) - 1 do
                            for y = int (floor ys) + 1 to int (ceil ye) - 1 do
                                { x = float x; y = float y} }

    let treesinplgm data = data |> treesarroundplgm |> Seq.choose (fun tree -> if contains tree data then Some tree else None)
    let treesinsector data = data |> treesarroundplgm |> Seq.choose (fun tree -> if sectorcontains tree data then Some tree else None)
    
    let containstrees data = data |> treesinplgm |> Seq.isEmpty |> not
    
    let bycsm (csm : ReadOnlyCoordinatesSystemManager) ((o, (u, v)) : plgm) =
        let o2 = Point(o.x, o.y) |*> csm
        let u2 = csm.ComputeOutCoordinates(Vector(u.x, u.y))
        let v2 = csm.ComputeOutCoordinates(Vector(v.x, v.y))
        { x = o2.X ; y = o2.Y }, ({ x = u2.X ; y = u2.Y }, { x = v2.X ; y = v2.Y })
    
    let geometry ((o, (u, v)) : plgm) =
        let a, b, c = o + u, o + v, o + u + v
        let sr = StreamGeometry()
        use ctxt = sr.Open()
        ctxt.BeginFigure(Point(o.x, o.y), true, true)
        ctxt.LineTo(Point(a.x, a.y), true, true)
        ctxt.LineTo(Point(c.x, c.y), true, true)
        ctxt.LineTo(Point(b.x, b.y), true, true)
        sr
    
    let translateOrNot tr ((o, (u, v)) : plgm) =
        let l, b, r, t = sortinbase (base2.orthdir tr) (o, (u, v))
        if treesinplgm (b, (tr, t - b)) |> Seq.isEmpty then
            let newdata = translate tr (o, (u, v))
            if treesinplgm newdata |> Seq.isEmpty then Some newdata
            else None
        else None
    
    let rotateOrNot cr ang ((o, (u, v)) : plgm) =
        let a, b, c = o + u, o + v, o + u + v
        let (op, (up, vp)) = rotate cr ang (o, (u, v))
        let ap, bp, cp = op + up, op + vp, op + up + vp
        if treesinplgm (op, (up, vp)) |> Seq.isEmpty 
           && treesinsector (fromvertices cr a ap) |> Seq.isEmpty 
           && treesinsector (fromvertices cr b bp) |> Seq.isEmpty 
           && treesinsector (fromvertices cr c cp) |> Seq.isEmpty 
           && treesinsector (fromvertices cr o op) |> Seq.isEmpty then Some (op, (up, vp))
        else None
    
    let translateOrNotIL step data = translateOrNot (vec2.x (-step)) data
    let translateOrNotIR step data = translateOrNot (vec2.x (step)) data
    let translateOrNotJD step data = translateOrNot (vec2.y (-step)) data
    let translateOrNotJU step data = translateOrNot (vec2.y (step)) data
    
    let translateOrNotUL (step : float) ((o, (u, v)) : plgm) =
        translateOrNot (-step * (vec2.norm u)) (o, (u, v))
    let translateOrNotUR (step : float)((o, (u, v)) : plgm) =
           translateOrNot (step * (vec2.norm u)) (o, (u, v))
    let translateOrNotVD (step : float)((o, (u, v)) : plgm) =
           translateOrNot (-step * (vec2.norm v)) (o, (u, v))
    let translateOrNotVU (step : float) ((o, (u, v)) : plgm) =
           translateOrNot (step * (vec2.norm v)) (o, (u, v))
    
    let rotateOrNotD c step data = rotateOrNot c (step * tau / 360.0) data
    let rotateOrNotH c step data = rotateOrNot c (-step * tau / 360.0) data
    
    let horiz onnext mstep data =
        async {
            let r c step data = rotateOrNotH c step data
            let i step data = translateOrNotUR step data
            let u step data = translateOrNotVU step data

            let rec opn n op data =
                if n = 0 then data
                else match op data with
                     | Some newdata -> opn (n - 1) op newdata
                     | None -> data

            let rec opsn n op data =
                async {
                    let newdata = opn n op data
                    if newdata = data then return data
                    else
                        do! onnext newdata
                        return! opsn n op newdata
                }
    
            let rec ops op step data =
                async {
                    match op step data with
                    | Some newdata ->
                        do! onnext newdata
                        return! ops op step newdata
                    | None -> return data
                }
    
            let opsfast op data =
                let rec opsf step op data =
                    async {
                        if step <= mstep then return data
                        else
                            let! aops = ops op step data
                            return! opsf (step / 2.0) op aops
                    }
                opsf (4096.0 * mstep) op data

            let rsf c = opsfast (r c)
            let isf = opsfast i
            let usf = opsfast u
    
            let rec rbs data = 
                let o, (u, v) = data
                async {
                    let! ars1 = rsf (o + u + v) data
                    if ars1 = data then return! rsf (o + v) data
                    else return ars1
                }

            let rec ris data =
                async {
                    let! ars = rbs data
                    let! ais = isf ars
                    if ais = ars then return ais
                    else return! ris ais
                }
    
            let rec hz ((o, (u, v)) : plgm) =
                async {
                    if v.y <= 0.0 then
                        return (o, (u, v)), true
                    else
                        let! aris = ris (o, (u, v))
                        let! aus = usf aris
                        if aus = aris then 
                            return aus, false
                        else return! hz aus
                }
    
            return! hz data
        }

    let horizmaxwidth height onnext mstep =
        async {
            let rec hmw width =
                async {
                    let data = (vec2.zero, (vec2.x width, vec2.y height))
                    do! onnext data
                    let! _, success = horiz onnext mstep data
                    if success then return width
                    else return! hmw (width - 0.01)
                }
            return! hmw 0.99
        }

