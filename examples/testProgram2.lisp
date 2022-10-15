(define main (function ()
    (define total 0)
    (define count 0)

    (loop (< count 10)
        (set total (+ total (step)))
        (set count (+ count 1))

        (if (< count 5)
        (
            (print "Count less than 5")
            (continue)
            (print "Should not print!")
        )
        (
            (print "Count more than 5")
            (break)
        )
        )

    )

    (print "Total: " total ", Count: " count)

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