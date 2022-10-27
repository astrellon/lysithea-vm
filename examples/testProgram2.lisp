(define main (function ()
    (define total 0)
    (define count 0)

    (loop (< count 10)
        (set total (+ total (step)))
        (inc count)

        (if (< count 5)
        (
            (print "Count less than 5")
        )
        (
            (print "Count more than 5")
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

;; (main)
;; (print "Super done!")

(define labelJump (function ()
    (return :Hello)
))

(jump (labelJump))
(print "Should not print")

(:Hello)
(print "Hello\nThere")