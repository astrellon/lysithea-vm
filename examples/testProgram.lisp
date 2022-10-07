(procedure main ()
    (set total 0)
    (set count 0)

    (loop (comp.less count 10)
        (if (comp.less count 5)
            (print "Count less than 5")
            (print "Count more than 5")
        )

        (set total (math.add total (step)))
        (set count (math.add count 1))
    )

    (print total)

    (if (comp.less total 10)
        (print "Total less than 10")

        (print "Total greater than 10")
    )

    (done)
)

(procedure step ()
    (return (math.add (rand) (rand)))
)