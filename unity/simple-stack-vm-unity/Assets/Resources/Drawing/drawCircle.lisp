(define calcPosition (function (index)
    (define angle (* index 5 math.DegToRad))

    (define x (* (math.cos angle) 20))
    (define y (* index 0.15))
    (define z (* (math.sin angle) 20))

    (return (vector3.new x y z))
))

(define calcColour (function (index)
    (return (colour.hsv (/ index 100) 0.8 0.75))
))

(draw.clear)
(makeGround)
(define i 0)
(loop (< i 100)
    (draw.element "box" (calcPosition i) (calcColour i) 1)
    (inc i)
)