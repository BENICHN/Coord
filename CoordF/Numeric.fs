[<AutoOpen>]
module Numeric

open System.Windows
open Coord

let (|*>) (p : Point) (csm : ReadOnlyCoordinatesSystemManager) = csm.ComputeOutCoordinates p

let private swap = List.map (fun ((x1:float, y1:float), (x2:float, y2:float)) -> (y1, x1), (y2, x2))
let private shiftx x = List.map (fun ((x1:float, y1:float), (x2:float, y2:float)) -> (x1 + x, y1), (x2 + x, y2))
let private shifty y = List.map (fun ((x1:float, y1:float), (x2:float, y2:float)) -> (x1, y1 + y), (x2, y2 + y))

let divs n =
    [
        for i = n downto n |> float |> sqrt |> ceil |> int do
            let p = n / i
            if i * p = n then yield i, p 
    ]

let rec parts (w : float) (h : float) (n : int) =
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

let slices (w : float) (h : float) (n : int) =
    let L = max w h
    let l = min w h
    let inv = l = w
    let Lp = L / (float n)
    let res = [ for i = 1 to n - 1 do yield let ip = Lp * (float i) in ((ip, 0.0), (ip, l))]
    if inv then res |> swap else res