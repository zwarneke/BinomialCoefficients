# BinomialCoefficients
This repository contains two projects I wrote for my honors thesis at UNL on binomial coefficients modulo any number n. Specifically, it contains a utility to create Pascal's Triangle modulo n (with various options), as well as an actual implementation of the algorithm described in my thesis for efficient computation of binomial coefficients modulo n.

## Efficient Binomial Coefficient Computation Program Usage
To use the binomial coefficient calculator, simply open the project in Visual Studio and run it. From there, you'll be prompted to enter a, b, and n, and the program will compute a choose b mod n using a basic method, a basic method with intermediate modularization (i.e. taking intermediate remainders), and the efficient method described in my thesis. The program will then output the results and times of each.

## Pascal's Square Generator Program Usage
To use the Pascal's Square generator, simply open the project in Visual Studio and run it. From there, you'll be prompted to enter a prime power, a number of rows/columns to generate, and whether all digits or only the leftmost digit in base p should be outputted (all digits is equivalent to just the triangle mod the prime power you entered). From there, the desired triangle (in square form) will be written to a file titled "Mod&lt;PrimePower&gt;.txt" or "Mod&lt;PrimePower&gt;\_LeftmostDigit.txt".
