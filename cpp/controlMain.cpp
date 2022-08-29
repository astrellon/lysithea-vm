#include <random>
#include <iostream>

std::random_device _rd;
std::mt19937 _rand(_rd());

int counter = 0;
double total = 0.0;

bool isDone()
{
    return counter++ >= 1000000;
}
double doRand()
{
    std::uniform_real_distribution<double> dist(0.0, 1.0);
    return dist(_rand);
}
double add(double num1, double num2)
{
    return num1 + num2;
}
void done()
{
    std::cout << "Done: " << total << "\n";
}

void step()
{
    auto num1 = doRand();
    auto num2 = doRand();
    auto num3 = add(num1, num2);
    total = add(num3, total);
}

int main()
{
    do
    {
        step();
    } while(!isDone());
    done();
    return 0;
}