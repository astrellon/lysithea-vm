(define main (function ()
    (actor SELF "shocked")
    (beginLine)
    (text (random.pick (+ "Hello " PLAYER.name "!") (+ "Welcome " PLAYER.name "!")))

    (wait 1000)
    (emotion "happy")

    (text " How are you going?")
    (choice "I'm good thank you" choice1)
    (choice "I'm not good" choice2)
    (endLine)
))

(define choice1 (function ()
    (actor PLAYER "happy")
    (beginLine)
    (text "Hello " SELF.name ", I'm good thank you")
    (endLine)

    (actor SELF "happy")
    (beginLine)
    (text "Glad to hear it!")
    (choice "Good bye" done)
    (choice "Question" (function () (question) (moveTo choice1) ))
    (endLine)
))

(define choice2 (function ()
    (actor PLAYER "sad")
    (beginLine)
    (text "Oh hey " SELF.name ". I'm not doing so good.")
    (endLine)

    (actor SELF "sad")
    (beginLine)
    (text "I'm sorry to hear that.")
    (choice "Good bye" done)
    (choice "Question" (function () (question) (moveTo choice2) ))
    (endLine)
))

(define isSetWhoAmI false)
(define isSetWhereIsThis false)
(define question (function ()
    (:Start)
    (actor SELF)
    (beginLine)
    (text "What do you want to know?")
    (choice "Who are you?" :WhoAmI?)

    (if (== isSetWhoAmI true)
        (choice "Where is this?" :WhereIsThis?)
    )
    (if (== isSetWhereIsThis true)
        (choice "What is this?" :WhatIsThis?)
    )

    (choice "Enough Quetions" :Return)
    (endLine)

    (:WhoAmI?)
    (set isSetWhoAmI true)
    (beginLine)
    (text "I'm just a person.")
    (endLine)

    (actor PLAYER "shocked")
    (beginLine)
    (text "I knew it!")
    (endLine)
    (jump :Start)

    (:WhereIsThis?)
    (set isSetWhereIsThis true)
    (beginLine)
    (text "This is a text box.")
    (endLine)

    (actor PLAYER "shocked")
    (beginLine)
    (text "I knew it!")
    (endLine)
    (jump :Start)

    (:WhatIsThis?)
    (beginLine)
    (text "This ia shop for talking about dialogue.")
    (endLine)

    (actor PLAYER)
    (beginLine)
    (text "I knew that.")
    (endLine)
    (jump :Start)

    (:Return)
    (return)
))

(define done (function ()
    (actor SELF)
    (beginLine)
    (text "Thanks for coming by " PLAYER.name ", see you later!")
    (endLine)
))

(main)