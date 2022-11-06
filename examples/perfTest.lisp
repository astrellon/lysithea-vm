(define main (function ()
    (define total 0)
    (define counter 0)

    (loop (lessThan counter 1000000)
        (set total (add total (step)))
        (inc counter)
    )

    (print "Done: " total)
))

(define step (function ()
    (return (add (rand) (rand)))
))

(main)