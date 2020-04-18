namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.WPF
open Coord
open Parallelogram
open System
open System.Threading

type Tronc() =
    inherit VisualObject()

    static let WidthProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Width", 1.0, dict [ ("min", box 0.0); ("max", box 1.0) ])
    static let HeightProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Height", 1.0, dict [ ("min", box 0.0) ])
    static let TranslationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "TranslationStep", 0.01, dict [ ("min", box 0.0) ])
    static let RotationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "RotationStep", 1.0, dict [ ("min", box 0.0); ("max", box 360.0) ])
    static let RotationCenterProperty = nobj.CreateProperty<Tronc, RectPoint>(true, true, true, "RotationCenter", RectPoint.Center)
    static let MiniboxProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Minibox", true)
    static let TreesProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Trees", true)
    static let TrackProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Track", true)

    let mutable data : plgm = (vec2.zero, base2.ij)

    static do        
        nobj.OverrideDefaultValue<Tronc, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Tronc, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 1.0))

    member this.Width
        with get() = this.GetValue(WidthProperty) :?> float
        and set(value : float) = this.SetValue(WidthProperty, value)
    member this.Height
        with get() = this.GetValue(HeightProperty) :?> float
        and set(value : float) = this.SetValue(HeightProperty, value)
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
            true
        | None -> false

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

    member this.TranslateOrNot v = this.Apply (plgm.translateOrNot v data)
    member this.RotateOrNot a = this.Apply (plgm.rotateOrNot (this.RCenter) a data)

    member this.TranslateOrNotIL () = this.Apply (plgm.translateOrNotIL (this.TranslationStep) data)
    member this.TranslateOrNotIR () = this.Apply (plgm.translateOrNotIR (this.TranslationStep) data)
    member this.TranslateOrNotJD () = this.Apply (plgm.translateOrNotJD (this.TranslationStep) data)
    member this.TranslateOrNotJU () = this.Apply (plgm.translateOrNotJU (this.TranslationStep) data)
                                                  
    member this.TranslateOrNotUL () = this.Apply (plgm.translateOrNotUL (this.TranslationStep) data)
    member this.TranslateOrNotUR () = this.Apply (plgm.translateOrNotUR (this.TranslationStep) data)
    member this.TranslateOrNotVD () = this.Apply (plgm.translateOrNotVD (this.TranslationStep) data)
    member this.TranslateOrNotVU () = this.Apply (plgm.translateOrNotVU (this.TranslationStep) data)

    member this.RotateOrNotD () = this.Apply (plgm.rotateOrNotD (this.RCenter) (this.RotationStep) data)
    member this.RotateOrNotH () = this.Apply (plgm.rotateOrNotH (this.RCenter) (this.RotationStep) data)

    member this.Horiz () = 
        let uictxt = SynchronizationContext.Current
        let track = this.Track
        async { 
            let! _, success = plgm.horiz (this.OnNext track uictxt) 0.0001 data
            if success then MessageBox.Show "Terminé horizontal" |> ignore
            else MessageBox.Show "Terminé" |> ignore
        } |> Async.Start

    member this.HorizMaxWidth () = 
        let uictxt = SynchronizationContext.Current
        let track = this.Track
        let height = this.Height
        async { 
            let! width = plgm.horizmaxwidth height (this.OnNext track uictxt) 0.0001
            MessageBox.Show (width.ToString ()) |> ignore
        } |> Async.Start

    member this.Reset () =
        data <- (vec2.zero, base2.ij)
        this.Width <- 1.0
        this.Height <- 1.0

    override this.GetCharactersCore csm =
        let fill = this.Fill
        let stroke = this.Stroke
        let inr = csm.InputRange
        let c = this.RCenter
        let xs, ys, xe, ye = plgm.minibox data
        let m = this.Minibox
        let t = this.Trees
        seq { if t then for i = int (floor inr.Left) to int (ceil inr.Right) do
                            for j = int (floor inr.Bottom) to int (ceil inr.Top) do
                                yield Character.Ellipse(Point(float i, float j) |*> csm, 5.0, 5.0).Color(FlatBrushes.Alizarin)
              yield (data |> plgm.bycsm csm |> plgm.geometry).ToCharacter(fill, stroke)
              if m then yield Character.Rectangle(csm.ComputeOutCoordinates(Rect(Point(xs, ys), Point(xe, ye)))).Color(Pen(FlatBrushes.Alizarin, 1.0))
              yield Character.Ellipse(Point(c.x, c.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))}

    override this.OnPropertyChanged (e : DependencyPropertyChangedEventArgs) =
        if (e.Property = WidthProperty) then
            let o, (u, v) = data
            let w = this.Width
            let newdata = (o, (vec2.relength w u, v))
            if plgm.containstrees newdata then this.Width <- e.OldValue :?> float else data <- newdata
        else if (e.Property = HeightProperty) then
            let o, (u, v) = data
            let h = this.Height
            let newdata = (o, (u, vec2.relength h v))
            if plgm.containstrees newdata then this.Height <- e.OldValue :?> float else data <- newdata
        base.OnPropertyChanged(e)
