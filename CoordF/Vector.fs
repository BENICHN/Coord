[<AutoOpen>]
module VectorMod

type vec2 =
    {
        x : float
        y : float
    }

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

type base2 = vec2 * vec2
type base3 = vec3 * vec3 * vec3
type base4 = vec4 * vec4 * vec4 * vec4

module vec2 =

    let zero = { x = 0.0 ; y = 0.0 }
    let i = { x = 1.0 ; y = 0.0 }
    let j = { x = 0.0 ; y = 1.0 }

    let inline x x = { x = x ; y = 0.0 }
    let inline y y = { x = 0.0 ; y = y }
    let inline xy x y z = { x = x ; y = y }

    let inline lengthSquared (v : vec2) = v .* v
    let length (v : vec2) = v |> lengthSquared |> sqrt
    let norm (v : vec2) = v / length v
    let relength (l : float) (v : vec2) = l * norm v

    let rotate a (v : vec2) =
        let sin, cos = sin a, cos a
        let x, y = v.x, v.y
        { x = cos * x - sin * y; y = sin * x + cos * y }

module vec3 =

    let zero = { x = 0.0; y = 0.0 ; z = 0.0}
    let i = { x = 1.0 ; y = 0.0 ; z = 0.0 }
    let j = { x = 0.0 ; y = 1.0 ; z = 0.0 }
    let k = { x = 0.0 ; y = 0.0 ; z = 1.0 }

    let inline x x = { x = x ; y = 0.0 ; z = 0.0 }
    let inline y y = { x = 0.0 ; y = y ; z = 0.0 }
    let inline z z = { x = 0.0 ; y = 0.0 ; z = z }
    let inline xyz x y z = { x = x ; y = y ; z = z }

    let inline lengthSquared (v : vec3) = v .* v
    let length (v : vec3) = v |> lengthSquared |> sqrt
    let norm (v : vec3) = v / length v
    let relength (l : float) (v : vec3) = l * norm v

module vec4 =

    let zero = { x = 0.0; y = 0.0 ; z = 0.0}
    let i = { x = 1.0 ; y = 0.0 ; z = 0.0 ; w = 0.0 }
    let j = { x = 0.0 ; y = 1.0 ; z = 0.0 ; w = 0.0 }
    let k = { x = 0.0 ; y = 0.0 ; z = 1.0 ; w = 0.0 }
    let l = { x = 0.0 ; y = 0.0 ; z = 0.0 ; w = 1.0 }

    let inline x x = { x = x ; y = 0.0 ; z = 0.0 ; w = 0.0 }
    let inline y y = { x = 0.0 ; y = y ; z = 0.0 ; w = 0.0 }
    let inline z z = { x = 0.0 ; y = 0.0 ; z = z ; w = 0.0 }
    let inline w w = { x = 0.0 ; y = 0.0 ; z = 0.0 ; w = w }
    let inline xyzw x y z w = { x = x ; y = y ; z = z ; w = w }

    let inline lengthSquared (v : vec4) = v .* v
    let length (v : vec4) = v |> lengthSquared |> sqrt
    let norm (v : vec4) = v / length v
    let relength (l : float) (v : vec4) = l * norm v

module base2 =
    let ij = vec2.i, vec2.j
    let norm ((u, v) : base2) = vec2.norm u, vec2.norm v
    let orthdir (u : vec2) = u, { x = -u.y; y = u.x }
    let orthndir = orthdir >> norm
    let coordinates ((u, v) : base2) (w : vec2) =
        if u.y = 0.0 && v.x = 0.0 then w.x / u.x, w.y / v.y
        else
            let xu, yu = u.x, u.y
            let xv, yv = v.x, v.y
            let x, y = w.x, w.y
            let c = xu * yv - xv * yu
            let a = (x * yv - y * xv) / c
            let b = (y * xu - x * yu) / c
            a, b
    let decompose ((u, v) : base2) (w : vec2) =
        let a, b = coordinates (u, v) w
        a * u, b * v

module base3 =
    let ijk = vec3.i, vec3.j, vec3.k
    let norm ((u, v, w) : base3) = vec3.norm u, vec3.norm v, vec3.norm w

module base4 =
    let ijkl = vec4.i, vec4.j, vec4.k, vec4.l
    let norm ((u, v, w, x) : base4) = vec4.norm u, vec4.norm v, vec4.norm w, vec4.norm x

