(draw.clear)
(makeGround)
(define i 0)
(loop (< i 100)
    (draw.element "Box" (nativeCalcPosition i) (nativeCalcColour i) 1)
    (inc i)
)