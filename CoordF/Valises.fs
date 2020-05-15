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
        let rec vals l c cur =
            [ 
                if (cur = n) then ()
                elif c < ncot then
                    yield ({x = float c ; y = l}, bas)
                    yield! vals l (c + 1) (cur + 1)
                else yield! vals (l + h) 0 (cur + 1)
            ]
        let res = vals h 0 1
        let { x = _; vec2.y = ly }, _ = List.last res
        res, ly + h

    let q2 onnext h n =
        asyncSeq {
            for ncot in 1 .. n do
                let (res, cot) = getvals h n ncot
                do! onnext (res, cot)
                yield cot
        }

type Valises() =
    inherit VisualObject()

    static let TrackProperty = nobj.CreateProperty<Valises, bool>(true, true, true, "Track", true)

    let mutable data : plgm list * float = [], 0.0

    static do        
        nobj.OverrideDefaultValue<Valises, Brush>(vobj.FillProperty, Brushes.YellowGreen.EditFreezable(fun b -> b.Opacity <- 0.2))
        nobj.OverrideDefaultValue<Valises, Pen>(vobj.StrokeProperty, Pen(Brushes.YellowGreen, 1.0))

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
                    data <- newdata
                    this.NotifyChanged()
                    do! Async.SwitchToThreadPool ()
                } 
        else fun newdata -> async { data <- newdata }

    member this.Q2 h n = 
        let uictxt = SynchronizationContext.Current
        let track = this.Track
        async { 
            let! width = vals.q2 (this.OnNext track uictxt) h n |> AsyncSeq.fold (fun cm c -> max cm c) 0.0
            MessageBox.Show (width.ToString ()) |> ignore
        } |> Async.Start

    override this.GetCharactersCore csm =
        let fill = this.Fill
        let stroke = this.Stroke
        let vals, cot = data
        (((vec2.zero, (vec2.x cot, vec2.y cot)) |> plgm.bycsm csm |> plgm.geometry).ToCharacter(Pen(FlatBrushes.Alizarin, 5.0))) :: (vals |> List.map (fun v -> (v |> plgm.bycsm csm |> plgm.geometry).ToCharacter(fill, stroke))) |> Seq.ofList
