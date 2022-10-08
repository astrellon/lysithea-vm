(define main (procedure ()
    (push 0)
    (:Start)

    (step)
    (isDone)
    (jumpFalse :Start)

    (done)
))

(define step (procedure ()
    (rand)
    (rand)
    (add)
    (add)
    (return)
))

(main)