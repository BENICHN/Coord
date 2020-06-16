namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.Standard
open BenLib.Framework
open BenLib.WPF
open Coord
open Parallelogram
open System.Threading

module Tronc =
    
    type troncdata =
        {
            hinfo : (int * float * bool) option
            indata : plgm
            dims : float * float
        }
        
    let formattroncdata { hinfo = hi; indata = ({ x = ox ; y = oy }, ({ x = xu ; y = yu }, { x = xv ; y = yv })); dims = w, h } =
        [
            sprintf "w = %.9f\r\nh = %.9f" w h ;
            match hi with
            | Some (n, nw, pl) -> sprintf "h = %f\r\nPartie %d [√%d, √%d]\r\n%s\r\nwmax = %f" h n (if n = 0 then 0 else int plgm.b008784.[n-1]) (int plgm.b008784.[n]) (if pl then "Palier" else "Transition") nw
            | None -> "Pas d'infos" ;
            sprintf "o = (%.9f, %.9f)\r\nu = (%.9f, %.9f)\r\nv = (%.9f, %.9f)\r\n tan = %.9f" ox oy xu yu xv yv (yv / xv)
        ] |> List.fold (fun acc s -> acc + nl + s + nl + "-----------------------------------------") "" |> fun s -> s.TrimStart ('\r', '\n')
    
    type Tronc() =
        inherit VisualObject()
    
        static let WidthProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Width", 1.0, dict [ ("min", box 0.0); ("max", box 1.0) ])
        static let HeightProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Height", 1.0, dict [ ("min", box 0.0) ])
        static let HeightsProperty = nobj.CreateProperty<Tronc, Range<decimal>>(true, true, true, "Heights", Range<decimal>.EmptySet)
        static let WStartProperty = nobj.CreateProperty<Tronc, decimal>(true, true, true, "WStart", 0.999999M)
        static let WStepProperty = nobj.CreateProperty<Tronc, decimal>(true, true, true, "WStep", 0.000001M)
        static let HStepProperty = nobj.CreateProperty<Tronc, decimal>(true, true, true, "HStep", 0.000001M)
        static let MLogProperty = nobj.CreateProperty<Tronc, int>(true, true, true, "MLog", -6, dict [ ("min", box -12); ("max", box 0) ])
        static let TranslationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "TranslationStep", 0.01, dict [ ("min", box 0.0) ])
        static let RotationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "RotationStep", 1.0, dict [ ("min", box 0.0); ("max", box 360.0) ])
        static let RotationCenterProperty = nobj.CreateProperty<Tronc, RectPoint>(true, true, true, "RotationCenter", RectPoint.Center)
        static let MiniboxProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Minibox", true)
        static let TreesProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Trees", true)
        static let TrackProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Track", true)
        static let SlowProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Slow", false)
        static let ReverseProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Reverse", false)
        static let PhantomProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Phantom", false)
        static let ZNProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "ZN", false)
        static let MTHProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "MTH", false)
        static let ClrProperty = nobj.CreateProperty<Tronc, NotifyObjectCollection<NotifyObjectTuple<int, int, Brush>>>(true, true, true, "Clr")
    
        let mutable data = { hinfo = None ; indata = plgm.init 1.0 1.0 ; dims = 1.0, 1.0 }
    
        static do        
            nobj.OverrideDefaultValue<Tronc, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
            nobj.OverrideDefaultValue<Tronc, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 1.0))
    
        member this.Width
            with get() = this.GetValue(WidthProperty) :?> float
            and set(value : float) = this.SetValue(WidthProperty, value)
        member this.Height
            with get() = this.GetValue(HeightProperty) :?> float
            and set(value : float) = this.SetValue(HeightProperty, value)
        member this.Heights
            with get() = this.GetValue(HeightsProperty) :?> Range<decimal>
            and set(value : Range<decimal>) = this.SetValue(HeightsProperty, value)
        member this.WStart
            with get() = this.GetValue(WStartProperty) :?> decimal
            and set(value : decimal) = this.SetValue(WStartProperty, value)
        member this.WStep
            with get() = this.GetValue(WStepProperty) :?> decimal
            and set(value : decimal) = this.SetValue(WStepProperty, value)
        member this.HStep
            with get() = this.GetValue(HStepProperty) :?> decimal
            and set(value : decimal) = this.SetValue(HStepProperty, value)
        member this.MLog
            with get() = this.GetValue(MLogProperty) :?> int
            and set(value : int) = this.SetValue(MLogProperty, value)
        member this.TranslationStep
            with get() = this.GetValue(TranslationStepProperty) :?> float
            and set(value : float) = this.SetValue(TranslationStepProperty, value)
        member this.RotationStep
            with get() = this.GetValue(RotationStepProperty) :?> float
            and set(value : float) = this.SetValue(RotationStepProperty, value)
        member this.RotationCenter
            with get() = this.GetValue(RotationCenterProperty) :?> RectPoint
            and set(value : RectPoint) = this.SetValue(RotationCenterProperty, value)
        member this.Minibox
            with get() = this.GetValue(MiniboxProperty) :?> bool
            and set(value : bool) = this.SetValue(MiniboxProperty, value)
        member this.Trees
            with get() = this.GetValue(TreesProperty) :?> bool
            and set(value : bool) = this.SetValue(TreesProperty, value)
        member this.Track
            with get() = this.GetValue(TrackProperty) :?> bool
            and set(value : bool) = this.SetValue(TrackProperty, value)
        member this.Slow
            with get() = this.GetValue(SlowProperty) :?> bool
            and set(value : bool) = this.SetValue(SlowProperty, value)
        member this.Reverse
            with get() = this.GetValue(ReverseProperty) :?> bool
            and set(value : bool) = this.SetValue(ReverseProperty, value)
        member this.Phantom
            with get() = this.GetValue(PhantomProperty) :?> bool
            and set(value : bool) = this.SetValue(PhantomProperty, value)
        member this.ZN
            with get() = this.GetValue(ZNProperty) :?> bool
            and set(value : bool) = this.SetValue(ZNProperty, value)
        member this.MTH
            with get() = this.GetValue(MTHProperty) :?> bool
            and set(value : bool) = this.SetValue(MTHProperty, value)
        member this.Clr
            with get() = this.GetValue(ClrProperty) :?> NotifyObjectCollection<NotifyObjectTuple<int, int, Brush>>
            and set(value : NotifyObjectCollection<NotifyObjectTuple<int, int, Brush>>) = this.SetValue(ClrProperty, value)
        member this.Data
            with get() = this.GetValue(VisualObject.DataProperty) :?> troncdata
            and set(value : troncdata) = this.SetValue(VisualObject.DataProperty, value)

        member this.CEqs (bb, ll, tt, rr) =
            let clr = this.Clr
            if clr = null then []
            else
                [
                    for t in clr do
                        let a, b, brush = t.Item1, t.Item2, t.Item3
                        let brush = if brush = null then FlatBrushes.Nephritis :> Brush else brush
                        if a = 0 && b = 0 then yield [], brush
                        else
                            let d = gcd a b
                            let a, b = a / d, b / d
                            let a, b = if b < 0 then -a, -b else a, b
                            let eq i = (-b, a, i)
                            if a = 0 then yield [ ll .. rr ] |> List.map eq, brush
                            elif b = 0 then yield [ bb .. tt ] |> List.map ((~-) >> eq), brush
                            else
                                let bb, ll, tt, rr = double bb, double ll, double tt, double rr
                                let a, b = double a, double b
                                let xt = tt * a / b
                                let xb = bb * a / b
                                let kl = b * (ll - if a > 0.0 then xt else xb) |> floor |> int
                                let kr = b * (rr - if a > 0.0 then xb else xt) |> ceil |> int
                                yield [ kl .. kr ] |> List.map eq, brush
                ]
    
        member this.RCenter =
            let rp = this.RotationCenter
            let (o, (u, v)) = data.indata
            o + rp.XProgress * u + rp.YProgress * v
    
        override __.CreateInstanceCore() = Tronc() :> Freezable
        override __.Type = "Tronc"
    
        member private this.Apply ndo =
            match ndo with
            | Some newdata ->
                this.Data <- { data with indata = newdata }
            | None -> ()
    
        member private this.OnNext track uictxt =
            if track then 
                fun newdata -> 
                    async {
                        do! Async.SwitchToContext uictxt
                        let! _ = Async.AwaitEvent (CompositionTarget.Rendering)
                        this.Data <- { data with indata = newdata }
                        do! Async.SwitchToThreadPool ()
                    } 
            else fun newdata -> async { data <- { data with indata = newdata } }
    
        member private this.OpTK hz = plgm.opstk this.MLog (this.OnNext this.Track SynchronizationContext.Current) hz this.Slow
    
        member this.TranslateOrNot v = this.Apply (plgm.translateOrNot v data.indata)
        member this.RotateOrNot a = this.Apply (plgm.rotateOrNot (this.RCenter) a data.indata)
    
        member this.TranslateOrNotIL () = if this.Phantom then this.Data <- { data with indata = plgm.translateIL (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotIL (this.TranslationStep) data.indata)
        member this.TranslateOrNotIR () = if this.Phantom then this.Data <- { data with indata = plgm.translateIR (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotIR (this.TranslationStep) data.indata)
        member this.TranslateOrNotJD () = if this.Phantom then this.Data <- { data with indata = plgm.translateJD (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotJD (this.TranslationStep) data.indata)
        member this.TranslateOrNotJU () = if this.Phantom then this.Data <- { data with indata = plgm.translateJU (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotJU (this.TranslationStep) data.indata)
                                                                                                                                                
        member this.TranslateOrNotUL () = if this.Phantom then this.Data <- { data with indata = plgm.translateUL (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotUL (this.TranslationStep) data.indata)
        member this.TranslateOrNotUR () = if this.Phantom then this.Data <- { data with indata = plgm.translateUR (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotUR (this.TranslationStep) data.indata)
        member this.TranslateOrNotVD () = if this.Phantom then this.Data <- { data with indata = plgm.translateVD (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotVD (this.TranslationStep) data.indata)
        member this.TranslateOrNotVU () = if this.Phantom then this.Data <- { data with indata = plgm.translateVU (this.TranslationStep) data.indata } else this.Apply (plgm.translateOrNotVU (this.TranslationStep) data.indata)
        
        member this.RotateOrNotD () = if this.Phantom then this.Data <- { data with indata = plgm.rotateD (this.RCenter) (this.RotationStep) data.indata } else this.Apply (plgm.rotateOrNotD (this.RCenter) (this.RotationStep) data.indata)
        member this.RotateOrNotH () = if this.Phantom then this.Data <- { data with indata = plgm.rotateH (this.RCenter) (this.RotationStep) data.indata } else this.Apply (plgm.rotateOrNotH (this.RCenter) (this.RotationStep) data.indata)
    
        member this.TranslateOrNotILF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotIL) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotIRF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotIR) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotJDF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotJD) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotJUF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotJU) data.indata) |> Async.Ignore |> Async.Start
                                                                                                                                                  
        member this.TranslateOrNotULF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotUL) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotURF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotUR) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotVDF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotVD) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotVUF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotVU) data.indata) |> Async.Ignore |> Async.Start
                                                                                                                                                  
        member this.TranslateOrNotILM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotIL) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotIRM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotIR) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotJDM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotJD) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotJUM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotJU) data.indata) |> Async.Ignore |> Async.Start
                                                                                                                                                  
        member this.TranslateOrNotULM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotUL) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotURM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotUR) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotVDM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotVD) data.indata) |> Async.Ignore |> Async.Start
        member this.TranslateOrNotVUM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotVU) data.indata) |> Async.Ignore |> Async.Start
    
        member this.RotateOrNotDF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.rotateOrNotD (this.RCenter)) data.indata) |> Async.Ignore |> Async.Start
        member this.RotateOrNotHF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.rotateOrNotH (this.RCenter)) data.indata) |> Async.Ignore |> Async.Start
    
        member this.Horiz() = 
            let optk = this.OpTK true
            async { 
                let!  _, success = plgm.horiz2 optk true data.indata
                if success then MessageBox.Show "Terminé horizontal" |> ignore
                else MessageBox.Show "Terminé" |> ignore
            } |> Async.Start
    
        member this.HS() = 
            let optk = this.OpTK true
            plgm.horiz2 optk false data.indata |> Async.Ignore |> Async.Start
    
        member this.NCS() =
            let w, h = data.dims
            let r = this.RotationStep
            ThreadPool.QueueUserWorkItem (new WaitCallback (fun _ -> let s, a = plgm.ncss w h r 0.0 90.0 in if s then MessageBox.Show "N" |> ignore else MessageBox.Show (sprintf "NN : %f : tan = %f" a (tan (a * tau / 360.0))) |> ignore))
    
        member this.HorizMaxWidth() = 
            let _, h = data.dims
            let wstep = this.WStep
            let wstart = this.WStart
            let optk = this.OpTK true
            async { 
                let! width = plgm.horizmaxwidth1 h optk wstep wstart
                MessageBox.Show (width.ToString ()) |> ignore
            } |> Async.Start
    
        member this.HorizsMaxWidths() =
            let heights = this.Heights
            let hmin, hmax = heights.Start.Value, heights.End.Value
            let hstep = this.HStep
            let wstep = this.WStep
            let wstart = this.WStart
            let optk = this.OpTK true
            plgm.horizmaxwidths hmin hmax hstep optk wstep wstart |> Async.Ignore |> Async.Start
    
        member this.Tans() =
            let heights = this.Heights
            let hmin, hmax = heights.Start.Value, heights.End.Value
            let hstep = this.HStep
            let optk = this.OpTK true
            plgm.tans hmin hmax hstep optk |> Async.Ignore |> Async.Start
    
        member this.Resetpos() =
            let w, h = data.dims
            this.Data <- { data with indata = plgm.init w h }
    
        override this.GetCharactersCore(csm) =
            let fill = this.Fill
            let stroke = this.Stroke
            let inr = csm.InputRange
            let c = this.RCenter
            let xs, ys, xe, ye = plgm.minibox data.indata
            let m = this.Minibox
            let t = this.Trees
            let r = this.Reverse
            let z = this.ZN
            let w, h = data.dims
            let bottom, left, top, right = inr.Bottom, inr.Left, inr.Top, inr.Right
            let bb, ll, tt, rr = int <| floor bottom, int <| floor left, int <| ceil top, int <| ceil right
            let trees = [ for i in  ll .. rr do for j in  bb .. tt do yield double i, double j ]
            seq {
                if r then
                    let l = w * w + h * h
                    let bb, ll, tt, rr = int <| floor bottom - l, int <| floor left - l, int <| ceil top + l, int <| ceil right + l
                    let trees = [ for i in  ll .. rr do for j in  bb .. tt do yield { x = double i ; y = double j } ]
                    let ndata = data.indata |> plgm.scale c -1.0 -1.0
                    yield! trees |> List.map (fun tree -> (ndata |> plgm.translate (tree - c) |> plgm.bycsm csm |> plgm.geometry).ToCharacter(FlatBrushes.Alizarin))
                    yield! trees |> List.map (fun tree -> Character.Ellipse(Point(tree.x, tree.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.Pomegranate, 1.0)).WithData 0)
                    yield Character.Ellipse(Point(c.x, c.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))
                else
                    if t then yield! trees |> List.map (fun (i, j) -> Character.Ellipse(Point(float i, float j) |*> csm, 5.0, 5.0).Color(FlatBrushes.Alizarin))
                    yield (data.indata |> plgm.bycsm csm |> plgm.geometry).ToCharacter(fill, stroke)
                    if m then yield Character.Rectangle(csm.ComputeOutCoordinates(Rect(Point(xs, ys), Point(xe, ye)))).Color(Pen(FlatBrushes.Alizarin, 1.0))
                    yield Character.Ellipse(Point(c.x, c.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))
                if z then
                    let (_, ({ vec2.x = xu ; y = yu }, _)) = data.indata
                    let a = -360.0 / tau * atan2 yu xu
                    let s, us = plgm.ncs w h 1.0 a a csm
                    if s then yield! us |> List.map (fun g -> Character (g, FlatBrushes.Nephritis, new Pen (FlatBrushes.Nephritis, 1.0)))
                    yield! [ ((0.0, 0.5), (1.0, 0.5)) ; ((0.5, 0.0), (0.5, 1.0)) ] |> List.map (fun ((sx, sy), (ex, ey)) -> Character.Line(Point (sx, sy) |*> csm, Point (ex, ey) |*> csm).Color(Pen (Brushes.YellowGreen, 1.0)))
                yield! this.CEqs (bb, ll, tt, rr) |> List.collect (fun (eqs, brush) -> eqs |> List.map (fun (a, b, c) -> let struct (p1, p2) = LineVisualObject.GetEndpoints(new LinearEquation(a |> double, b |> double, c |> double), csm) in Character.Line(p1 |*> csm, p2 |*> csm).Color(new Pen(brush, 1.0))))
            } |> Seq.sortBy (fun ch -> ch.Data <> null)
    
        override this.OnPropertyChanged(e : DependencyPropertyChangedEventArgs) =
            if e.Property = WidthProperty then
                let nw = e.NewValue :?> float
                let w, h = data.dims
                if nw <> w && nw > 0.0 then
                    let o, (u, v) = data.indata
                    let nindata = (o, (vec2.relength nw u, v))
                    if not this.Phantom && plgm.containstrees nindata then this.Width <- e.OldValue :?> float else this.Data <- { data with indata = nindata; dims = nw, h }
            elif e.Property = HeightProperty then
                let nh = e.NewValue :?> float
                let w, _ = data.dims
                if nh > 0.0 then
                    let o, (u, v) = data.indata
                    let mth = this.MTH
                    let hinfo = plgm.mthop nh
                    let nindata, nw = match hinfo with
                                      | Some (_, nw, _) when mth -> (o, (vec2.relength nw u, vec2.relength nh v)), nw
                                      | _ -> (o, (u, vec2.relength nh v)), w
                    if not this.Phantom && plgm.containstrees nindata then
                        this.Height <- e.OldValue :?> float
                    else
                        this.Data <- { data with indata = nindata ; dims = nw, nh ; hinfo = hinfo }
                        if mth then this.Width <- nw
            elif e.Property = PhantomProperty then
                let p = e.NewValue :?> bool
                if not p then
                    if plgm.containstrees data.indata then this.Phantom <- true
            elif e.Property = VisualObject.DataProperty then
                match e.NewValue with
                | :? troncdata as td ->
                    data <- td
                    this.NotifyChanged()
                | _ -> ()
            base.OnPropertyChanged(e)
            
        override __.DataFormat(d) =
            match d with
            | :? troncdata as td -> formattroncdata td
            | _ -> ""
