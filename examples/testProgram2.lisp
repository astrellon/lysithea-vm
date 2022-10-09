(define main (function ()
    (define total 0)
    (define count 0)

    (loop (< count 10)
        (if (< count 5)
            (print "Count less than 5")
            (print "Count more than 5")
        )

        (set total (+ total (step)))
        (set count (+ count 1))
    )

    (print total)

    (if (< total 10)
        (print "Total less than 10")

        (print "Total greater than 10")
    )

    (done)
))

(define step (function ()
    (return (+ (rand) (rand)))
))

(main)

(print "Super done!")