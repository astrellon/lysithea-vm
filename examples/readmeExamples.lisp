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

(define ifExample (function ()
    (define logCounter (function ()
        (if (< counter 10)
            (print "Counter less than 10")
            (print "Counter more than 10")
        )
    ))

    (define counter 0)
    (logCounter) ; Prints Counter less than 10
    (define counter 20)
    (logCounter) ; Prints Counter more than 10
))

(define functionExample (function ()
    (define clamp (function (input lower upper)
        (if (< input lower)
            (return lower)
        )
        (if (> input upper)
            (return upper)
        )
        (return input)
    ))

    (print "Clamped 5 "  (clamp 5 -1 1))  ; Clamped 5 1
    (print "Clamped -5 " (clamp -5 -1 1)) ; Clamped -5 -1
    (print "Clamped 0 "  (clamp 0 -1 1))  ; Clamped 0 0
))

(define functionUnpackExample (function ()
    (define log (function (type ...inputs)
        (print "[" type "]: " ...inputs)
    ))

    (define findMin (function (...values)
        (if (== values.length 0)
            (return null)
        )

        (define min values.0)
        (define i 1)
        (loop (< i values.length)
            (define curr (array.get values i))
            (if (> min curr)
                (set min curr)
            )
            (inc i)
        )

        (return min)
    ))

    (log "Info" "Minimum Number: " (findMin 1 2 3))
    (log "Info" "Minimum Number: " (findMin 20 30 10))
    (log "Info" "Minimum Lexical: " (findMin "ABC" "DEF" "ZXC"))
    (log "Info" "Minimum Empty: " (findMin))
))

(functionUnpackExample)