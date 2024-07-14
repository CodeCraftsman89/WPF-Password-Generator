using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PassGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TxtPass_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void GenPassBtn_Click(object sender, RoutedEventArgs e)
        {
            TxtPass.Text = PasswordGenerator.Generate();
        }
    }
    /// <summary>
    /// Base64Url encoder/decoder
    /// </summary>
    public static class Base64Url
    {
        /// <summary>
        /// Encodes the specified byte array.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static string Encode(byte[] arg)
        {
            var s = Convert.ToBase64String(arg); // Standard base64 encoder

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        /// <summary>
        /// Decodes the specified string.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Illegal base64url string!</exception>
        public static byte[] Decode(string arg)
        {
            var s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding

            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default: throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(s); // Standard base64 decoder
        }
    }
    /// <summary>
    /// A class that mimics the standard Random class in the .NET Framework - but uses a random number generator internally.
    /// Taken from IdentityModel (ref.: https://github.com/IdentityModel/IdentityModel/ )
    /// </summary>
    public class CryptoRandom : Random
    {
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
        private readonly byte[] _uint32Buffer = new byte[4];

        /// <summary>
        /// Output format for unique IDs
        /// </summary>
        public enum OutputFormat
        {
            /// <summary>
            /// URL-safe Base64
            /// </summary>
            Base64Url,
            /// <summary>
            /// Base64
            /// </summary>
            Base64,
            /// <summary>
            /// Hex
            /// </summary>
            Hex
        }

        /// <summary>
        /// Creates a random key byte array.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static byte[] CreateRandomKey(int length)
        {
            var bytes = new byte[length];
            Rng.GetBytes(bytes);

            return bytes;
        }

        /// <summary>
        /// Creates a URL safe unique identifier.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="format">The output format</param>
        /// <returns></returns>
        public static string CreateUniqueId(int length = 32, OutputFormat format = OutputFormat.Base64Url)
        {
            var bytes = CreateRandomKey(length);

            switch (format)
            {
                case OutputFormat.Base64Url:
                    return Base64Url.Encode(bytes);
                case OutputFormat.Base64:
                    return Convert.ToBase64String(bytes);
                case OutputFormat.Hex:
                    return BitConverter.ToString(bytes).Replace("-", "");
                default:
                    throw new ArgumentException("Invalid output format", nameof(format));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom"/> class.
        /// </summary>
        public CryptoRandom() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom"/> class.
        /// </summary>
        /// <param name="ignoredSeed">seed (ignored)</param>
        public CryptoRandom(int ignoredSeed) { }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>.
        /// </returns>
        public override int Next()
        {
            Rng.GetBytes(_uint32Buffer);
            return BitConverter.ToInt32(_uint32Buffer, 0) & 0x7FFFFFFF;
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="maxValue"/> is less than zero.
        /// </exception>
        public override int Next(int maxValue)
        {
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue));
            return Next(0, maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
            if (minValue == maxValue) return minValue;
            long diff = maxValue - minValue;

            while (true)
            {
                Rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                long max = (1 + (long)UInt32.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(minValue + (rand % diff));
                }
            }
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        public override double NextDouble()
        {
            Rng.GetBytes(_uint32Buffer);
            uint rand = BitConverter.ToUInt32(_uint32Buffer, 0);
            return rand / (1.0 + uint.MaxValue);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="buffer"/> is null.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            Rng.GetBytes(buffer);
        }
    }
    /*
 * Original version by Stephen Toub and Shawn Farkas.
 * Random pool and thread safety added by Markus Olsson (freakcode.com).
 * 
 * Taken from: https://gist.github.com/niik/1017834
 * Original source: http://msdn.microsoft.com/en-us/magazine/cc163367.aspx
 * 
 * Some benchmarks (2009-03-18):
 * 
 *  Results produced by calling Next() 1 000 000 times on my machine (dual core 3Ghz)
 * 
 *      System.Random completed in 20.4993 ms (avg 0 ms) (first: 0.3454 ms)
 *      CryptoRandom with pool completed in 132.2408 ms (avg 0.0001 ms) (first: 0.025 ms)
 *      CryptoRandom without pool completed in 2 sec 587.708 ms (avg 0.0025 ms) (first: 1.4142 ms)
 *      
 *      |---------------------|------------------------------------|
 *      | Implementation      | Slowdown compared to System.Random |
 *      |---------------------|------------------------------------|
 *      | System.Random       | 0                                  |
 *      | CryptoRand w pool   | 6,6x                               |
 *      | CryptoRand w/o pool | 19,5x                              |
 *      |---------------------|------------------------------------|
 * 
 * ent (http://www.fourmilab.ch/) results for 16mb of data produced by this class:
 * 
 *  > Entropy = 7.999989 bits per byte.
 *  >
 *  > Optimum compression would reduce the size of this 16777216 byte file by 0 percent.
 *  >
 *  > Chi square distribution for 16777216 samples is 260.64, 
 *  > and randomly would exceed this value 50.00 percent of the times.
 *  >
 *  > Arithmetic mean value of data bytes is 127.4974 (127.5 = random).
 *  > Monte Carlo value for Pi is 3.141838823 (error 0.01 percent).
 *  > Serial correlation coefficient is 0.000348 (totally uncorrelated = 0.0).
 * 
 *  your mileage may vary ;)
 *  
 */
    /// <summary>
    /// A random number generator based on the RNGCryptoServiceProvider.
    /// Adapted from the "Tales from the CryptoRandom" article in MSDN Magazine (September 2007)
    /// but with explicit guarantee to be thread safe. Note that this implementation also includes
    /// an optional (enabled by default) random buffer which provides a significant speed boost as
    /// it greatly reduces the amount of calls into unmanaged land.
    /// </summary>
    public class CryptoRandom2 : Random
    {
        private RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        private byte[] _buffer;

        private int _bufferPosition;

        /// <summary>
        /// Gets a value indicating whether this instance has random pool enabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has random pool enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsRandomPoolEnabled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom"/> class with.
        /// Using this overload will enable the random buffer pool.
        /// </summary>
        public CryptoRandom2() : this(true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom"/> class.
        /// This method will disregard whatever value is passed as seed and it's only implemented
        /// in order to be fully backwards compatible with <see cref="System.Random"/>.
        /// Using this overload will enable the random buffer pool.
        /// </summary>
        /// <param name="ignoredSeed">The ignored seed.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ignoredSeed", Justification = "Cannot remove this parameter as we implement the full API of System.Random")]
        public CryptoRandom2(int ignoredSeed) : this(true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandom"/> class with
        /// optional random buffer.
        /// </summary>
        /// <param name="enableRandomPool">set to <c>true</c> to enable the random pool buffer for increased performance.</param>
        public CryptoRandom2(bool enableRandomPool)
        {
            IsRandomPoolEnabled = enableRandomPool;
        }

        private void InitBuffer()
        {
            if (IsRandomPoolEnabled)
            {
                if (_buffer == null || _buffer.Length != 512)
                    _buffer = new byte[512];
            }
            else
            {
                if (_buffer == null || _buffer.Length != 4)
                    _buffer = new byte[4];
            }

            _rng.GetBytes(_buffer);
            _bufferPosition = 0;
        }

        /// <summary>
        /// Returns a nonnegative random number.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>.
        /// </returns>
        public override int Next()
        {
            // Mask away the sign bit so that we always return nonnegative integers
            return (int)GetRandomUInt32() & 0x7FFFFFFF;
        }

        /// <summary>
        /// Returns a nonnegative random number less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to zero.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="maxValue"/> is less than zero.
        /// </exception>
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException("maxValue");

            return Next(0, maxValue);
        }

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
        /// </returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
        /// </exception>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");

            if (minValue == maxValue)
                return minValue;

            long diff = maxValue - minValue;

            while (true)
            {
                uint rand = GetRandomUInt32();

                long max = 1 + (long)uint.MaxValue;
                long remainder = max % diff;

                if (rand < max - remainder)
                    return (int)(minValue + (rand % diff));
            }
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        public override double NextDouble()
        {
            return GetRandomUInt32() / (1.0 + uint.MaxValue);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">An array of bytes to contain random numbers.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="buffer"/> is null.
        /// </exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            lock (this)
            {
                if (IsRandomPoolEnabled && _buffer == null)
                    InitBuffer();

                // Can we fit the requested number of bytes in the buffer?
                if (IsRandomPoolEnabled && _buffer.Length <= buffer.Length)
                {
                    int count = buffer.Length;

                    EnsureRandomBuffer(count);

                    Buffer.BlockCopy(_buffer, _bufferPosition, buffer, 0, count);

                    _bufferPosition += count;
                }
                else
                {
                    // Draw bytes directly from the RNGCryptoProvider
                    _rng.GetBytes(buffer);
                }
            }
        }

        /// <summary>
        /// Gets one random unsigned 32bit integer in a thread safe manner.
        /// </summary>
        private uint GetRandomUInt32()
        {
            lock (this)
            {
                EnsureRandomBuffer(4);

                uint rand = BitConverter.ToUInt32(_buffer, _bufferPosition);

                _bufferPosition += 4;

                return rand;
            }
        }

        /// <summary>
        /// Ensures that we have enough bytes in the random buffer.
        /// </summary>
        /// <param name="requiredBytes">The number of required bytes.</param>
        private void EnsureRandomBuffer(int requiredBytes)
        {
            if (_buffer == null)
                InitBuffer();

            if (requiredBytes > _buffer.Length)
                throw new ArgumentOutOfRangeException("requiredBytes", "cannot be greater than random buffer");

            if ((_buffer.Length - _bufferPosition) < requiredBytes)
                InitBuffer();
        }
    }
    public static class PasswordGenerator
    {
        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        public static string Generate(
            int requiredLength = 8,
            int requiredUniqueChars = 4,
            bool requireDigit = true,
            bool requireLowercase = true,
            bool requireNonAlphanumeric = true,
            bool requireUppercase = true)
        {
            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
            };
            CryptoRandom rand = new CryptoRandom();
            List<char> chars = new List<char>();

            if (requireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (requireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (requireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (requireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < requiredLength
                || chars.Distinct().Count() < requiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}

