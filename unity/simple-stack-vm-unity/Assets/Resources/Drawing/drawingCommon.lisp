(define randomColour (function ()
    (define hue (random.range 0.0 1.0))
    (define saturation (random.range 0.5 1.0))
    (define value (random.range 0.5 1.0))

    (return (colour.hsv hue saturation value))
))

(define randomVector (function (from to)
    (define x (random.range from.x to.x))
    (define y (random.range from.y to.y))
    (define z (random.range from.z to.z))

    (return (vector3.new x y z))
))

(define makeGround (function ()
    (draw.element "plane" vector3.zero (randomColour) 100)
))

(makeGround)