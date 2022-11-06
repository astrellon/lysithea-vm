(define example1 (function()
    (push "Hello")
    (push " There")
    (get "print")
    (call 2)
))

(define example2 (function()
    (define clamp (function (x lower upper)
        (if (> x upper)
            (return upper)
        )
        (if (< x lower)
            (return lower)
        )
        (return x)
    ))

    (print "Clamped 5 " (clamp 5 1 2))
    (print "Clamped 0 " (clamp 0 1 2))
    (print "Clamped 1.5 " (clamp 1.5 1 2))
))

(example1)