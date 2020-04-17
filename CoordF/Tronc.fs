namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.WPF
open Coord
open Parallelogram

type Tronc() =
    inherit VisualObject()

    static let WidthProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Width", 1.0, dict [ ("min", box 0.0); ("max", box 1.0) ])
    static let HeightProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Height", 2.0, dict [ ("min", box 0.0) ])
    static let TranslationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "TranslationStep", 0.01, dict [ ("min", box 0.0) ])
    static let RotationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "RotationStep", 1.0, dict [ ("min", box 0.0); ("max", box 360.0) ])
    static let RotationCenterProperty = nobj.CreateProperty<Tronc, RectPoint>(true, true, true, "RotationCenter", RectPoint.Center)
    static let MiniboxProperty = nobj.CreateProperty<Tronc, bool>(true, true, true, "Minibox", true)

    let mutable data : plgm = (vec2.zero, vec2.i 1.0, vec2.j 2.0)

    static do        
        nobj.OverrideDefaultValue<Tronc, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Tronc, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 5.0))

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
    member __.Data = data

    member this.RCenter =
        let rp = this.RotationCenter
        let (o, u, v) = data
        o + rp.XProgress * u + rp.YProgress * v

    override __.CreateInstanceCore() = Tronc() :> Freezable
    override __.Type = "Tronc"

    member this.TranslateOrNot v =
        let newdata = plgmtranslate v data
        if not (plgmcontainstrees newdata) then
            data <- newdata
            this.NotifyChanged()
    member this.RotateOrNot a =
        let newdata = plgmrotate (this.RCenter) a data
        if not (plgmcontainstrees newdata) then
            data <- newdata
            this.NotifyChanged()

    override this.GetCharactersCore csm =
        let fill = this.Fill
        let stroke = this.Stroke
        let inr = csm.InputRange
        let c = this.RCenter
        let xs, ys, xe, ye = plgmminibox data
        let m = this.Minibox
        seq { for i = int (floor inr.Left) to int (ceil inr.Right) do
                for j = int (floor inr.Bottom) to int (ceil inr.Top) do
                    yield Character.Ellipse(Point(float i, float j) |*> csm, 5.0, 5.0).Color(FlatBrushes.Alizarin)
              yield (data |> plgmwithcsm csm |> plgmgeometry).ToCharacter(fill, stroke)
              if m then yield Character.Rectangle(csm.ComputeOutCoordinates(Rect(Point(xs, ys), Point(xe, ye)))).Color(Pen(FlatBrushes.Alizarin, 1.0))
              yield Character.Ellipse(Point(c.x, c.y) |*> csm, 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))}

    override this.OnPropertyChanged (e : DependencyPropertyChangedEventArgs) =
        if (e.Property = WidthProperty) then
            let o, u, v = data
            let w = this.Width
            let newdata =(o, u |> relength w, v)
            if plgmcontainstrees newdata then this.Width <- e.OldValue :?> float else data <- newdata
        else if (e.Property = HeightProperty) then
            let o, u, v = data
            let h = this.Height
            let newdata =(o, u, v |> relength h)
            if plgmcontainstrees newdata then this.Height <- e.OldValue :?> float else data <- newdata
        base.OnPropertyChanged(e)
