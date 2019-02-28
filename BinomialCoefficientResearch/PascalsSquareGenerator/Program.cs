using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PascalsSquareGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Pascal's Square Mod (in form p^s, where p is prime):");
            Console.Write("Prime: ");
            var basePrime = int.Parse(Console.ReadLine());
            Console.Write("Power: ");
            var primePower = int.Parse(Console.ReadLine());
            var modulus = (int)Math.Pow(basePrime, primePower);

            Console.Write("Leftmost digit (l) or all digits (a) in base p? ");
            var lastDigitOnly = Console.ReadLine()?.Trim().ToLower() == "l";

            Console.Write("Number of rows/columns to generate: ");
            var squareSize = int.Parse(Console.ReadLine());

            var square = new int[squareSize, squareSize];
            for (var i = 0; i < squareSize; i++)
            {
                square[i, 0] = 1;
                square[0, i] = 1;
            }

            for (var row = 1; row < squareSize; row++)
            {
                for (var col = 1; col < squareSize; col++)
                {
                    square[row, col] = (square[row - 1, col] + square[row, col - 1]) % modulus;
                }
            }

            if (lastDigitOnly)
            {
                var pToSMinus1 = (int)Math.Pow(basePrime, primePower - 1);
                for (var row = 0; row < squareSize; row++)
                {
                    for (var col = 0; col < squareSize; col++)
                    {
                        square[row, col] /= pToSMinus1;
                    }
                }
            }

            var lastDigitString = lastDigitOnly ? "_LastDigit" : "";
            WriteSquareToFile(square, $"../../Mod{modulus}{lastDigitString}.txt");
        }

        public static void WriteSquareToFile(int[,] square, string filePath)
        {
            // Find largest number in square
            var maxVal = square.Cast<int>().Max();
            var maxNumDigits = (int)Math.Log10(maxVal) + 1;
            var formatString = $"{{0,{maxNumDigits + 1}}}";
            using (var file = new StreamWriter(filePath))
            {
                for (var row = 0; row < square.GetLength(0); row++)
                {
                    for (var col = 0; col < square.GetLength(1); col++)
                    {
                        file.Write(formatString, square[row, col]);
                    }
                    file.Write('\n');
                }
            }
        }
    }
}
