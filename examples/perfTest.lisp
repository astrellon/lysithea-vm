(set Main (procedure ()
    (push 0)
    (:Start)

    (Step)
    (isDone)
    (JumpFalse :Start)

    (done)
))

(set Step (procedure ()
    (rand)
    (rand)
    (add)
    (add)
    (return)
))