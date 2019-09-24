using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Nemesis.Essentials.Design
{
    public static class StreamHelpers
    {
        internal const int BUFFER_SIZE = 4096;

        #region Copy / Read / Write
        
        /// <summary>
        /// Reads all bytes that are left to the end of given stream and advances stream position to the end
        /// </summary>
        /// <param name="stream">Underlying stream</param>
        /// <returns>Binary array</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Debug.Listeners.Clear();
        /// Debug.Listeners.Add(new TextWriterTraceListener(Console.Error));
        /// using (var ms = new MemoryStream())            
        /// {
        ///     Func<int, byte> func = i => (byte)((i % 256) ^ 7);
        ///     const ushort length = 5000;
        ///     for (int i = 0; i < length; i++)
        ///         ms.WriteByte(func(i));
        ///     ms.Position = 0;
        ///     var bytes = ms.ReadAllBytes();
        ///
        ///     Debug.Assert(bytes.Length == length, "Length");
        ///     for (int i = 0; i < length; i++)
        ///         Debug.Assert(bytes[i] == func(i), "Failed at " + i + "th position");
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            long? originalPosition = null;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }
            try
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms, BUFFER_SIZE);
                return ms.ToArray();
            }
            finally
            {
                if (stream.CanSeek && originalPosition.HasValue && originalPosition.Value >= 0)
                    stream.Position = originalPosition.Value;
            }
        }

        /// <summary>
        /// Reads all bytes that are left to the end of given stream and advances stream position to the end
        /// </summary>
        /// <param name="reader">Underlying binary reader</param>
        /// <returns>Binary array</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Debug.Listeners.Clear();
        /// Debug.Listeners.Add(new TextWriterTraceListener(Console.Error));
        /// using (var ms = new MemoryStream())
        /// using (var br = new BinaryReader(ms))
        /// {
        ///     Func<int, byte> func = i => (byte)((i % 256) ^ 7);
        ///     const ushort length = 5000;
        ///     for (int i = 0; i < length; i++)
        ///         ms.WriteByte(func(i));
        ///     ms.Position = 0;
        ///     var bytes = br.ReadAllBytes();
        /// 
        ///     Debug.Assert(bytes.Length == length, "Length");
        ///     for (int i = 0; i < length; i++)
        ///         Debug.Assert(bytes[i] == func(i), "Failed at " + i + "th position");
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            using var ms = new MemoryStream();
            int count;
            var buffer = new byte[BUFFER_SIZE];
            do
            {
                count = reader.Read(buffer, 0, BUFFER_SIZE);
                if (count == 0) break;
                ms.Write(buffer, 0, count);
            } while (count > 0);

            return ms.ToArray();
        }

        /// <summary>Asynchronously reads all available bytes from the stream. </summary>
        /// <param name="stream">The stream to read from. </param>
        /// <param name="token">The cancellation token. </param>
        /// <param name="progress">The progress. </param>
        /// <returns>The read byte array. </returns>
        public static Task<byte[]> ReadToEndAsync(this Stream stream, CancellationToken token = default, IProgress<long> progress = null)
        {
            var source = new TaskCompletionSource<byte[]>();
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var buffer = new byte[16 * 1024];

                    using var ms = new MemoryStream();
                    int read;
                    long totalRead = 0;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        token.ThrowIfCancellationRequested();
                        ms.Write(buffer, 0, read);
                        totalRead += read;
                        progress?.Report(totalRead);
                    }
                    source.SetResult(ms.ToArray());
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            }, token);
            return source.Task;
        }

        /// <summary>
        /// Reads a stream line by line
        /// </summary>            
        /// <returns>The lines read given as enumerable</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// Debug.Listeners.Clear();
        /// Debug.Listeners.Add(new TextWriterTraceListener(Console.Error));
        /// using (var ms = new MemoryStream())
        /// using (var bw = new BinaryWriter(ms, Encoding.UTF8))
        /// {
        ///     Func<int, string> func = i =>
        ///        {
        ///            if (i != 0 && i % 255 == 0) return Environment.NewLine;
        ///            else return ((char)(i % ('Z' - 'A' + 1) + 'A')).ToString();
        ///        };
        ///     for (int i = 0; i < 5000; i++)
        ///         bw.Write(Encoding.UTF8.GetBytes(func(i)));
        ///     ms.Position = 0;
        ///     var lines = ms.ReadAllLines().ToList();
        /// 
        ///     Debug.Assert(lines.Count == 20, "Length");
        ///     for (int i = 0; i < lines.Count; i++)
        ///     {
        ///         var line = lines[i];
        ///         Debug.Assert(line.Length == 255, "Failed at " + i + "th position");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<string> ReadAllLines(this Stream stream)
        {
            long? originalPosition = null;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }
            try
            {
#pragma warning disable IDE0067 // Dispose objects before losing scope
                var sr = new StreamReader(stream);
#pragma warning restore IDE0067 // Dispose objects before losing scope
                string line;
                while ((line = sr.ReadLine()) != null)
                    yield return line;
            }
            finally
            {
                if (stream.CanSeek && originalPosition.HasValue && originalPosition.Value >= 0)
                    stream.Position = originalPosition.Value;
            }
        }

        /// <summary>
        /// Reads a complete stream's textual content from current position to end
        /// </summary>            
        /// <returns>The textual contents of the stream</returns>
        public static string ReadAllText(this Stream stream)
        {
            long? originalPosition = null;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }
            try
            {
#pragma warning disable IDE0067 // Dispose objects before losing scope
                var sr = new StreamReader(stream);
#pragma warning restore IDE0067 // Dispose objects before losing scope
                return sr.ReadToEnd();
            }
            finally
            {
                if (stream.CanSeek && originalPosition.HasValue && originalPosition.Value >= 0)
                    stream.Position = originalPosition.Value;
            }
        }

        public static void WriteAllLines(this Stream stream, IEnumerable<string> lines)
        {
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var sw = new StreamWriter(stream);
#pragma warning restore IDE0067 // Dispose objects before losing scope
            foreach (string line in lines)
                sw.WriteLine(line);
            sw.Flush();
        }

        public static void WriteAllText(this Stream stream, string text)
        {
#pragma warning disable IDE0067 // Dispose objects before losing scope
            var sw = new StreamWriter(stream);
#pragma warning restore IDE0067 // Dispose objects before losing scope
            sw.Write(text);
            sw.Flush();
        }

        #endregion

        #region Check file availability

        public static bool TryOpenOrCreateFile(string path, out FileStream fileStream, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            try
            {
                fileStream = null;
                if (string.IsNullOrEmpty(path)) return false;

                fileStream = File.Open(path, FileMode.OpenOrCreate, //or use File.Exists(path) ? FileMode.Open : FileMode.CreateNew
                    access, share);
                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException || ex is UnauthorizedAccessException)
            {
                fileStream = null;
                return false;
            }
        }

        public static FileStream WaitAndOpenFile(string path, TimeSpan timeout = default, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            //if (timeout == default(TimeSpan)) timeout = TimeSpan.Zero;//for clarity only 

            var dt = DateTime.UtcNow;
            FileStream fs;
            while (!TryOpenOrCreateFile(path, out fs, access, share) && DateTime.UtcNow - dt < timeout)
                Thread.Sleep(250); // who knows better way than spin waiting and wants a free cookie? ;)
            return fs;
        }

        internal static bool TryOpenOrCreateFile2(string path, out FileStream fileStream, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            static bool IsFileLockedException(Exception exception)
            {
                const int ERROR_SHARING_VIOLATION = 32;
                const int ERROR_LOCK_VIOLATION = 33;
                int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
                return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
            }

            try
            {
                fileStream = File.Open(path, FileMode.OpenOrCreate, access, share);
                return true;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException || ex is UnauthorizedAccessException)
            {
                fileStream = null;
                if (IsFileLockedException(ex))
                {
                    // do something, eg File.Copy or present the user with a MsgBox - I do not recommend Killing the process that is locking the file
                }
                return false;
            }
        }

        #endregion

        #region Info

        /// <summary>
        /// Return the encoding of a text file
        /// </summary>
        /// <param name="fileName">File to determine encoding from</param>
        /// <returns>Detected file encoding. Returns Encoding.Default if no Unicode BOM (byte order mark) is found</returns>
        public static Encoding GetFileEncoding(string fileName)
        {
            using var fs = File.OpenRead(fileName);
            return GetFileEncoding(fs);
        }

        /// <summary>
        /// Return the encoding of a text file
        /// </summary>
        /// <param name="stream">Stream to determine encoding from</param>
        /// <returns>Detected file encoding. Returns Encoding.Default if no Unicode BOM (byte order mark) is found</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// const string text = @"Alice has a cat and a cat has Alice. アリスと猫が猫のアリスている. ラドクリフ、マラソン五輪代表に1万m出場にも含み ";
        /// Encoding[] encodings = { Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF8 };
        /// 
        /// foreach (var encoding in encodings)
        /// {
        ///     using (var ms = new MemoryStream())
        ///     using (var sw = new StreamWriter(ms, encoding))
        ///     {
        ///         sw.Write(text);sw.Flush();
        ///         Encoding detectedEncoding = ms.GetFileEncoding();
        ///         Debug.Assert(detectedEncoding == encoding, "Encoding detection failed");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Encoding GetFileEncoding(this Stream stream)
        {
            try
            {
                Encoding[] unicodeEncodings = { Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF8 };

                foreach (var encoding in unicodeEncodings)
                {
                    stream.Position = 0;

                    byte[] preamble = encoding.GetPreamble();

                    bool preamblesAreEqual = true;

                    for (int j = 0; preamblesAreEqual && j < preamble.Length; j++)
                        preamblesAreEqual = preamble[j] == stream.ReadByte();

                    if (preamblesAreEqual)
                        return encoding;
                }
            }
            catch (IOException) { }

            return Encoding.Default;
        }

        #endregion
    }
}
