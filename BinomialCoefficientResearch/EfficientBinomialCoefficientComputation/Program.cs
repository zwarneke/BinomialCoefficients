using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EfficientBinomialCoefficientComputation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Sample input that yields good results: 306255 choose 151923 mod 343 = 7^3
            // (should be congruent to 98, with significant time improvements since 343 << 306255)
            Console.WriteLine("Computing a choose b modulo n");
            Console.Write("a: ");
            var a = BigInteger.Parse(Console.ReadLine());
            Console.Write("b: ");
            var b = BigInteger.Parse(Console.ReadLine());
            Console.Write("n: ");
            var n = BigInteger.Parse(Console.ReadLine());
            Console.WriteLine();

            var time1 = RunAndTimeExecution(() => BasicBinomialCoefficient(a, b) % n, out var result1);
            var time2 = RunAndTimeExecution(() => BasicBinomialCoefficientModN(a, b, n), out var result2);
            var time3 = RunAndTimeExecution(() => ImprovedBinomialCoefficientModN(a, b, n), out var result3);
            Console.WriteLine($"Basic Result: {result1}\nBasic Time: {time1} ms\n");
            Console.WriteLine($"Basic Result (w/intermediate mod.): {result2}\nBasic Time (w/intermediate mod.): {time2} ms\n");
            Console.WriteLine($"Improved Result: {result3}\nImproved Time: {time3} ms\n");
            
            Console.ReadLine();
        }

        public static long RunAndTimeExecution<T>(Func<T> function, out T functionReturn)
        {
            var stopwatch = Stopwatch.StartNew();
            functionReturn = function();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Computes a choose b, using a basic method
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>a choose b</returns>
        /// <remarks>
        /// This method will order the multiplications and divisions in an efficient way;
        /// for instance, to compute 10 choose 5, the method will take 10 / 1 * 9 / 2 * 8 / 3 ...
        /// so that the numerator doesn't become too big too quickly.
        /// </remarks>
        public static BigInteger BasicBinomialCoefficient(BigInteger a, BigInteger b)
        {
            var minDenom = BigInteger.Min(b, a - b);
            var result = BigInteger.One;
            for (var i = BigInteger.One; i <= minDenom; i++)
            {
                result *= a - i + BigInteger.One;
                result /= i;
            }

            return result;
        }

        /// <summary>
        /// Computes a choose b mod n, using a basic method with intermediate modularization
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="n">n</param>
        /// <returns>a choose b mod n</returns>
        public static BigInteger BasicBinomialCoefficientModN(BigInteger a, BigInteger b, BigInteger n)
        {
            var minDenom = BigInteger.Min(b, a - b);
            var result = BigInteger.One;
            var endAdjustment = BigInteger.One;
            for (var i = BigInteger.One; i <= minDenom; i++)
            {
                var top = a - i + BigInteger.One;
                var gcd = GCD(top, n);
                while (gcd != 1)
                {
                    top /= gcd;
                    endAdjustment *= gcd;
                    gcd = GCD(top, n);
                }
                result *= top % n;

                var bottom = i;
                gcd = GCD(bottom, n);
                while (gcd != 1)
                {
                    bottom /= gcd;
                    endAdjustment /= gcd;
                    gcd = GCD(bottom, n);
                }
                result *= InverseModN(bottom % n, n);

                result %= n;
            }

            return (result * endAdjustment) % n;
        }

        /// <summary>
        /// Computes a choose b mod n, using the improved, more efficient method
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="n">n</param>
        /// <returns>a choose b mod n</returns>
        /// <remarks>
        /// TODO: Could add memoization of basic-computed binomial coefficients for speed improvement
        /// </remarks>
        public static BigInteger ImprovedBinomialCoefficientModN(BigInteger a, BigInteger b, BigInteger n)
        {
            // First, find the prime factorization of n
            var nFactorization = PrimeFactorization(n);

            // Next, compute the binomial coefficient modulo each prime power in the factorization
            var binomialCoefficientCongruences = new List<Tuple<BigInteger, BigInteger>>();
            foreach (var primeAndPower in nFactorization)
            {
                var primePower = BigInteger.Pow(primeAndPower.Item1, (int)primeAndPower.Item2);
                var aCopy = a;
                var bCopy = b;
                var result = BigInteger.One;
                var totalPrimesInProduct = BigInteger.Zero;
                while (aCopy >= primePower)
                {
                    var aRemainder = aCopy % primePower;
                    var bRemainder = bCopy % primePower;

                    result *= ModifiedBasicBinomialCoefficientModN(aRemainder, bRemainder, primeAndPower, out var numPrimesInProduct);
                    totalPrimesInProduct += numPrimesInProduct;

                    result *= InverseModN(ModifiedBasicBinomialCoefficientModN(aRemainder / primeAndPower.Item1,
                        bRemainder / primeAndPower.Item1, primeAndPower, out numPrimesInProduct), primePower);
                    totalPrimesInProduct -= numPrimesInProduct;

                    aCopy /= primeAndPower.Item1;
                    bCopy /= primeAndPower.Item1;
                    result %= primePower;
                }

                result *= ModifiedBasicBinomialCoefficientModN(aCopy, bCopy, primeAndPower, out var finalNumPrimesInProduct);
                totalPrimesInProduct += finalNumPrimesInProduct;

                result *= BigInteger.Pow(primeAndPower.Item1, (int)totalPrimesInProduct);
                result %= primePower;

                binomialCoefficientCongruences.Add(new Tuple<BigInteger, BigInteger>(result, primePower));
            }

            // Lastly, determine overall coefficient using effective chinese remainder theorem
            return EffectiveChineseRemainderTheorem(binomialCoefficientCongruences);
        }

        /// <summary>
        /// Computes the modified version of a choose b mod n used in the method above
        /// NOTE: n must be a prime power
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="primeAndPower">n expressed as p^s</param>
        /// <returns>a choose b mod n</returns>
        public static BigInteger ModifiedBasicBinomialCoefficientModN(BigInteger a, BigInteger b, Tuple<BigInteger, BigInteger> primeAndPower, out BigInteger primesInProduct)
        {
            var n = BigInteger.Pow(primeAndPower.Item1, (int)primeAndPower.Item2);
            var nCopy = n;

            // Reduce nCopy to the power of p just less than or equal to a;
            // for instance, if a = 60, b = 58, and n = 7^3 = 343, this will reduce nCopy to 49
            while (nCopy > a && nCopy > b)
            {
                nCopy /= primeAndPower.Item1;
            }

            primesInProduct = BigInteger.Zero;
            while (a < b)
            {
                primesInProduct++;
                a %= nCopy;
                b %= nCopy;
                nCopy /= primeAndPower.Item1;
            }

            return BasicBinomialCoefficient(a, b) % n;
        }

        /// <summary>
        /// Computes the effective chinese remainder theorem given a list of congruences.
        /// </summary>
        /// <param name="congruences">
        /// A list of tuples corresponding to each congruence;
        /// for instance, if x is congruent to 3 mod 5, the tuple (3,5) should be in the list.
        /// </param>
        /// <returns>
        /// The result of the effective chinese remainder theorem given the list of congruences;
        /// for instance, if {(3,5),(4,7)} is passed, 18 will be returned
        /// (since 18 is the unique number in Z_35 that is both congruent to 3 mod 5 and 4 mod 7)
        /// </returns>
        public static BigInteger EffectiveChineseRemainderTheorem(List<Tuple<BigInteger, BigInteger>> congruences)
        {
            var result = BigInteger.Zero;
            var overallModulus = congruences.Aggregate(BigInteger.One, (product, congruence) => product * congruence.Item2);
            for (var i = 0; i < congruences.Count; i++)
            {
                // product of moduli (except current one)
                var b = congruences.Where((c, index) => index != i)
                    .Aggregate(BigInteger.One, (product, congruence) => product * congruence.Item2);

                // inverse of that product
                var bInv = InverseModN(b, congruences[i].Item2);

                result += congruences[i].Item1 * b * bInv;
                result %= overallModulus;
            }

            return result;
        }

        /// <summary>
        /// Computes the prime factorization of a given integer n
        /// </summary>
        /// <param name="n">n</param>
        /// <returns>
        /// A list of tuples corresponding to each prime and its power in the prime factorization of n
        /// </returns>
        public static List<Tuple<BigInteger, BigInteger>> PrimeFactorization(BigInteger n)
        {
            var result = new List<Tuple<BigInteger, BigInteger>>();

            var i = new BigInteger(2);
            var currentPrimePower = BigInteger.Zero;
            while (n != BigInteger.One)
            {
                while (n % i == 0)
                {
                    n /= i;
                    currentPrimePower++;
                }

                if (currentPrimePower > 0)
                {
                    result.Add(new Tuple<BigInteger, BigInteger>(i, currentPrimePower));
                    currentPrimePower = BigInteger.Zero;
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Computes a^{-1} mod n (i.e. b such that ab is congruent to 1 mod n)
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="n">n</param>
        /// <returns>a^{-1} mod n</returns>
        public static BigInteger InverseModN(BigInteger a, BigInteger n)
        {
            ExtendedEuclideanAlgorithm(a, n, out var unused1, out var inverse, out var unused2);

            // Standardize inverse to range 0 <= inv < n
            // (extended euclidean algorithm guarantees that |inv| < n, but not that inv is positive)
            inverse = (inverse + n) % n;

            return inverse;
        }

        /// <summary>
        /// Computes the gcd of a and b
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>The gcd of a and b</returns>
        public static BigInteger GCD(BigInteger a, BigInteger b)
        {
            ExtendedEuclideanAlgorithm(a, b, out var gcd, out var unused1, out var unused2);
            return gcd;
        }

        /// <summary>
        /// Performs extended euclidean algorithm on a and b. Additionally, computes coefficients that satisfy as + bt = r = gcd(a,b).
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="r">r, gcd of a and b</param>
        /// <param name="s">s</param>
        /// <param name="t">t</param>
        public static void ExtendedEuclideanAlgorithm(BigInteger a, BigInteger b, out BigInteger r, out BigInteger s, out BigInteger t)
        {
            BigInteger r0 = a, s0 = 1, t0 = 0;
            BigInteger r1 = b, s1 = 0, t1 = 1;

            while (r1 != 0)
            {
                var q = r0 / r1;

                var temp = r0 - q * r1;
                r0 = r1;
                r1 = temp;

                temp = s0 - q * s1;
                s0 = s1;
                s1 = temp;

                temp = t0 - q * t1;
                t0 = t1;
                t1 = temp;
            }

            r = r0;
            s = s0;
            t = t0;
        }
    }
}
