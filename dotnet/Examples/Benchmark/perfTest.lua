function Step()
    return rand() + rand();
end

function Main()
    local total = 0;
    local counter = 0;

    while counter < 1000000 do
        total = total + Step();
        counter = counter + 1;
    end

    print("Done: " .. total)
end

Main();