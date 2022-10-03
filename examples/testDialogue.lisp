(procedure main()
    (say "Here is the test dialogue tree")
    (say "What is your name? ")
    (getPlayerName)
    (say "Hello {playerName}, hope you are well.")

    (:Start)

    (if (isShopEnabled)
        (
            (randomSay ("Welcome to my shop", "Come on in! What can I do for you?"))
            (choice "Open Shop", "openShop")
        )
        (randomSay ("Welcome stranger", "Hey, come on in"))
    )
    (choice "Questions", "questions")
    (choice "Good bye", "goodBye")
    (waitForChoice)
)

(procedure questions()
)