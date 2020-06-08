namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.WPF
open Coord

module Gateau =
    let private swap = List.map (fun ((x1:float, y1:float), (x2:float, y2:float)) -> (y1, x1), (y2, x2))
    let private shiftx x = List.map (fun ((x1:float, y1:float), (x2:float, y2:float)) -> (x1 + x, y1), (x2 + x, y2))
    let private shifty y = List.map (fun ((x1:float, y1:float), (x2:float, y2:float)) -> (x1, y1 + y), (x2, y2 + y))

    let rec parts w h n =
        let L = max w h
        let l = min w h
        let inv = l = w
        let ratio = L / l
        let m, p, r = divs n |> List.map (fun (m, p) -> m, p, (float m) / (float p)) |> List.minBy (fun (_, _, r) -> r - ratio)
        if r - ratio < 2.0 then
            let Lp = L / (float m)
            let lp = l / (float p)
            let res = [ for i = 1 to m - 1 do yield let ip = Lp * (float i) in (ip, 0.0), (ip, l) ] @ [ for i = 1 to p - 1 do yield let ip = lp * (float i) in (0.0, ip), (L, ip) ]
            if inv then res |> swap else res
        else
            let m1 = (m - 1) / 2
            let m2 = m1 + 1
            let L1 = (float m1) * L / (float m)
            let L2 = L - L1
            if inv then (parts l L1 (m1 * p)) @ ((0.0, L1), (l, L1)) :: (parts l L2 (m2 * p) |> shifty L1)
            else (parts L1 l (m1 * p)) @ ((L1, 0.0), (L1, l)) :: (parts L2 l (m2 * p) |> shiftx L1)
    
    let slices w h n =
        let L = max w h
        let l = min w h
        let inv = l = w
        let Lp = L / (float n)
        let res = [ for i = 1 to n - 1 do yield let ip = Lp * (float i) in ((ip, 0.0), (ip, l))]
        if inv then res |> swap else res
    
    let sectors w h n =
        let a = (max w h) / 2.0
        let b = (min w h) / 2.0
        let s = tau * a * b / 2.0
        let inv = 2.0 * a = w
        let res = 
            [ 
                for i = 0 to n - 1 do 
                    let ap =
                        let dap = atan (b * tan (2.0 * (float i) * s / ((float n) * a * b)) / a)
                        let dap = if dap < 0.0 then tau / 4.0 + dap + tau / 2.0 else tau / 4.0 + dap
                        if i <= n / 2 then dap else dap + tau / 2.0
                    yield ((b, a), (b + a * cos ap, a + a * sin ap))
            ]
        if inv then res |> swap else res

type Decoupe =
    | Secteurs = 0
    | Tranches = 1
    | Grilles = 2

type Gateau() =
    inherit VisualObject()

    static let WidthProperty = nobj.CreateProperty<Gateau, float>(true, true, true, "Width", 36.0, dict [ ("min", box 1.0) ])
    static let HeightProperty = nobj.CreateProperty<Gateau, float>(true, true, true, "Height", 36.0, dict [ ("min", box 1.0) ])
    static let CountProperty = nobj.CreateProperty<Gateau, int>(true, true, true, "Count", 23, dict [ ("min", box 2) ])
    static let DecoupeProperty = nobj.CreateProperty<Gateau, Decoupe>(true, true, true, "Decoupe", Decoupe.Grilles)

    static let maptopoints = List.map (fun ((x1, y1), (x2, y2)) -> Point(x1, y1), Point(x2, y2))

    let mutable cache : (Point * Point) list = []

    static do        
        nobj.OverrideDefaultValue<Gateau, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Gateau, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 5.0))

    member this.Width
        with get() = this.GetValue(WidthProperty) :?> float
        and set(value : float) = this.SetValue(WidthProperty, value)
    member this.Height
        with get() = this.GetValue(HeightProperty) :?> float
        and set(value : float) = this.SetValue(HeightProperty, value)
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
        let h = this.Height
        let d = this.Decoupe
        let fill = this.Fill
        let stroke = this.Stroke
        seq {
                match d with
                | Decoupe.Secteurs -> yield Character.Ellipse(Rect(Point(0.0, 0.0) |*> csm, Point(w, h) |*> csm)).Color(fill, stroke)
                | Decoupe.Tranches | Decoupe.Grilles -> yield Character.Rectangle(Rect(Point(0.0, 0.0) |*> csm, Point(w, h) |*> csm)).Color(fill, stroke)
                | _ -> failwith "Type de découpe inconnu"
                for p1, p2 in cache -> Character.Line(p1 |*> csm, p2 |*> csm).Color(stroke)
            }
    override this.OnChanged () =
        let w = this.Width
        let h = this.Height
        let n = this.Count
        let d = this.Decoupe
        cache <- 
            match d with
            | Decoupe.Secteurs -> Gateau.sectors w h n |> maptopoints
            | Decoupe.Tranches -> Gateau.slices w h n |> maptopoints
            | Decoupe.Grilles -> Gateau.parts w h n |> maptopoints
            | _ -> failwith "Type de découpe inconnu"
        base.OnChanged()
