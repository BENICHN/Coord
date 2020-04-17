module Parallelogram

open Coord
open System.Windows
open System.Windows.Media

type plgm = vec2 * base2

module plgm =

    let normbase ((_, bas) : plgm) = base2.norm bas
    
    let contains p ((o, bas) : plgm) =
        let x, y = base2.coordinates bas (p - o)
        x >= 0.0 && x <= 1.0 && y >= 0.0 && y <= 1.0
    
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
    
    let treesinplgm data =
        let xs, ys, xe, ye = minibox data
        let trees = seq { for x = int (floor xs) + 1 to int (ceil xe) - 1 do
                            for y = int (floor ys) + 1 to int (ceil ye) - 1 do
                                { x = float x; y = float y} }
        trees |> Seq.choose (fun tree -> if contains tree data then Some tree else None)
    
    let containstrees data =
        match treesinplgm data |> Seq.tryHead with
        | Some _ -> true
        | None -> false
    
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
    
    //------------------------------------------------------------------------------------
    
    // let plgmTranslateOrTree v data =
    //     let newdata = translate v data
    //     match treesinplgm newdata |> Seq.tryHead with
    //     | Some tree -> Choice2Of2 tree
    //     | None -> Choice1Of2 newdata
    // 
    // let plgmRotateOrTree c a data =
    //     let newdata = rotate c a data
    //     match treesinplgm newdata |> Seq.tryHead with
    //     | Some tree -> Choice2Of2 tree
    //     | None -> Choice1Of2 newdata
        
    //------------------------------------------------------------------------------------
    
    let translateOrNot tr ((o, (u, v)) : plgm) =
        let l, b, r, t = sortinbase (base2.orthdir tr) (o, (u, v))
        if treesinplgm (b, (tr, u + v)) |> Seq.isEmpty then
            let newdata = translate tr (o, (u, v))
            if treesinplgm newdata |> Seq.isEmpty then Some newdata
            else None
        else None
    
    let rotateOrNot c a data =
        let newdata = rotate c a data
        if treesinplgm newdata |> Seq.isEmpty then Some newdata
        else None
    
    //------------------------------------------------------------------------------------
    
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
    
    //------------------------------------------------------------------------------------
    
    // let plgmTranslateOrTreeIL step data = plgmTranslateOrTree (vec2.x (-step)) data
    // let plgmTranslateOrTreeIR step data = plgmTranslateOrTree (vec2.x (step)) data
    // let plgmTranslateOrTreeJD step data = plgmTranslateOrTree (vec2.y (-step)) data
    // let plgmTranslateOrTreeJU step data = plgmTranslateOrTree (vec2.y (step)) data
    // 
    // let plgmTranslateOrTreeUL (step : float) data =
    //     let u, _ = plgmbase data
    //     plgmTranslateOrTree (-step * u) data
    // let plgmTranslateOrTreeUR (step : float) data =
    //     let u, _ = plgmbase data
    //     plgmTranslateOrTree (step * u) data
    // let plgmTranslateOrTreeVD (step : float) data =
    //     let _, v = plgmbase data
    //     plgmTranslateOrTree (-step * v) data
    // let plgmTranslateOrTreeVU (step : float) data =
    //     let _, v = plgmbase data
    //     plgmTranslateOrTree (step * v) data
    // 
    // let plgmRotateOrTreeD c step data = plgmRotateOrTree c (step * tau / 360.0) data
    // let plgmRotateOrTreeH c step data = plgmRotateOrTree c (-step * tau / 360.0) data
    
    //------------------------------------------------------------------------------------
    
    let horiz onnext mstep data =
        async {
            let r c step data = rotateOrNotH c (10.0 * step) data
            let i step data = translateOrNotUR step data
            let u step data = translateOrNotVU step data
    
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
                opsf (1024.0 * mstep) op data
    
            let rs c = ops (r c) mstep
            let is = ops i mstep
            let us = ops u mstep
    
            // let rsf c = opsfast (r c)
            let isf = opsfast i
            let usf = opsfast u
    
            let rec rbs data = 
                let o, (u, v) = data
                async {
                    let! ars1 = rs (o + u + v) data
                    if ars1 = data then return! rs (o + v) data
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
                        MessageBox.Show "Terminé" |> ignore
                        return o, (u, v)
                    else
                        let! aris = ris (o, (u, v))
                        let! aus = usf aris
                        if aus = aris then return aus
                        else return! hz aus
                }
    
            return! hz data
        }

