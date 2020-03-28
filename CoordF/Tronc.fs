namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.WPF
open Coord
open Parallelogram

type Tronc() =
    inherit VisualObject()

    static let WidthProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Width", 1.0, dict [ ("min", box 0.0); ("max", box 1.0) ])
    static let HeigthProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "Heigth", 2.0, dict [ ("min", box 0.0) ])
    static let TranslationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "TranslationStep", 0.01, dict [ ("min", box 0.0) ])
    static let RotationStepProperty = nobj.CreateProperty<Tronc, float>(true, true, true, "RotationStep", 1.0, dict [ ("min", box 0.0); ("max", box 360.0) ])
    static let RotationCenterProperty = nobj.CreateProperty<Tronc, RectPoint>(true, true, true, "RotationCenter", RectPoint.Center)

    let mutable data : plgm = (vec2.zero, vec2.i 1.0, vec2.j 2.0)

    static do        
        nobj.OverrideDefaultValue<Tronc, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Tronc, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 5.0))

    member this.Width
        with get() = this.GetValue(WidthProperty) :?> float
        and set(value : float) = this.SetValue(WidthProperty, value)
    member this.Heigth
        with get() = this.GetValue(HeigthProperty) :?> float
        and set(value : float) = this.SetValue(HeigthProperty, value)
    member this.TranslationStep
        with get() = this.GetValue(TranslationStepProperty) :?> float
        and set(value : float) = this.SetValue(TranslationStepProperty, value)
    member this.RotationStep
        with get() = this.GetValue(RotationStepProperty) :?> float
        and set(value : float) = this.SetValue(RotationStepProperty, value)
    member this.RotationCenter
        with get() = this.GetValue(RotationCenterProperty) :?> RectPoint
        and set(value : RectPoint) = this.SetValue(RotationCenterProperty, value)
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
        seq { for i = int (floor inr.Left) to int (ceil inr.Right) do
                for j = int (floor inr.Bottom) to int (ceil inr.Top) do
                    yield Character.Ellipse(csm.ComputeOutCoordinates(Point(float i, float j)), 5.0, 5.0).Color(FlatBrushes.Alizarin)
              yield (data |> plgmwithcsm csm |> plgmgeometry).ToCharacter(fill, stroke)
              yield Character.Ellipse(csm.ComputeOutCoordinates(Point(c.x, c.y)), 5.0, 5.0).Color(Pen(FlatBrushes.SunFlower, 1.0))}

    override this.OnPropertyChanged (e : DependencyPropertyChangedEventArgs) =
        if (e.Property = WidthProperty || e.Property = HeigthProperty) then
            let w = this.Width
            let h = this.Heigth
            data <- (vec2.zero, vec2.i w, vec2.j h)
        base.OnPropertyChanged(e)
