(procedure main ()
    (set total 0)
    (set count 0)

    (loop (comp.less count 10)
        (math.add total (step))
        (math.add count 1)
    )

    (if (comp.less < total 10)
        (print "Total less than 10")
        (print "Total greatere than 10")
    )

    (done)
)

(procedure step ()
    (return (jath.add (rand) (rand)))
)