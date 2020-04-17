[<AutoOpen>]
module VectorMod

[<Struct>]
type vec2 =
    {
        x : float
        y : float
    }

    static member inline zero = { x = 0.0; y = 0.0 }
    static member inline i x = { x = x; y = 0.0 }
    static member inline j y = { x = 0.0; y = y }
    static member inline ij x y = { x = x; y = y }

    static member inline (+) (u, v) = 
        { 
            x = u.x + v.x
            y = u.y + v.y 
        }
    static member inline (-) (u, v) = 
        { 
            x = u.x - v.x
            y = u.y - v.y 
        }
    static member inline (*) (u, v) =
        {
            x = u.x * v.x
            y = u.y * v.y
        }
    static member inline (/) (u, v) =
        {
            x = u.x / v.x
            y = u.y / v.y
        }
    static member inline (~-) u = 
        { 
            x = -u.x
            y = -u.y
        }
        
    static member inline (*) (v, k) =
        {
            x = v.x * k
            y = v.y * k
        }
    static member inline (/) (v, k) =
        {
            x = v.x / k
            y = v.y / k
        }
    static member inline (*) (k : float, v : vec2) = v * k

    static member inline (.*) (u, v) = u.x * v.x + u.y * v.y
   
[<Struct>]
type vec3 =
    {
        x : float
        y : float
        z : float
    }

    static member inline zero = { x = 0.0; y = 0.0; z = 0.0 }

    static member inline (+) (u, v) = 
        { 
            x = u.x + v.x
            y = u.y + v.y
            z = u.z + v.z
        }
    static member inline (-) (u, v) = 
        { 
            x = u.x - v.x
            y = u.y - v.y 
            z = u.z - v.z
        }
    static member inline (*) (u, v) =
        {
            x = u.x * v.x
            y = u.y * v.y
            z = u.z * v.z
        }
    static member inline (/) (u, v) =
        {
            x = u.x / v.x
            y = u.y / v.y
            z = u.z / v.z
        }
    static member inline (~-) u = 
        { 
            x = -u.x
            y = -u.y
            z = -u.z
        }
        
    static member inline (*) (v, k) =
        {
            x = v.x * k
            y = v.y * k
            z = v.z * k
        }
    static member inline (/) (v, k) =
        {
            x = v.x / k
            y = v.y / k
            z = v.z / k
        }
    static member inline (*) (k : float, v : vec3) = v * k

    static member inline (.*) (u, v) = u.x * v.x + u.y * v.y + u.z * v.z
    static member inline (^*) (u, v) =
        {
            x = u.y * v.z - u.z * v.y
            y = u.z * v.x - u.x * v.z
            z = u.x * v.y - u.y * v.x
        }
   
[<Struct>]
type vec4 =
    {
        x : float
        y : float
        z : float
        w : float
    }
    
    static member inline zero = { x = 0.0; y = 0.0; z = 0.0; w = 0.0 }

    static member inline (+) (u, v) = 
        { 
            x = u.x + v.x
            y = u.y + v.y
            z = u.z + v.z
            w = u.w + v.w
        }
    static member inline (-) (u, v) = 
        { 
            x = u.x - v.x
            y = u.y - v.y 
            z = u.z - v.z
            w = u.w - v.w
        }
    static member inline (*) (u, v) =
        {
            x = u.x * v.x
            y = u.y * v.y
            z = u.z * v.z
            w = u.w * v.w
        }
    static member inline (/) (u, v) =
        {
            x = u.x / v.x
            y = u.y / v.y
            z = u.z / v.z
            w = u.w / v.w
        }
    static member inline (~-) u = 
        { 
            x = -u.x
            y = -u.y
            z = -u.z
            w = -u.w
        }

    static member inline (*) (v, k) =
        {
            x = v.x * k
            y = v.y * k
            z = v.z * k
            w = v.w * k
        }
    static member inline (/) (v, k) =
        {
            x = v.x / k
            y = v.y / k
            z = v.z / k
            w = v.w / k
        }
    static member inline (*) (k : float, v : vec4) = v * k

    static member inline (.*) (u, v) = u.x * v.x + u.y * v.y + u.z * v.z + u.w * v.w
                         
let inline lengthSquared v = v .* v
let inline length v = v |> lengthSquared |> sqrt
let inline norm v = v / length v
let inline relength l v = l * norm v

let vec4 (v : vec3) =
    {
        x = v.x
        y = v.y
        z = v.z
        w = 1.0
    }
let vec3 (v : vec4) =
    {
        x = v.x / v.w
        y = v.y / v.w
        z = v.z / v.w
    }

let coordinatesinbase (u : vec2) (v : vec2) (w : vec2) =
    let xu, yu = u.x, u.y
    let xv, yv = v.x, v.y
    let x, y = w.x, w.y
    let c = xu * yv - xv * yu
    let a = (x * yv - y * xv) / c
    let b = (y * xu - x * yu) / c
    a, b

let decompose (u : vec2) (v : vec2) (w : vec2) =
    let a, b = coordinatesinbase u v w
    a * u, b * v

let rotate a (v : vec2) =
    let sin, cos = sin a, cos a
    let x, y = v.x, v.y
    { x = cos * x - sin * y; y = sin * x + cos * y }

