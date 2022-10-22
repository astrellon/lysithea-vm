(define randomColour (function ()
    (define hue (random.range 0.0 1.0))
    (define saturation (random.range 0.5 1.0))
    (define value (random.range 0.5 1.0))

    (return (colour.hsv hue saturation value))
))

(define makeGround (function ()
    (draw.complex "plane" vector.zero (randomColour) 100)
))

(makeGround)