module Parallelogram

open Coord
open System.Windows
open System.Windows.Media
open System
open System.IO

type plgm = vec2 * base2

module plgm =

    let init w h = ({ x = (1.0 - w) / 2.0 ; y = -h / 2.0 }, (vec2.x w, vec2.y h))

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

    let miniboxofpoints (points : vec2 list) =
        let xs = points |> List.minBy (fun p -> p.x)
        let ys = points |> List.minBy (fun p -> p.y)
        let xe = points |> List.maxBy (fun p -> p.x)
        let ye = points |> List.maxBy (fun p -> p.y)
        xs, ys, xe, ye
    
    let minibox ((o, (u, v)) : plgm) =
        let l, b, r, t = sort (o, (u, v))
        l.x, b.y, r.x, t.y

    let treesinminibox (xs, ys, xe, ye) =
        seq { for x = int (floor xs) + 1 to int (ceil xe) - 1 do
                for y = int (floor ys) + 1 to int (ceil ye) - 1 do
                    { x = float x; y = float y} }
    
    let treesarroundplgm data = data |> minibox |> treesinminibox

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

    let trajitr tr ((o, (u, v)) : plgm) =
        let _, b, _, t = sortinbase (base2.orthdir tr) (o, (u, v))
        (b, (tr, t - b))

    let trajirotim cr ang ((o, (u, v)) : plgm) =
        let a, b, c = o + u, o + v, o + u + v
        let (op, (up, vp)) = rotate cr ang (o, (u, v))
        let ap, bp, cp = op + up, op + vp, op + up + vp
        (op, (up, vp)), fromvertices cr a ap, fromvertices cr b bp, fromvertices cr c cp, fromvertices cr o op
    
    let translateOrNot tr data =
        if trajitr tr data |> treesinplgm |> Seq.isEmpty then
            let newdata = translate tr data
            if treesinplgm newdata |> Seq.isEmpty then Some newdata
            else None
        else None
    
    let rotateOrNot cr ang data =
        let newdata, s1, s2, s3, s4 = trajirotim cr ang data
        if treesinplgm newdata |> Seq.isEmpty 
           && treesinsector s1 |> Seq.isEmpty 
           && treesinsector s2 |> Seq.isEmpty 
           && treesinsector s3 |> Seq.isEmpty 
           && treesinsector s4 |> Seq.isEmpty then Some newdata
        else None

    let translatemax tr ((o, (u, v)) : plgm) =
        let (op, _) = translate tr (o, (u, v))
        let box = miniboxofpoints [ o; o + u; o + v; o + u + v; op; op + u; op + v; op + u + v ]
        ()
    
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
    
    let rotateOrNotAtCenterD step ((o, (u, v)) : plgm) = rotateOrNotD (o + u / 2.0 + v / 2.0) step (o, (u, v))
    let rotateOrNotAtCenterH step ((o, (u, v)) : plgm) = rotateOrNotH (o + u / 2.0 + v / 2.0) step (o, (u, v))

    let opstk mstep onnext =
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
                | Some ((o, (u, v)) : plgm) ->
                    do! onnext (o, (u, v))
                    if v.y <= 0.0 then return (o, (u, v))
                    else return! ops op step (o, (u, v))
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
            opsf (1048576.0 * mstep) op data
            
        let trtomid optr ((o, bas) : plgm) =
            async {
                let! no, _ = opsfast optr (o, bas)
                let res = ((o + no) / 2.0, bas)
                do! onnext res
                return res
            }

        ops, opn, opsn, opsfast, trtomid
    
    let horiz1 onnext mstep data =
            let _, _, _, opsfast, _ = opstk mstep onnext

            let rsf c = opsfast (rotateOrNotH c)
            let isf = opsfast translateOrNotUR
            let usf = opsfast translateOrNotVU
    
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
            hz data
    
    let horiz2 onnext mstep data =
            let _, _, _, opsfast, trtomid = opstk mstep onnext
                
            let rf = opsfast rotateOrNotAtCenterH
            let uf = opsfast translateOrNotUR
            let um = trtomid translateOrNotUL
            let vf = opsfast translateOrNotVU
            let vm = trtomid translateOrNotVD
    
            let rec hz ((o, (u, v)) : plgm) =
                async {
                    if v.y <= 0.0 then
                        return (o, (u, v)), true
                    else
                        let! arf = rf (o, (u, v))
                        let! auf = uf arf
                        let! avf = vf auf
                        let! aum = um avf
                        let! (no, (nu, nv)) = vm aum
                        if u = nu then 
                            return (no, (nu, nv)), false
                        else return! hz (no, (nu, nv))
                }
            hz data

    let horizmaxwidth1 height mstep (wstep : decimal) (wstart : decimal) onnext =
            let rec hmw width =
                async {
                    let! _, success = horiz2 onnext mstep (init (double width) height)
                    if success then return width
                    else let w = width - wstep in return! hmw w
                }
            hmw wstart

    let horizmaxwidth2 height mstep (wstep : decimal) (wstart : decimal) onnext =
            let rec hmw width data =
                async {
                    let! (o, (u, v)), success = horiz2 onnext mstep data
                    if success then return width
                    else let w = width - wstep in return! hmw w (o, (vec2.relength (double w) u, v))
                }
            hmw wstart (init (double wstart) height)

    // let horizmaxwidth height mstep wstep wstart onnext =
    //     let rec hmw wst ws =
    //         async {
    //             let! w = horizmaxwidthenc height mstep wst (min (0.99M) (ws + 10.0M * wst)) onnext
    //             if wst <= wstep then return w
    //             else return! hmw (wst / 10.0M) w
    //         }
    //     hmw 0.1M wstart

    let horizmaxwidths (hstart : decimal) (hend : decimal) (hstep : decimal) mstep (wstep : decimal) (wstart : decimal) onnext =
        let total = int ((hend - hstart) / hstep) + 1
        let rec hmw h ws n acc =
            async {
                let! w = horizmaxwidth1 (double h) mstep wstep ws onnext
                printfn "%f : %f ---- %d / %d" h w n total
                if h >= hend then return (h, w) :: acc
                else return! hmw (h + hstep) w (n + 1) ((h, w) :: acc)
            }
        async {
            printfn "Hauteurs dans [ %f , %f ] avec un pas de %f" hstart hend hstep
            let! res = hmw hstart wstart 1 []
            use fstr = File.Open ("res.txt", FileMode.Create)
            fstr.Flush ()
            use sr = new StreamWriter (fstr :> Stream)
            res
            |> List.map (fun (h, w) -> sprintf "%f : %f" h w)
            |> List.iter (fun s -> sr.WriteLine s)
            return res
        }
        

