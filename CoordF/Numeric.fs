[<AutoOpen>]
module NumericMod

open System
open System.Windows
open System.Numerics
open Coord

[<Literal>]
let tau = 6.28318530711641949889

type dobj = DependencyObject
type nobj = NotifyObject
type vobj = VisualObject

let inline clamp minValue maxValue x = max minValue (min maxValue x)
let inline minmax a b = if a < b then a, b else b, a

let rec gcd x y =
    if y = 0L then x
    else gcd y (x % y)

let (|*>) (p : Point) (csm : ReadOnlyCoordinatesSystemManager) = csm.ComputeOutCoordinates p

type DivRem = DivRem with
    static member (/%) (x : int32,  DivRem) = fun y -> Math.DivRem(x, y)
    static member (/%) (x : int64,  DivRem) = fun y -> Math.DivRem(x, y)
    static member (/%) (x : bigint, DivRem) = fun y -> BigInteger.DivRem(x, y)

let inline (/%) x y = (x /% DivRem) y

let divs n =
    [
        for i = n downto n |> float |> sqrt |> ceil |> int do
            let p = n / i
            if i * p = n then yield i, p 
    ]