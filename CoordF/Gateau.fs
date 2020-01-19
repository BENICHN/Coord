namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.WPF
open Coord

type dobj = DependencyObject
type nobj = NotifyObject
type vobj = VisualObject

type Decoupe =
    | Secteurs = 0
    | Tranches = 1
    | Grilles = 2

type Gateau() =
    inherit VisualObject()

    static let WidthProperty = nobj.CreateProperty<Gateau, float>(true, true, true, "Width", 36.0, dict [ ("min", box 1.0) ])
    static let HeigthProperty = nobj.CreateProperty<Gateau, float>(true, true, true, "Heigth", 36.0, dict [ ("min", box 1.0) ])
    static let CountProperty = nobj.CreateProperty<Gateau, int>(true, true, true, "Count", 23, dict [ ("min", box 2) ])
    static let DecoupeProperty = nobj.CreateProperty<Gateau, Decoupe>(true, true, true, "Decoupe", Decoupe.Grilles)

    let mutable cache : (Point * Point) list = []

    static do        
        nobj.OverrideDefaultValue<Gateau, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Gateau, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 5.0))

    member this.Width
        with get() = this.GetValue(WidthProperty) :?> float
        and set(value : float) = this.SetValue(WidthProperty, value)
    member this.Heigth
        with get() = this.GetValue(HeigthProperty) :?> float
        and set(value : float) = this.SetValue(HeigthProperty, value)
    member this.Count
        with get() = this.GetValue(CountProperty) :?> int
        and set(value : int) = this.SetValue(CountProperty, value)
    member this.Decoupe
        with get() = this.GetValue(DecoupeProperty) :?> Decoupe
        and set(value : Decoupe) = this.SetValue(DecoupeProperty, value)

    override __.CreateInstanceCore() = Gateau() :> Freezable
    override __.Type = "Gateau"

    override this.GetCharactersCore csm =
        let w = this.Width
        let h = this.Heigth
        let fill = this.Fill
        let stroke = this.Stroke
        seq {
                yield Character.Rectangle(Rect(Point(0.0, 0.0) |*> csm, Point(w, h) |*> csm)).Color(fill, stroke)
                for p1, p2 in cache -> Character.Line(p1 |*> csm, p2 |*> csm).Color(stroke)
            }
    override this.OnChanged () =
        let w = this.Width
        let h = this.Heigth
        let n = this.Count
        let d = this.Decoupe
        cache <- 
            match d with
            | Decoupe.Secteurs -> parts w h n |> List.map (fun ((x1, y1), (x2, y2)) -> Point(x1, y1), Point(x2, y2))
            | Decoupe.Tranches -> slices w h n |> List.map (fun ((x1, y1), (x2, y2)) -> Point(x1, y1), Point(x2, y2))
            | Decoupe.Grilles -> parts w h n |> List.map (fun ((x1, y1), (x2, y2)) -> Point(x1, y1), Point(x2, y2))
            | _ -> failwith "Type de découpe inconnu"
        base.OnChanged()
