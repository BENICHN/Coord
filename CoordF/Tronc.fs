namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.Standard
open BenLib.WPF
open Coord
open Parallelogram
open System.Threading

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

    let mutable data : plgm = plgm.init 1.0 1.0

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
    member __.Data = data

    member this.RCenter =
        let rp = this.RotationCenter
        let (o, (u, v)) = data
        o + rp.XProgress * u + rp.YProgress * v

    override __.CreateInstanceCore() = Tronc() :> Freezable
    override __.Type = "Tronc"

    member private this.Apply ndo =
        match ndo with
        | Some newdata ->
            data <- newdata
            this.NotifyChanged ()
        | None -> ()

    member private this.OnNext track uictxt =
        if track then 
            fun newdata -> 
                async {
                    do! Async.SwitchToContext uictxt
                    let! _ = Async.AwaitEvent (CompositionTarget.Rendering)
                    data <- newdata
                    this.NotifyChanged()
                    do! Async.SwitchToThreadPool ()
                } 
        else fun newdata -> async { data <- newdata }

    member private this.OpTK hz = plgm.opstk this.MLog (this.OnNext this.Track SynchronizationContext.Current) hz this.Slow

    member this.TranslateOrNot v = this.Apply (plgm.translateOrNot v data)
    member this.RotateOrNot a = this.Apply (plgm.rotateOrNot (this.RCenter) a data)

    member this.TranslateOrNotIL () = if this.Phantom then data <- plgm.translateIL (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotIL (this.TranslationStep) data)
    member this.TranslateOrNotIR () = if this.Phantom then data <- plgm.translateIR (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotIR (this.TranslationStep) data)
    member this.TranslateOrNotJD () = if this.Phantom then data <- plgm.translateJD (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotJD (this.TranslationStep) data)
    member this.TranslateOrNotJU () = if this.Phantom then data <- plgm.translateJU (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotJU (this.TranslationStep) data)
                                                  
    member this.TranslateOrNotUL () = if this.Phantom then data <- plgm.translateUL (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotUL (this.TranslationStep) data)
    member this.TranslateOrNotUR () = if this.Phantom then data <- plgm.translateUR (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotUR (this.TranslationStep) data)
    member this.TranslateOrNotVD () = if this.Phantom then data <- plgm.translateVD (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotVD (this.TranslationStep) data)
    member this.TranslateOrNotVU () = if this.Phantom then data <- plgm.translateVU (this.TranslationStep) data; this.NotifyChanged () else this.Apply (plgm.translateOrNotVU (this.TranslationStep) data)
    
    member this.RotateOrNotD () = if this.Phantom then data <- plgm.rotateD (this.RCenter) (this.RotationStep) data; this.NotifyChanged () else this.Apply (plgm.rotateOrNotD (this.RCenter) (this.RotationStep) data)
    member this.RotateOrNotH () = if this.Phantom then data <- plgm.rotateH (this.RCenter) (this.RotationStep) data; this.NotifyChanged () else this.Apply (plgm.rotateOrNotH (this.RCenter) (this.RotationStep) data)

    member this.TranslateOrNotILF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotIL) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotIRF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotIR) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotJDF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotJD) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotJUF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotJU) data) |> Async.Ignore |> Async.Start
                                                                                                                                              
    member this.TranslateOrNotULF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotUL) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotURF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotUR) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotVDF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotVD) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotVUF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.translateOrNotVU) data) |> Async.Ignore |> Async.Start
                                                                                                                                              
    member this.TranslateOrNotILM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotIL) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotIRM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotIR) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotJDM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotJD) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotJUM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotJU) data) |> Async.Ignore |> Async.Start
                                                                                                                                              
    member this.TranslateOrNotULM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotUL) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotURM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotUR) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotVDM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotVD) data) |> Async.Ignore |> Async.Start
    member this.TranslateOrNotVUM () = let _, _, _, _, trtomid = this.OpTK false in (trtomid (plgm.translateOrNotVU) data) |> Async.Ignore |> Async.Start

    member this.RotateOrNotDF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.rotateOrNotD (this.RCenter)) data) |> Async.Ignore |> Async.Start
    member this.RotateOrNotHF () = let _, _, _, opsfast, _ = this.OpTK false in (opsfast (plgm.rotateOrNotH (this.RCenter)) data) |> Async.Ignore |> Async.Start

    member this.Horiz () = 
        let optk = this.OpTK true
        async { 
            let!  _, success = plgm.horiz2 optk data
            if success then MessageBox.Show "Terminé horizontal" |> ignore
            else MessageBox.Show "Terminé" |> ignore
        } |> Async.Start

    member this.NCS () =
        let w = this.Width
        let h = this.Height
        let r = this.RotationStep
        ThreadPool.QueueUserWorkItem (new WaitCallback (fun _ -> let s, a = plgm.ncss w h r 0.0 90.0 in if s then MessageBox.Show "N" |> ignore else MessageBox.Show (sprintf "NN : %f : tan = %f" a (tan (a * tau / 360.0))) |> ignore))

    member this.HorizMaxWidth () = 
        let height = this.Height
        let wstep = this.WStep
        let wstart = this.WStart
        let optk = this.OpTK true
        async { 
            let! width = plgm.horizmaxwidth1 height optk wstep wstart
            MessageBox.Show (width.ToString ()) |> ignore
        } |> Async.Start

    member this.HorizsMaxWidths () =
        let heights = this.Heights
        let hmin, hmax = heights.Start.Value, heights.End.Value
        let hstep = this.HStep
        let wstep = this.WStep
        let wstart = this.WStart
        let optk = this.OpTK true
        plgm.horizmaxwidths hmin hmax hstep optk wstep wstart |> Async.Ignore |> Async.Start

    member this.Resetpos () = data <- plgm.init this.Width this.Height

    override this.GetCharactersCore csm =
        let fill = this.Fill
        let stroke = this.Stroke
        let inr = csm.InputRange
        let c = this.RCenter
        let xs, ys, xe, ye = plgm.minibox data
        let m = this.Minibox
        let t = this.Trees
        let r = this.Reverse
        let z = this.ZN
        let bottom, left, top, right = inr.Bottom, inr.Left, inr.Top, inr.Right
        seq {
            if r then
                let w = this.Width
                let h = this.Height
                let l = w * w + h * h
                let bb, ll, tt, rr = int <| floor bottom - l, int <| floor left - l, int <| ceil top + l, int <| ceil right + l
                let trees = [ for i in  ll .. rr do for j in  bb .. tt do yield { x = double i ; y = double j } ]
                let ndata = data |> plgm.scale c -1.0 -1.0
                yield! trees |> List.map (fun tree -> (ndata |> plgm.translate (tree - c) |> plgm.bycsm csm |> plgm.geometry).ToCharacter(FlatBrushes.Alizarin))
                yield! trees |> List.map (fun tree -> Character.Ellipse(Point(tree.x, tree.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.Pomegranate, 1.0)).WithData 0)
                yield Character.Ellipse(Point(c.x, c.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))
            else
                if t then
                    let bb, ll, tt, rr = int <| floor bottom, int <| floor left, int <| ceil top, int <| ceil right
                    let trees = [ for i in  ll .. rr do for j in  bb .. tt do yield double i, double j ]
                    yield! trees |> List.map (fun (i, j) -> Character.Ellipse(Point(float i, float j) |*> csm, 5.0, 5.0).Color(FlatBrushes.Alizarin))
                yield (data |> plgm.bycsm csm |> plgm.geometry).ToCharacter(fill, stroke)
                if m then yield Character.Rectangle(csm.ComputeOutCoordinates(Rect(Point(xs, ys), Point(xe, ye)))).Color(Pen(FlatBrushes.Alizarin, 1.0))
                yield Character.Ellipse(Point(c.x, c.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))
            if z then
                let (_, ({ vec2.x = xu ; y = yu }, _)) = data
                let a = -360.0 / tau * atan2 yu xu
                let s, us = plgm.ncs this.Width this.Height 1.0 a a csm
                if s then yield! us |> List.map (fun g -> Character (g, FlatBrushes.Nephritis, new Pen (FlatBrushes.Nephritis, 1.0)))
                yield! [ ((0.0, 0.5), (1.0, 0.5)) ; ((0.5, 0.0), (0.5, 1.0)) ] |> List.map (fun ((sx, sy), (ex, ey)) -> Character.Line(Point (sx, sy) |*> csm, Point (ex, ey) |*> csm).Color(Pen (Brushes.YellowGreen, 1.0)))
        } |> Seq.sortBy (fun ch -> ch.Data <> null)

    override this.OnPropertyChanged (e : DependencyPropertyChangedEventArgs) =
        if (e.Property = WidthProperty) then
            let w = e.NewValue :?> float
            if w > 0.0 then
                let o, (u, v) = data
                let newdata = (o, (vec2.relength w u, v))
                if not this.Phantom && plgm.containstrees newdata then this.Width <- e.OldValue :?> float else data <- newdata
        else if (e.Property = HeightProperty) then
            let h = e.NewValue :?> float
            let w = this.Width
            if h > 0.0 then
                let o, (u, v) = data
                let newdata, nw = match plgm.mthop h with
                                  | Some (n, nw, pl) ->
                                      this.Info <- sprintf "h = %f\r\nPartie %d [√%d, √%d]\r\n%s" h n (if n = 0 then 0 else int plgm.b008784.[n-1]) (int plgm.b008784.[n]) (if pl then "Palier" else "Transition")
                                      if this.MTH then (o, (vec2.relength nw u, vec2.relength h v)), nw else (o, (u, vec2.relength h v)), nw
                                  | None -> (o, (u, vec2.relength h v)), w
                if not this.Phantom && plgm.containstrees newdata then this.Height <- e.OldValue :?> float else data <- newdata
                this.Width <- nw
        else if (e.Property = PhantomProperty) then
            let p = e.NewValue :?> bool
            if not p then
                if data |> plgm.containstrees then this.Phantom <- true
        base.OnPropertyChanged(e)
