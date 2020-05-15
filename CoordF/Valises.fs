namespace CoordF

open System.Windows
open System.Windows.Media
open BenLib.WPF
open Coord
open Parallelogram
open System
open System.Threading
open FSharp.Control

module vals = 

    let getvals h n ncot =
        let bas = vec2.i, vec2.y h
        let rec vals l c cur acc =
            if (cur > n) then acc
            elif c < ncot then vals l (c + 1) (cur + 1) (({x = float c ; y = l}, bas) :: acc)
            else vals (l + h) 0 cur acc
        let res = vals 0.0 0 1 []
        let ({ x = _; vec2.y = ly }, _) = List.head res
        res, max (ly + h) (float ncot)

    let q2 onnext h n =
        asyncSeq {
            for ncot in 1 .. n do
                let (res, cot) = getvals h n ncot
                do! onnext (res, cot)
                yield cot
        }

type Valises() =
    inherit VisualObject()
    
    static let DelayProperty = nobj.CreateProperty<Valises, int>(true, true, true, "Delay", 500, dict [ ("min", box 0) ])
    static let Q2hProperty = nobj.CreateProperty<Valises, float>(true, true, true, "Q2 : h", 1.0, dict [ ("min", box 0.0) ])
    static let Q2nProperty = nobj.CreateProperty<Valises, int>(true, true, true, "Q2 : n", 1, dict [ ("min", box 0.0) ])
    static let TrackProperty = nobj.CreateProperty<Valises, bool>(true, true, true, "Track", true)

    let mutable data : plgm list * float = [], 0.0

    static do        
        nobj.OverrideDefaultValue<Valises, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Valises, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 1.0))
        
    member this.Delay
        with get() = this.GetValue(DelayProperty) :?> int
        and set(value : int) = this.SetValue(DelayProperty, value)
    member this.Q2h
        with get() = this.GetValue(Q2hProperty) :?> float
        and set(value : float) = this.SetValue(Q2hProperty, value)
    member this.Q2n
        with get() = this.GetValue(Q2nProperty) :?> int
        and set(value : int) = this.SetValue(Q2nProperty, value)
    member this.Track
        with get() = this.GetValue(TrackProperty) :?> bool
        and set(value : bool) = this.SetValue(TrackProperty, value)
    member __.Data = data

    override __.CreateInstanceCore() = Valises() :> Freezable
    override __.Type = "Valises"

    member private this.OnNext track uictxt =
        if track then 
            fun newdata -> 
                async {
                    do! Async.SwitchToContext uictxt
                    let! _ = Async.AwaitEvent (CompositionTarget.Rendering)
                    do! Async.Sleep this.Delay
                    data <- newdata
                    let _, cot = newdata
                    printfn "%f" cot
                    this.NotifyChanged()
                    do! Async.SwitchToThreadPool ()
                } 
        else fun newdata -> async { data <- newdata }

    member this.Q2 () = 
        let n = this.Q2n
        let h = this.Q2h
        let uictxt = SynchronizationContext.Current
        let track = this.Track
        async { 
            let! res = vals.q2 (this.OnNext track uictxt) h n |> AsyncSeq.toListAsync
            MessageBox.Show ((List.min res).ToString ()) |> ignore
        } |> Async.Start

    override this.GetCharactersCore csm =
        let fill = this.Fill
        let stroke = this.Stroke
        let vals, cot = data
        (((vec2.zero, (vec2.x cot, vec2.y cot)) |> plgm.bycsm csm |> plgm.geometry).ToCharacter(Pen(FlatBrushes.Alizarin, 5.0))) :: (vals |> List.map (fun v -> (v |> plgm.bycsm csm |> plgm.geometry).ToCharacter(fill, stroke))) |> Seq.ofList
