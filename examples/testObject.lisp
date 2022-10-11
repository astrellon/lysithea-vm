(define main (function ()
    (define person {
        name "Alan"
        age 30
        address ("location 1" "location 2")
    })

    (print person.name " Hello " (math.add 5 person.age))
))

(main)