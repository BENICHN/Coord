[<AutoOpen>]
module MatrixMod

let private rz a = 
    let cos = cos a
    let sin = sin a
    (cos,-sin, 0.0,
     sin, cos, 0.0,
     0.0, 0.0, 1.0)

let private ry a = 
    let cos = cos a
    let sin = sin a
    (cos, 0.0,-sin,
     0.0, 1.0, 0.0,
     sin, 0.0, cos)
     
let private rx a = 
    let cos = cos a
    let sin = sin a
    (1.0, 0.0, 0.0,
     0.0, cos,-sin,
     0.0, sin, cos)

let private scale x y z = (x,  0.0, 0.0,
                           0.0, y,  0.0,
                           0.0, 0.0, z )

type mat3(_m11 : float, _m12 : float, _m13 : float,
          _m21 : float, _m22 : float, _m23 : float,
          _m31 : float, _m32 : float, _m33 : float) = 
    struct
        
        member __.m11 = _m11 member __.m12 = _m12 member __.m13 = _m13
        member __.m21 = _m21 member __.m22 = _m22 member __.m23 = _m23
        member __.m31 = _m31 member __.m32 = _m32 member __.m33 = _m33

        static member val identity = mat3(1.0, 0.0, 0.0,
                                          0.0, 1.0, 0.0,
                                          0.0, 0.0, 1.0)

        static member private fromTuple t =
            let (a, b, c, d, e, f, g, h, i) = t
            mat3(a, b, c,
                 d, e, f,
                 g, h, i)

        static member rx a = mat3.fromTuple (rx a)
        static member ry a = mat3.fromTuple (ry a)
        static member rz a = mat3.fromTuple (rz a)
        static member scale x y z = mat3.fromTuple (scale x y z)

        static member trans x y= mat3(1.0, 0.0,  x,
                                      0.0, 1.0,  y,
                                      0.0, 0.0, 1.0)

        static member inline (+) (l : mat3, r : mat3) = mat3(l.m11 + r.m11, l.m12 + r.m12, l.m13 + r.m13,
                                                             l.m21 + r.m21, l.m22 + r.m22, l.m23 + r.m23,
                                                             l.m31 + r.m31, l.m32 + r.m32, l.m33 + r.m33)

        static member inline (-) (l : mat3, r : mat3) = mat3(l.m11 - r.m11, l.m12 - r.m12, l.m13 - r.m13,
                                                             l.m21 - r.m21, l.m22 - r.m22, l.m23 - r.m23,
                                                             l.m31 - r.m31, l.m32 - r.m32, l.m33 - r.m33)

        static member inline (*) (m : mat3, k : float) = mat3(m.m11 * k, m.m12 * k, m.m13 * k,
                                                              m.m21 * k, m.m22 * k, m.m23 * k,
                                                              m.m31 * k, m.m32 * k, m.m33 * k)

        static member inline (/) (m : mat3, k : float) = mat3(m.m11 / k, m.m12 / k, m.m13 / k,
                                                              m.m21 / k, m.m22 / k, m.m23 / k,
                                                              m.m31 / k, m.m32 / k, m.m33 / k)
        static member inline (*) (k : float, m : mat3) = m * k

        static member inline (<*|) (l : mat3, r : mat3) = mat3(l.m11 * r.m11 + l.m12 * r.m21 + l.m13 * r.m31, l.m11 * r.m12 + l.m12 * r.m22 + l.m13 * r.m32, l.m11 * r.m13 + l.m12 * r.m23 + l.m13 * r.m33,
                                                               l.m21 * r.m11 + l.m22 * r.m21 + l.m23 * r.m31, l.m21 * r.m12 + l.m22 * r.m22 + l.m23 * r.m32, l.m21 * r.m13 + l.m22 * r.m23 + l.m23 * r.m33,
                                                               l.m31 * r.m11 + l.m32 * r.m21 + l.m33 * r.m31, l.m31 * r.m12 + l.m32 * r.m22 + l.m33 * r.m32, l.m31 * r.m13 + l.m32 * r.m23 + l.m33 * r.m33)
        static member inline (|*>) (l : mat3, r : mat3) = r <*| l

        static member inline (<*|) (l : mat3, r : vec3) = 
            { 
                x = l.m11 * r.x + l.m12 * r.y + l.m13 * r.z
                y = l.m21 * r.x + l.m22 * r.y + l.m23 * r.z
                z = l.m31 * r.x + l.m32 * r.y + l.m33 * r.z
            }
        static member inline (|*>) (l : vec3, r : mat3) = r <*| l
    end

type mat4(_m11 : float, _m12 : float, _m13 : float, _m14 : float,
          _m21 : float, _m22 : float, _m23 : float, _m24 : float,
          _m31 : float, _m32 : float, _m33 : float, _m34 : float,
          _m41 : float, _m42 : float, _m43 : float, _m44 : float) = 
    struct

        member __.m11 = _m11 member __.m12 = _m12 member __.m13 = _m13 member __.m14 = _m14
        member __.m21 = _m21 member __.m22 = _m22 member __.m23 = _m23 member __.m24 = _m24
        member __.m31 = _m31 member __.m32 = _m32 member __.m33 = _m33 member __.m34 = _m34
        member __.m41 = _m41 member __.m42 = _m42 member __.m43 = _m43 member __.m44 = _m44

        static member inline identity = mat4(1.0, 0.0, 0.0, 0.0,
                                             0.0, 1.0, 0.0, 0.0,
                                             0.0, 0.0, 1.0, 0.0,
                                             0.0, 0.0, 0.0, 1.0)
                                          
        static member private fromTuple t =
            let (a, b, c, d, e, f, g, h, i) = t
            mat4(a,   b,   c,   0.0,
                 d,   e,   f,   0.0,
                 g,   h,   i,   0.0,
                 0.0, 0.0, 0.0, 1.0)
                 
        static member rx a = mat4.fromTuple (rx a)
        static member ry a = mat4.fromTuple (ry a)
        static member rz a = mat4.fromTuple (rz a)
        static member scale x y z = mat4.fromTuple (scale x y z)
        static member trans x y z = mat4(1.0, 0.0, 0.0,  x,
                                         0.0, 1.0, 0.0,  y,
                                         0.0, 0.0, 1.0,  z,
                                         0.0, 0.0, 0.0, 1.0)

        static member proj f n w h = 
            let ffn = f / (f - n)
            mat4(2.0 * n / w,     0.0    , 0.0,   0.0   ,
                     0.0    , 2.0 * n / h, 0.0,   0.0   ,
                     0.0    ,     0.0    , ffn, -n * ffn,
                     0.0    ,     0.0    , 1.0,   0.0   )
                                         
        static member inline (+) (l : mat4, r : mat4) = mat4(l.m11 + r.m11, l.m12 + r.m12, l.m13 + r.m13, l.m14 + r.m14,
                                                             l.m21 + r.m21, l.m22 + r.m22, l.m23 + r.m23, l.m24 + r.m24,
                                                             l.m31 + r.m31, l.m32 + r.m32, l.m33 + r.m33, l.m34 + r.m34,
                                                             l.m41 + r.m41, l.m42 + r.m42, l.m43 + r.m43, l.m44 + r.m44)
        
        static member inline (-) (l : mat4, r : mat4) = mat4(l.m11 - r.m11, l.m12 - r.m12, l.m13 - r.m13, l.m14 - r.m14,
                                                             l.m21 - r.m21, l.m22 - r.m22, l.m23 - r.m23, l.m24 - r.m24,
                                                             l.m31 - r.m31, l.m32 - r.m32, l.m33 - r.m33, l.m34 - r.m34,
                                                             l.m41 - r.m41, l.m42 - r.m42, l.m43 - r.m43, l.m44 - r.m44)
        
        static member inline (*) (m : mat4, k : float) = mat4(m.m11 * k, m.m12 * k, m.m13 * k, m.m14 * k,
                                                              m.m21 * k, m.m22 * k, m.m23 * k, m.m24 * k,
                                                              m.m31 * k, m.m32 * k, m.m33 * k, m.m34 * k,
                                                              m.m41 * k, m.m42 * k, m.m43 * k, m.m44 * k)
        
        static member inline (/) (m : mat4, k : float) = mat4(m.m11 / k, m.m12 / k, m.m13 / k, m.m14 / k,
                                                              m.m21 / k, m.m22 / k, m.m23 / k, m.m24 / k,
                                                              m.m31 / k, m.m32 / k, m.m33 / k, m.m34 / k,
                                                              m.m41 / k, m.m42 / k, m.m43 / k, m.m44 / k)
        static member inline (*) (k : float, m : mat4) = m * k

        static member inline (<*|) (l : mat4, r : mat4) = mat4(l.m11 * r.m11 + l.m12 * r.m21 + l.m13 * r.m31 + l.m14 * r.m41, l.m11 * r.m12 + l.m12 * r.m22 + l.m13 * r.m32 + l.m14 * r.m42, l.m11 * r.m13 + l.m12 * r.m23 + l.m13 * r.m33 + l.m14 * r.m43, l.m11 * r.m14 + l.m12 * r.m24 + l.m13 * r.m34 + l.m14 * r.m44,
                                                               l.m21 * r.m11 + l.m22 * r.m21 + l.m23 * r.m31 + l.m24 * r.m41, l.m21 * r.m12 + l.m22 * r.m22 + l.m23 * r.m32 + l.m24 * r.m42, l.m21 * r.m13 + l.m22 * r.m23 + l.m23 * r.m33 + l.m24 * r.m43, l.m21 * r.m14 + l.m22 * r.m24 + l.m23 * r.m34 + l.m24 * r.m44,
                                                               l.m31 * r.m11 + l.m32 * r.m21 + l.m33 * r.m31 + l.m34 * r.m41, l.m31 * r.m12 + l.m32 * r.m22 + l.m33 * r.m32 + l.m34 * r.m42, l.m31 * r.m13 + l.m32 * r.m23 + l.m33 * r.m33 + l.m34 * r.m43, l.m31 * r.m14 + l.m32 * r.m24 + l.m33 * r.m34 + l.m34 * r.m44,
                                                               l.m41 * r.m11 + l.m42 * r.m21 + l.m43 * r.m31 + l.m44 * r.m41, l.m41 * r.m12 + l.m42 * r.m22 + l.m43 * r.m32 + l.m44 * r.m42, l.m41 * r.m13 + l.m42 * r.m23 + l.m43 * r.m33 + l.m44 * r.m43, l.m41 * r.m14 + l.m42 * r.m24 + l.m43 * r.m34 + l.m44 * r.m44)
        static member inline (|*>) (l : mat4, r : mat4) = r <*| l

        static member inline (<*|) (l : mat4, r : vec4) = 
            {
                x = l.m11 * r.x + l.m12 * r.y + l.m13 * r.z + l.m14 * r.w
                y = l.m21 * r.x + l.m22 * r.y + l.m23 * r.z + l.m24 * r.w
                z = l.m31 * r.x + l.m32 * r.y + l.m33 * r.z + l.m34 * r.w
                w = l.m41 * r.x + l.m42 * r.y + l.m43 * r.z + l.m44 * r.w
            }
        static member inline (|*>) (l : vec4, r : mat4) = r <*| l
    end
