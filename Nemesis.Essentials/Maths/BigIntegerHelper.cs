using System.Diagnostics.Contracts;
using System.Numerics;

namespace Nemesis.Essentials.Maths
{
    //TODO: generate Next and Next min max if not existing in Random extensions
    //TODO: check SecureRandom, -Primality tests from Nemesis, Generate not only positive numbers
    //http://stackoverflow.com/questions/17357760/how-can-i-generate-a-random-biginteger-within-a-certain-range
    public static class BigIntegerHelper
    {
        #region Fields and properties
        private static readonly int[] _primesBelow2000 =
        [
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,
            101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,
            211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293,
            307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,
            401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499,
            503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599,
            601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691,
            701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797,
            809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887,
            907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997,
            1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097,
            1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193,
            1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297,
            1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399,
            1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499,
            1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597,
            1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699,
            1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789,
            1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889,
            1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999
        ];
        #endregion

        #region Maths
        /// <summary>
        /// Generates a random number with the specified number of bits such that gcd(number, this) = 1
        /// </summary>
        public static BigInteger GenerateCoPrime(this BigInteger bi, int bits, Random rand = null)
        {
            rand ??= new Random();

            while (true)
            {
                var result = GenerateRandom(bits, rand);
                var gcd = BigInteger.GreatestCommonDivisor(bi, result);
                if (gcd == 1) return result;
            }
        }

        /// <summary>
        /// Generates a positive BigInteger that is probably prime.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="confidence"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static BigInteger GeneratePseudoPrime(int bits, int confidence = 1024, Random rand = null)
        {
            rand ??= new Random();

            while (true)
            {
                var result = BigInteger.Abs(GenerateRandom(bits, rand));
                if (result.IsEven) result += 1;// make it odd

                if (result.IsProbablePrime(confidence, rand))
                    return result;
            }
        }

        /// <summary>
        /// Determines whether a number is probably prime, using the Rabin-Miller's test. Before applying the test, the number is tested for divisibility by primes &lt; 2000
        /// </summary>
        /// <param name="bi"></param>
        /// <param name="confidence"></param>
        /// <param name="rand"></param>
        /// <returns>false if number is definitely composite (NOT prime), true if number looks like prime</returns>
        [Pure]
        public static bool IsProbablePrime(this BigInteger bi, int confidence = 1024, Random rand = null)
        {
            rand ??= new Random();

            if (bi == 0 || bi.IsEven) return false;

            if (bi < 0) bi = -bi;

            // test for divisibility by primes < 2000
            if (_primesBelow2000.TakeWhile(divisor => divisor < bi).Any(divisor => bi % divisor == 0))
                return false;

            return bi.RabinMillerTest(confidence, rand);
        }

        /// <summary>
        /// Probabilistic prime test based on Rabin-Miller's lemma
        /// </summary>
        /// <param name="n"></param>
        /// <param name="confidence">Number of test rounds</param>
        /// <param name="rand"></param>
        /// <returns>false if number <paramref name="n"/> is definitely composite (NOT prime), true if number <paramref name="n"/> is a strong pseudoprime to randomly chosen bases</returns>
        /// <remarks><![CDATA[Taken from Wikipedia
        /// Input: n > 3, an odd integer to be tested for primality;
        /// Input: k, a parameter that determines the accuracy of the test
        /// Output: composite if n is composite, otherwise probably prime
        /// write n − 1 as 2s·d with d odd by factoring powers of 2 from n − 1
        /// WitnessLoop: repeat k times:
        ///    pick a random integer a in the range [2, n − 2]
        ///    x ← ad mod n
        ///    if x = 1 or x = n − 1 then do next WitnessLoop
        ///    repeat s − 1 times:
        ///       x ← x2 mod n
        ///       if x = 1 then return composite
        ///       if x = n − 1 then do next WitnessLoop
        ///    return composite
        /// return probably prime 
        /// 
        /// 
        /// for any p > 0 with p - 1 = 2^s * t
        /// p is probably prime (strong pseudoprime) if for any a < p,
        /// 1) a^t mod p = 1 or
        /// 2) a^((2^j)*t) mod p = p-1 for some 0 <= j <= s-1
        /// Otherwise, p is composite.
        /// ]]></remarks>
        public static bool RabinMillerTest(this BigInteger n, int confidence = 1024, Random rand = null)
        {
            rand ??= new Random();

            if (n < 0) n = -n;
            if (n == 2 || n == 3) return true;
            if (n == 0 || n == 1 || n.IsEven) return false;

            BigInteger d = n - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            for (int i = 0; i < confidence; i++)
            {
                BigInteger a = 0;
                while (a < 2 || a >= n - 2)
                    a = BigInteger.Abs(GenerateRandom(n.BitCount() - 1, rand));

                var x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == 1) return false;
                    if (x == n - 1) break;
                }

                if (x != n - 1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Generates a new, random BigInteger of the specified length.
        /// </summary>
        /// <param name="bits">The number of bits for the new number.</param>
        /// <param name="random">A random number generator to use to obtain the bits.</param>
        /// <returns>A random number of the specified length.</returns>
        public static BigInteger GenerateRandom(int bits, Random random = null)
        {
            random ??= new Random();

            uint numBytes = (uint)bits >> 3;
            byte numBits = (byte)(bits & 7);

            var randomBytes = new byte[numBytes + (numBits > 0 ? 1 : 0)];
            random.NextBytes(randomBytes);

            if (numBits > 0)
            {
                byte mask = (byte)(0x01 << (numBits - 1));
                randomBytes[randomBytes.Length - 1] |= mask;

                mask = (byte)(0xFF >> (8 - numBits));
                randomBytes[randomBytes.Length - 1] &= mask;
            }
            else
                randomBytes[randomBytes.Length - 1] |= 0xFF;

            return new BigInteger(randomBytes);
        }

        public static BigInteger GetRandom(BigInteger min, BigInteger max, Random random)
        {
            BigInteger diff = BigInteger.Abs(max - min);
            int diffByteSize = diff.ToByteArray().Length;
            byte[] randomBytes = new byte[diffByteSize + 1];
            random.NextBytes(randomBytes);
            BigInteger randomBigInteger = BigInteger.Abs(new BigInteger(randomBytes));
            BigInteger normalizedNumber = randomBigInteger % diff;
            BigInteger rand = normalizedNumber + min;
            return rand;
        }

        /// <summary>
        /// Returns the modulo inverse of number
        /// </summary>
        /// <param name="bi"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        /// <exception cref="ArithmeticException">Thrown when the inverse does not exist. (i.e. gcd(this, modulus) != 1)</exception>
        public static BigInteger ModInverse(this BigInteger bi, BigInteger modulus)
        {
            BigInteger[] p = [0, 1];
            BigInteger[] q = new BigInteger[2];    // quotients
            BigInteger[] r = [0, 0];             // remainders

            int step = 0;

            BigInteger a = modulus;
            BigInteger b = bi;

            while (b != 0)
            {
                if (step > 1)
                {
                    BigInteger pValue = (p[0] - p[1] * q[0]) % modulus;
                    p[0] = p[1];
                    p[1] = pValue;
                }
                BigInteger quotient = BigInteger.DivRem(a, b, out var remainder);

                q[0] = q[1];
                r[0] = r[1];
                q[1] = quotient; r[1] = remainder;

                a = b;
                b = remainder;

                step++;
            }

            if (r[0] != 1) throw (new ArithmeticException("No inverse!"));

            BigInteger result = ((p[0] - (p[1] * q[0])) % modulus);

            if (result < 0) result += modulus;  // get the least positive modulus

            return result;
        }
        #endregion

        #region Bitwise operations
        /// <summary>
        /// Calculate the number of bits needed to store a number
        /// </summary>
        public static int BitCount(this BigInteger bi)
        {
            var bytes = bi.ToByteArray();
            var length = bytes.Length;
            while (length > 0 && bytes[length - 1] == 0) length--;// Normalize length
            if (length == 0) length++;// Check for zero

            int msb = bytes[length - 1]; //get the most significant byte
            int count = 0;
            while (msb != 0)//determine the binary capacity of the most significant byte
            {
                count++;
                msb >>= 1;
            }

            count += (length - 1) << 3; //add the length of remaining binary representation
            return count;
        }

        /// <summary>
        /// Returns the number of set bits in the two's complement representation of this BigInteger that differ from its sign bit.
        /// </summary>
        public static int BitSetCount(this BigInteger bi) => bi.ToByteArray().Sum(CountBits);

        static int CountBits(byte b)
        {
            int count = 0;
            while (b > 0)
            {
                if ((b & 1) == 1) count++;
                b >>= 1;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of set bits in the minimal two's-complement representation of this BigInteger, excluding a sign bit.
        /// </summary>
        public static int BitSetLength(this BigInteger bi) => BitSetCount(BigInteger.Abs(bi));

        /// <summary>
        /// Tests if the specified bit is set.
        /// </summary>
        /// <param name="bi">Number to test</param>
        /// <param name="bitNum">The bit to test. The least significant bit is 0.</param>
        /// <returns>True if bitNum is set to 1, else false.</returns>
        public static bool TestBit(this BigInteger bi, int bitNum)
        {
            if (bitNum < 0) throw new IndexOutOfRangeException("bitNum out of range");
            var bytes = bi.ToByteArray();
            if (bytes.Length == 0) return false;

            uint bytePos = (uint)bitNum >> 3; // divide by 8
            if (bytePos > bytes.Length - 1) return false;
            byte bitPos = (byte)(bitNum & 7); // get the lowest 3 bits

            uint mask = (uint)1 << bitPos;
            return ((bytes[bytePos] | mask) == bytes[bytePos]);
        }
        #endregion

        /*public static IEnumerable<bool> ToBits(this BigInteger bi)
        {
            if (bi == 0) yield return false;
            else
            {
                var bytes = bi.ToByteArray();

                foreach (byte @byte in bytes)
                    for (int bitPos = 0; bitPos < 8; bitPos++)
                    {
                        byte mask = (byte)(1 << bitPos);
                        bool bit = (@byte & mask) == mask; //(@byte | mask) == @byte
                        yield return bit;
                    }
            }
        }

        public static bool[] ToBitArray(this BigInteger bi)
        {
            if (bi == 0) return new[] { false };

            var bytes = bi.ToByteArray();
            var length = bytes.Length;

            var bitList = new List<bool>(length * 8);

            foreach (byte @byte in bytes)
                for (int bitPos = 0; bitPos < 8; bitPos++)
                {
                    byte mask = (byte)(1 << bitPos);
                    bool bit = (@byte & mask) == mask;//(@byte | mask) == @byte
                    bitList.Add(bit);
                }
            return bitList.ToArray();
        }

        /// <summary>
        /// Determines the lowest set bit number
        /// </summary>
        /// <param name="bi">Number to test</param>
        /// <returns>Number of lowest set bit. The least significant bit is 0. If no bit is set then -1 is returned</returns>
        public static int LowestSetBit(this BigInteger bi)
        {
            if (bi == 0) return -1;
            int i = 0;
            while (!bi.TestBit(i)) i++;
            return i;
        }


        public static void ClearBit(this BigInteger bi, int bitNum)
        {
            bi.SetBit(bitNum, false);
        }

        public static BigInteger SetBit(this BigInteger bi, int bitNum, bool value = true)
        {
            var bytes = bi.ToByteArray();
            var length = bytes.Length;

            int bytePos = bitNum >> 3;
            if (bytePos > length - 1)
                bytes = bytes.Concat(Enumerable.Repeat((byte)0, bytePos - length + 1)).ToArray(); //length = bytes.Length;

            //if (bytePos < length)
            byte mask = (byte)(1 << (bitNum & 7));
            if (value)
                bytes[bytePos] |= mask;
            else
                bytes[bytePos] &= (byte)~mask;

            return new BigInteger(bytes);
        }

        /// <summary>
        /// Generates a random number with the specified number of bits such that gcd(number, this) = 1
        /// </summary>
        /// <param name="bi"></param>
        /// <param name="bits"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static BigInteger GenerateCoPrime(this BigInteger bi, int bits, Random rand = null)
        {
            BigInteger result;
            do
            {
                result = GenerateRandom(bits, rand);
            } while (BigInteger.GreatestCommonDivisor(bi, result) != 1);

            return result;
        }

        /// <summary>
        /// Returns a string representing the BigInteger in sign-and-magnitude format in the specified radix.
        /// </summary>
        /// <param name="bi"></param>
        /// <param name="radix"></param>
        /// <returns></returns>
        /// <example>
        /// If the value of BigInteger is -255 in base 10, then ToString(16) returns "-FF"
        /// </example>
        public static string ToString(this BigInteger bi, int radix)
        {
            if (radix < 2 || radix > 36) throw (new ArgumentException("Radix must be >= 2 and <= 36"));

            BigInteger a = bi;

            bool negative = a < 0;
            a = BigInteger.Abs(a);

            if (a == 0)
                return "0";
            else
            {
                var sb = new StringBuilder();
                var biRadix = new BigInteger(radix);
                while (a > 0)
                {
                    BigInteger remainder;
                    BigInteger quotient = BigInteger.DivRem(a, biRadix, out remainder);
                    sb.Insert(0, _charArray[(int)remainder]);
                    a = quotient;
                }
                if (negative) sb.Insert(0, "-");
                return sb.ToString();
            }
        }

        private static readonly char[] _charArray = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        /// <summary>
        /// Parses a number from string in given radix format
        /// </summary>
        /// <param name="value"></param>
        /// <param name="radix"></param>
        /// <returns></returns>
        public static BigInteger Parse(string value, int radix)
        {
            value = (value.ToUpper()).Trim();
            int limit = value[0] == '-' ? 1 : 0;

            var result = new BigInteger(0);
            var multiplier = new BigInteger(1);

            for (int i = value.Length - 1; i >= limit; i--)
            {
                int index = Array.IndexOf(_charArray, value[i]);
                if (index < 0) throw new FormatException("Unsupported character in value string"); // check if character is not in numbering scheme
                if (index >= radix) throw new FormatException("Value contains character not valid for number base"); // check if character is legal for number base and numbering scheme

                result = result + (multiplier * index);
                // overflow check
                if (result < 0) throw new OverflowException();
                multiplier *= radix;
            }
            return limit == 1 ? -result : result;
        }*/
    }
}
