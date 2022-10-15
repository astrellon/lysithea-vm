(define main (function ()
    (define person {
        name "Alan"
        age 30
        address ("location 1" "location 2")
    })

    (print person.name " Hello " (+ 5 person.age) " at " person.address.1)
))

(main)