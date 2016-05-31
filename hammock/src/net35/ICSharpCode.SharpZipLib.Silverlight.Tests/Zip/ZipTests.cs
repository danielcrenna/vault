using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Silverlight.Checksums;
using ICSharpCode.SharpZipLib.Silverlight.Core;
using ICSharpCode.SharpZipLib.Silverlight.Serialization;
using ICSharpCode.SharpZipLib.Silverlight.Zip;
using ICSharpCode.SharpZipLib.Tests.TestSupport;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace ICSharpCode.SharpZipLib.Tests.Zip
{

    #region Local Support Classes

    internal class RuntimeInfo
    {
        public RuntimeInfo(CompressionMethod method, int compressionLevel, int size, string password, bool getCrc)
        {
            this.method = method;
            this.compressionLevel = compressionLevel;
            this.password = password;
            this.size = size;
            random = false;

            original = new byte[Size];
            if (random)
            {
                var rnd = new Random();
                rnd.NextBytes(original);
            }
            else
            {
                for (var i = 0; i < size; ++i)
                {
                    original[i] = (byte) 'A';
                }
            }

            if (getCrc)
            {
                var crc32 = new Crc32();
                crc32.Update(original, 0, size);
                crc = crc32.Value;
            }
        }


        public RuntimeInfo(string password, bool isDirectory)
        {
            method = CompressionMethod.Stored;
            compressionLevel = 1;
            this.password = password;
            size = 0;
            random = false;
            isDirectory_ = isDirectory;
            original = new byte[0];
        }


        public byte[] Original
        {
            get { return original; }
        }

        public CompressionMethod Method
        {
            get { return method; }
        }

        public int CompressionLevel
        {
            get { return compressionLevel; }
        }

        public int Size
        {
            get { return size; }
        }

        public string Password
        {
            get { return password; }
        }

        private bool Random
        {
            get { return random; }
        }

        public long Crc
        {
            get { return crc; }
        }

        public bool IsDirectory
        {
            get { return isDirectory_; }
        }

        #region Instance Fields

        private readonly int compressionLevel;
        private readonly long crc = -1;
        private readonly bool isDirectory_;
        private readonly CompressionMethod method;
        private readonly byte[] original;
        private readonly string password;
        private readonly bool random;
        private readonly int size;

        #endregion
    }

    internal class MemoryDataSource : IStaticDataSource
    {
        #region Constructors

        /// <summary>
        /// Initialise a new instance.
        /// </summary>
        /// <param name="data">The data to provide.</param>
        public MemoryDataSource(byte[] data)
        {
            data_ = data;
        }

        #endregion

        #region IStaticDataSource Members

        /// <summary>
        /// Get a Stream for this <see cref="DataSource"/>
        /// </summary>
        /// <returns>Returns a <see cref="Stream"/></returns>
        public Stream GetSource()
        {
            return new MemoryStream(data_);
        }

        #endregion

        #region Instance Fields

        private readonly byte[] data_;

        #endregion
    }

    internal class StringMemoryDataSource : MemoryDataSource
    {
        public StringMemoryDataSource(string data)
            : base(Encoding.UTF8.GetBytes(data))
        {
        }
    }

    #endregion

    #region ZipBase

    public class ZipBase
    {
        protected static string GetTempFilePath()
        {
            string result = null;
            try
            {
                result = Path.GetTempPath();
            }
            catch (SecurityException)
            {
            }
            return result;
        }

        protected byte[] MakeInMemoryZip(bool withSeek, params object[] createSpecs)
        {
            MemoryStream ms;

            if (withSeek)
            {
                ms = new MemoryStream();
            }
            else
            {
                ms = new MemoryStreamWithoutSeek();
            }

            using (var outStream = new ZipOutputStream(ms))
            {
                for (var counter = 0; counter < createSpecs.Length; ++counter)
                {
                    var info = createSpecs[counter] as RuntimeInfo;
                    outStream.Password = info.Password;

                    if (info.Method != CompressionMethod.Stored)
                    {
                        outStream.SetLevel(info.CompressionLevel); // 0 - store only to 9 - means best compression
                    }

                    string entryName;

                    if (info.IsDirectory)
                    {
                        entryName = "dir" + counter + "/";
                    }
                    else
                    {
                        entryName = "entry" + counter + ".tst";
                    }

                    var entry = new ZipEntry(entryName);
                    entry.CompressionMethod = info.Method;
                    if (info.Crc >= 0)
                    {
                        entry.Crc = info.Crc;
                    }

                    outStream.PutNextEntry(entry);

                    if (info.Size > 0)
                    {
                        outStream.Write(info.Original, 0, info.Original.Length);
                    }
                }
            }
            return ms.ToArray();
        }

        protected byte[] MakeInMemoryZip(ref byte[] original, CompressionMethod method,
                                         int compressionLevel, int size, string password, bool withSeek)
        {
            MemoryStream ms;

            if (withSeek)
            {
                ms = new MemoryStream();
            }
            else
            {
                ms = new MemoryStreamWithoutSeek();
            }

            using (var outStream = new ZipOutputStream(ms))
            {
                outStream.Password = password;

                if (method != CompressionMethod.Stored)
                {
                    outStream.SetLevel(compressionLevel); // 0 - store only to 9 - means best compression
                }

                var entry = new ZipEntry("dummyfile.tst");
                entry.CompressionMethod = method;

                outStream.PutNextEntry(entry);

                if (size > 0)
                {
                    var rnd = new Random();
                    original = new byte[size];
                    rnd.NextBytes(original);

                    outStream.Write(original, 0, original.Length);
                }
            }
            return ms.ToArray();
        }

        protected static void MakeTempFile(string name, int size)
        {
            using (var fs = File.Create(name))
            {
                var buffer = new byte[4096];
                while (size > 0)
                {
                    fs.Write(buffer, 0, Math.Min(size, buffer.Length));
                    size -= buffer.Length;
                }
            }
        }

        protected static byte ScatterValue(byte rhs)
        {
            return (byte) ((rhs*253 + 7) & 0xff);
        }


        private static void AddKnownDataToEntry(ZipOutputStream zipStream, int size)
        {
            if (size > 0)
            {
                byte nextValue = 0;
                var bufferSize = Math.Min(size, 65536);
                var data = new byte[bufferSize];
                var currentIndex = 0;
                for (var i = 0; i < size; ++i)
                {
                    data[currentIndex] = nextValue;
                    nextValue = ScatterValue(nextValue);

                    currentIndex += 1;
                    if ((currentIndex >= data.Length) || (i + 1 == size))
                    {
                        zipStream.Write(data, 0, currentIndex);
                        currentIndex = 0;
                    }
                }
            }
        }

        public void WriteToFile(string fileName, byte[] data)
        {
            using (var fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        protected static void CheckKnownEntry(Stream inStream, int expectedCount)
        {
            var buffer = new byte[1024];

            int bytesRead;
            var total = 0;
            byte nextValue = 0;
            while ((bytesRead = inStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                total += bytesRead;
                for (var i = 0; i < bytesRead; ++i)
                {
                    Assert.AreEqual(nextValue, buffer[i], "Wrong value read from entry");
                    nextValue = ScatterValue(nextValue);
                }
            }
            Assert.AreEqual(expectedCount, total, "Wrong number of bytes read from entry");
        }

        protected byte ReadByteChecked(Stream stream)
        {
            var rawValue = stream.ReadByte();
            Assert.IsTrue(rawValue >= 0);
            return (byte) rawValue;
        }

        protected int ReadInt(Stream stream)
        {
            return ReadByteChecked(stream) |
                   (ReadByteChecked(stream) << 8) |
                   (ReadByteChecked(stream) << 16) |
                   (ReadByteChecked(stream) << 24);
        }

        protected long ReadLong(Stream stream)
        {
            var result = ReadInt(stream) & 0xffffffff;
            return result | (((long) ReadInt(stream)) << 32);
        }

        #region MakeZipFile Names

        protected void MakeZipFile(Stream storage, bool isOwner, string[] names, int size, string comment)
        {
            using (var zOut = new ZipOutputStream(storage))
            {
                zOut.IsStreamOwner = isOwner;
                zOut.SetComment(comment);
                for (var i = 0; i < names.Length; ++i)
                {
                    zOut.PutNextEntry(new ZipEntry(names[i]));
                    AddKnownDataToEntry(zOut, size);
                }
                zOut.Close();
            }
        }

        protected void MakeZipFile(string name, string[] names, int size, string comment)
        {
            using (var fs = File.Create(name))
            {
                using (var zOut = new ZipOutputStream(fs))
                {
                    zOut.SetComment(comment);
                    for (var i = 0; i < names.Length; ++i)
                    {
                        zOut.PutNextEntry(new ZipEntry(names[i]));
                        AddKnownDataToEntry(zOut, size);
                    }
                    zOut.Close();
                }
                fs.Close();
            }
        }

        #endregion

        #region MakeZipFile Entries

        protected void MakeZipFile(string name, string entryNamePrefix, int entries, int size, string comment)
        {
            using (var fs = File.Create(name))
            {
                using (var zOut = new ZipOutputStream(fs))
                {
                    zOut.SetComment(comment);
                    for (var i = 0; i < entries; ++i)
                    {
                        zOut.PutNextEntry(new ZipEntry(entryNamePrefix + (i + 1)));
                        AddKnownDataToEntry(zOut, size);
                    }
                }
            }
        }

        protected void MakeZipFile(Stream storage, bool isOwner,
                                   string entryNamePrefix, int entries, int size, string comment)
        {
            using (var zOut = new ZipOutputStream(storage))
            {
                zOut.IsStreamOwner = isOwner;
                zOut.SetComment(comment);
                for (var i = 0; i < entries; ++i)
                {
                    zOut.PutNextEntry(new ZipEntry(entryNamePrefix + (i + 1)));
                    AddKnownDataToEntry(zOut, size);
                }
            }
        }

        #endregion
    }

    #endregion

    internal class TestHelper
    {
        public static void SaveMemoryStream(MemoryStream ms, string fileName)
        {
            var data = ms.ToArray();
            using (var fs = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public static int CompareDosDateTimes(DateTime l, DateTime r)
        {
            // Compare dates to dos accuracy...
            // Ticks can be different yet all these values are still the same!
            var result = l.Year - r.Year;
            if (result == 0)
            {
                result = l.Month - r.Month;
                if (result == 0)
                {
                    result = l.Day - r.Day;
                    if (result == 0)
                    {
                        result = l.Hour - r.Hour;
                        if (result == 0)
                        {
                            result = l.Minute - r.Minute;
                            if (result == 0)
                            {
                                result = (l.Second/2) - (r.Second/2);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }

    [TestFixture]
    public class ZipEntryHandling : ZipBase
    {
        private void PiecewiseCompare(ZipEntry lhs, ZipEntry rhs)
        {
            var entryType = typeof (ZipEntry);
            var binding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var fields = entryType.GetFields(binding);

            Assert.Greater(fields.Length, 8, "Failed to find fields");

            foreach (var info in fields)
            {
                var lValue = info.GetValue(lhs);
                var rValue = info.GetValue(rhs);

                Assert.AreEqual(lValue, rValue);
            }
        }

        /// <summary>
        /// Check that cloned entries are correct.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void Cloning()
        {
            long testCrc = 3456;
            long testSize = 99874276;
            long testCompressedSize = 72347;
            var testExtraData = new byte[]{0x00, 0x01, 0x00, 0x02, 0x0EF, 0xFE};
            var testName = "Namu";
            var testFlags = 4567;
            long testDosTime = 23434536;
            var testMethod = CompressionMethod.Deflated;

            var testComment = "A comment";

            var source = new ZipEntry(testName);
            source.Crc = testCrc;
            source.Comment = testComment;
            source.Size = testSize;
            source.CompressedSize = testCompressedSize;
            source.ExtraData = testExtraData;
            source.Flags = testFlags;
            source.DosTime = testDosTime;
            source.CompressionMethod = testMethod;

            var clone = (ZipEntry) source.Clone();

            // Check values against originals
            Assert.AreEqual(testName, clone.Name, "Cloned name mismatch");
            Assert.AreEqual(testCrc, clone.Crc, "Cloned crc mismatch");
            Assert.AreEqual(testComment, clone.Comment, "Cloned comment mismatch");
            Assert.AreEqual(testExtraData, clone.ExtraData, "Cloned Extra data mismatch");
            Assert.AreEqual(testSize, clone.Size, "Cloned size mismatch");
            Assert.AreEqual(testCompressedSize, clone.CompressedSize, "Cloned compressed size mismatch");
            Assert.AreEqual(testFlags, clone.Flags, "Cloned flags mismatch");
            Assert.AreEqual(testDosTime, clone.DosTime, "Cloned DOSTime mismatch");
            Assert.AreEqual(testMethod, clone.CompressionMethod, "Cloned Compression method mismatch");

            // Check against source
            PiecewiseCompare(source, clone);
        }

        /// <summary>
        /// Test that obsolete copy constructor works correctly.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void Copying()
        {
            long testCrc = 3456;
            long testSize = 99874276;
            long testCompressedSize = 72347;
            var testExtraData = new byte[]{0x00, 0x01, 0x00, 0x02, 0x0EF, 0xFE};
            var testName = "Namu";
            var testFlags = 4567;
            long testDosTime = 23434536;
            var testMethod = CompressionMethod.Deflated;

            var testComment = "A comment";

            var source = new ZipEntry(testName);
            source.Crc = testCrc;
            source.Comment = testComment;
            source.Size = testSize;
            source.CompressedSize = testCompressedSize;
            source.ExtraData = testExtraData;
            source.Flags = testFlags;
            source.DosTime = testDosTime;
            source.CompressionMethod = testMethod;

#pragma warning disable 0618
            var clone = new ZipEntry(source);
#pragma warning restore

            PiecewiseCompare(source, clone);
        }

        [Test]
        [Category("Zip")]
        public void DateAndTime()
        {
            var ze = new ZipEntry("Pok");

            // -1 is not strictly a valid MS-DOS DateTime value.
            // ZipEntry is lenient about handling invalid values.
            ze.DosTime = -1;

            Assert.AreEqual(new DateTime(2107, 12, 31, 23, 59, 59), ze.DateTime);

            // 0 is a special value meaning Now.
            ze.DosTime = 0;
            var diff = DateTime.Now - ze.DateTime;

            // Value == 2 seconds!
            ze.DosTime = 1;
            Assert.AreEqual(new DateTime(1980, 1, 1, 0, 0, 2), ze.DateTime);

            // Over the limit are set to max.
            ze.DateTime = new DateTime(2108, 1, 1);
            Assert.AreEqual(new DateTime(2107, 12, 31, 23, 59, 58), ze.DateTime);

            // Under the limit are set to min.
            ze.DateTime = new DateTime(1906, 12, 4);
            Assert.AreEqual(new DateTime(1980, 1, 1, 0, 0, 0), ze.DateTime);
        }

        [Test]
        [Category("Zip")]
        public void DateTimeSetsDosTime()
        {
            var ze = new ZipEntry("Pok");

            var original = ze.DosTime;

            ze.DateTime = new DateTime(1987, 9, 12);
            Assert.AreNotEqual(original, ze.DosTime);
            Assert.AreEqual(0, TestHelper.CompareDosDateTimes(new DateTime(1987, 9, 12), ze.DateTime));
        }

        /// <summary>
        /// Setting entry comments to null should be allowed
        /// </summary>
        [Test]
        [Category("Zip")]
        public void NullEntryComment()
        {
            var test = new ZipEntry("null");
            test.Comment = null;
        }

        /// <summary>
        /// Entries with null names arent allowed
        /// </summary>
        [Test]
        [Category("Zip")]
        [ExpectedException(typeof (ArgumentNullException))]
        public void NullNameInConstructor()
        {
            string name = null;
            var test = new ZipEntry(name);
        }
    }

    /// <summary>
    /// This contains newer tests for stream handling. Much of this is still in GeneralHandling
    /// </summary>
    [TestFixture]
    public class StreamHandling : ZipBase
    {
        private void MustFailRead(Stream s, byte[] buffer, int offset, int count)
        {
            var exception = false;
            try
            {
                s.Read(buffer, offset, count);
            }
            catch
            {
                exception = true;
            }
            Assert.IsTrue(exception, "Read should fail");
        }

        private void Reader()
        {
            const int Size = 8192;
            var readBytes = 1;
            var buffer = new byte[Size];

            var passifierLevel = readTarget_ - 0x10000000;
            var single = inStream_.GetNextEntry();

            Assert.AreEqual(single.Name, "CantSeek");
            Assert.IsTrue((single.Flags & (int) GeneralBitFlags.Descriptor) != 0);

            while ((readTarget_ > 0) && (readBytes > 0))
            {
                var count = Size;
                if (count > readTarget_)
                {
                    count = (int) readTarget_;
                }

                readBytes = inStream_.Read(buffer, 0, count);
                readTarget_ -= readBytes;

                if (readTarget_ <= passifierLevel)
                {
                    Console.WriteLine("Reader {0} bytes remaining", readTarget_);
                    passifierLevel = readTarget_ - 0x10000000;
                }
            }

            Assert.IsTrue(window_.IsClosed, "Window should be closed");

            // This shouldnt read any data but should read the footer
            readBytes = inStream_.Read(buffer, 0, 1);
            Assert.AreEqual(0, readBytes, "Stream should be empty");
            Assert.AreEqual(0, window_.Length, "Window should be closed");
            inStream_.Close();
        }

        private void WriteTargetBytes()
        {
            const int Size = 8192;

            var buffer = new byte[Size];

            while (writeTarget_ > 0)
            {
                var thisTime = Size;
                if (thisTime > writeTarget_)
                {
                    thisTime = (int) writeTarget_;
                }

                outStream_.Write(buffer, 0, thisTime);
                writeTarget_ -= thisTime;
            }
        }

        private void Writer()
        {
            outStream_.PutNextEntry(new ZipEntry("CantSeek"));
            WriteTargetBytes();
            outStream_.Close();
        }

        private WindowedStream window_;
        private ZipOutputStream outStream_;
        private ZipInputStream inStream_;
        private long readTarget_;
        private long writeTarget_;

        /// <summary>
        /// Check that base stream is not closed when IsOwner is false;
        /// </summary>
        [Test]
        public void BaseClosedAfterFailure()
        {
            var ms = new MemoryStreamEx(new byte[32]);

            Assert.IsFalse(ms.IsClosed, "Underlying stream should NOT be closed initially");
            var blewUp = false;
            try
            {
                using (var stream = new ZipOutputStream(ms))
                {
                    Assert.IsTrue(stream.IsStreamOwner, "Should be stream owner by default");
                    try
                    {
                        stream.PutNextEntry(new ZipEntry("Tiny"));
                        stream.Write(new byte[32], 0, 32);
                    }
                    finally
                    {
                        Assert.IsFalse(ms.IsClosed, "Stream should still not be closed.");
                        stream.Close();
                        Assert.Fail("Exception not thrown");
                    }
                }
            }
            catch
            {
                blewUp = true;
            }

            Assert.IsTrue(blewUp, "Should have failed to write to stream");
            Assert.IsTrue(ms.IsClosed, "Underlying stream should be closed");
        }

        /// <summary>
        /// Base stream is closed when IsOwner is true ( default);
        /// </summary>
        [Test]
        public void BaseClosedWhenOwner()
        {
            var ms = new MemoryStreamEx();

            Assert.IsFalse(ms.IsClosed, "Underlying stream should NOT be closed");

            using (var stream = new ZipOutputStream(ms))
            {
                Assert.IsTrue(stream.IsStreamOwner, "Should be stream owner by default");
            }

            Assert.IsTrue(ms.IsClosed, "Underlying stream should be closed");
        }

        /// <summary>
        /// Check that base stream is not closed when IsOwner is false;
        /// </summary>
        [Test]
        public void BaseNotClosedWhenNotOwner()
        {
            var ms = new MemoryStreamEx();

            Assert.IsFalse(ms.IsClosed, "Underlying stream should NOT be closed");

            using (var stream = new ZipOutputStream(ms))
            {
                Assert.IsTrue(stream.IsStreamOwner, "Should be stream owner by default");
                stream.IsStreamOwner = false;
            }
            Assert.IsFalse(ms.IsClosed, "Underlying stream should still NOT be closed");
        }

        /// <summary>
        /// Empty zips can be created and read?
        /// </summary>
        [Test]
        [Category("Zip")]
        public void CreateAndReadEmptyZip()
        {
            var ms = new MemoryStream();
            var outStream = new ZipOutputStream(ms);
            outStream.Finish();

            ms.Seek(0, SeekOrigin.Begin);

            var inStream = new ZipInputStream(ms);
            ZipEntry entry;
            while ((entry = inStream.GetNextEntry()) != null)
            {
                Assert.Fail("No entries should be found in empty zip");
            }
        }

        /// <summary>
        /// Empty zip entries can be created and read?
        /// </summary>
        [Test]
        [Category("Zip")]
        public void EmptyZipEntries()
        {
            var ms = new MemoryStream();
            var outStream = new ZipOutputStream(ms);
            for (var i = 0; i < 10; ++i)
            {
                outStream.PutNextEntry(new ZipEntry(i.ToString()));
            }
            outStream.Finish();

            ms.Seek(0, SeekOrigin.Begin);

            var inStream = new ZipInputStream(ms);

            var extractCount = 0;
            ZipEntry entry;
            var decompressedData = new byte[100];

            while ((entry = inStream.GetNextEntry()) != null)
            {
                while (true)
                {
                    var numRead = inStream.Read(decompressedData, extractCount, decompressedData.Length);
                    if (numRead <= 0)
                    {
                        break;
                    }
                    extractCount += numRead;
                }
            }
            inStream.Close();
            Assert.AreEqual(extractCount, 0, "No data should be read from empty entries");
        }

        /// <summary>
        /// Check that adding an entry with no data and Zip64 works OK
        /// </summary>
        [Test]
        [Category("Zip")]
        public void EntryWithNoDataAndZip64()
        {
            MemoryStream msw = new MemoryStreamWithoutSeek();
            var outStream = new ZipOutputStream(msw);

            outStream.IsStreamOwner = false;
            var ze = new ZipEntry("Striped Marlin");
            ze.ForceZip64();
            ze.Size = 0;
            outStream.PutNextEntry(ze);
            outStream.CloseEntry();
            outStream.Finish();
            outStream.Close();

            var ms = new MemoryStream(msw.ToArray());
            using (var zf = new ZipFile(ms))
            {
                Assert.IsTrue(zf.TestArchive(true));
            }
        }

        [Test]
        [Category("Zip")]
        public void ParameterHandling()
        {
            var buffer = new byte[10];
            var emptyBuffer = new byte[0];

            var ms = new MemoryStream();
            var outStream = new ZipOutputStream(ms);
            outStream.IsStreamOwner = false;
            outStream.PutNextEntry(new ZipEntry("Floyd"));
            outStream.Write(buffer, 0, 10);
            outStream.Finish();

            ms.Seek(0, SeekOrigin.Begin);

            var inStream = new ZipInputStream(ms);
            var e = inStream.GetNextEntry();

            MustFailRead(inStream, null, 0, 0);
            MustFailRead(inStream, buffer, -1, 1);
            MustFailRead(inStream, buffer, 0, 11);
            MustFailRead(inStream, buffer, 7, 5);
            MustFailRead(inStream, buffer, 0, -1);

            MustFailRead(inStream, emptyBuffer, 0, 1);

            var bytesRead = inStream.Read(buffer, 10, 0);
            Assert.AreEqual(0, bytesRead, "Should be able to read zero bytes");

            bytesRead = inStream.Read(emptyBuffer, 0, 0);
            Assert.AreEqual(0, bytesRead, "Should be able to read zero bytes");
        }

        [Test]
        [Category("Zip")]
        [Category("Long Running")]
        public void SingleLargeEntry()
        {
            window_ = new WindowedStream(0x10000);
            outStream_ = new ZipOutputStream(window_);
            inStream_ = new ZipInputStream(window_);

            long target = 0x10000000;
            readTarget_ = writeTarget_ = target;

            var reader = new Thread(Reader);
            reader.Name = "Reader";

            var writer = new Thread(Writer);
            writer.Name = "Writer";

            var startTime = DateTime.Now;
            reader.Start();
            writer.Start();

            writer.Join();
            reader.Join();

            var endTime = DateTime.Now;
            var span = endTime - startTime;
            Console.WriteLine("Time {0} throughput {1} KB/Sec", span, (target/1024)/span.TotalSeconds);
        }

        [Test]
        [Category("Zip")]
        public void WriteThroughput()
        {
            outStream_ = new ZipOutputStream(new NullStream());

            var startTime = DateTime.Now;

            long target = 0x10000000;

            writeTarget_ = target;
            outStream_.PutNextEntry(new ZipEntry("0"));
            WriteTargetBytes();

            outStream_.Close();

            var endTime = DateTime.Now;
            var span = endTime - startTime;
            Console.WriteLine("Time {0} throughput {1} KB/Sec", span, (target/1024)/span.TotalSeconds);
        }

        /// <summary>
        /// Check that Zip64 descriptor is added to an entry OK.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void Zip64Descriptor()
        {
            MemoryStream msw = new MemoryStreamWithoutSeek();
            var outStream = new ZipOutputStream(msw);
            outStream.UseZip64 = UseZip64.Off;

            outStream.IsStreamOwner = false;
            outStream.PutNextEntry(new ZipEntry("StripedMarlin"));
            outStream.WriteByte(89);
            outStream.Close();

            var ms = new MemoryStream(msw.ToArray());
            using (var zf = new ZipFile(ms))
            {
                Assert.IsTrue(zf.TestArchive(true));
            }


            msw = new MemoryStreamWithoutSeek();
            outStream = new ZipOutputStream(msw);
            outStream.UseZip64 = UseZip64.On;

            outStream.IsStreamOwner = false;
            outStream.PutNextEntry(new ZipEntry("StripedMarlin"));
            outStream.WriteByte(89);
            outStream.Close();

            ms = new MemoryStream(msw.ToArray());
            using (var zf = new ZipFile(ms))
            {
                Assert.IsTrue(zf.TestArchive(true));
            }
        }
    }

    [TestFixture]
    public class NameHandling : ZipBase
    {
        private void TestFile(ZipNameTransform t, string original, string expected)
        {
            var transformed = t.TransformFile(original);
            Assert.AreEqual(expected, transformed, "Should be equal");
        }

        [Test]
        [Category("Zip")]
        public void Basic()
        {
            var t = new ZipNameTransform();

            TestFile(t, "abcdef", "abcdef");
            TestFile(t, @"\\uncpath\d1\file1", "file1");
            TestFile(t, @"C:\absolute\file2", "absolute/file2");

            // This is ignored but could be converted to 'file3'
            TestFile(t, @"./file3", "./file3");

            // The following relative paths cant be handled and are ignored
            TestFile(t, @"../file3", "../file3");
            TestFile(t, @".../file3", ".../file3");

            // Trick filenames.
            TestFile(t, @".....file3", ".....file3");
        }

        /// <summary>
        /// Test ZipEntry static file name cleaning methods
        /// </summary>
        [Test]
        [Category("Zip")]
        public void FilenameCleaning()
        {
            Assert.AreEqual(0, string.Compare(ZipEntry.CleanName("hello"), "hello"));
            Assert.AreEqual(0, string.Compare(ZipEntry.CleanName(@"z:\eccles"), "eccles"));
            Assert.AreEqual(0, string.Compare(ZipEntry.CleanName(@"\\server\share\eccles"), "eccles"));
            Assert.AreEqual(0, string.Compare(ZipEntry.CleanName(@"\\server\share\dir\eccles"), "dir/eccles"));
        }

        [Test]
        [Category("Zip")]
        public void NameTransforms()
        {
            INameTransform t = new ZipNameTransform(@"C:\Slippery");
            Assert.AreEqual("Pongo/Directory/", t.TransformDirectory(@"C:\Slippery\Pongo\Directory"),
                            "Value should be trimmed and converted");
            Assert.AreEqual("PoNgo/Directory/", t.TransformDirectory(@"c:\slipperY\PoNgo\Directory"),
                            "Trimming should be case insensitive");
            Assert.AreEqual("slippery/Pongo/Directory/", t.TransformDirectory(@"d:\slippery\Pongo\Directory"),
                            "Trimming should be case insensitive");
        }

        [Test]
        [Category("Zip")]
        public void PathalogicalNames()
        {
            var badName = ".*:\\zy3$";

            Assert.IsFalse(ZipNameTransform.IsValidName(badName));

            var t = new ZipNameTransform();
            var result = t.TransformFile(badName);

            Assert.IsTrue(ZipNameTransform.IsValidName(result));
        }
    }

    /// <summary>
    /// This class contains test cases for Zip compression and decompression
    /// </summary>
    [TestFixture]
    public class GeneralHandling : ZipBase
    {
        private void AddRandomDataToEntry(ZipOutputStream zipStream, int size)
        {
            if (size > 0)
            {
                var data = new byte[size];
                var rnd = new Random();
                rnd.NextBytes(data);

                zipStream.Write(data, 0, data.Length);
            }
        }

        private void ExerciseZip(CompressionMethod method, int compressionLevel,
                                 int size, string password, bool canSeek)
        {
            byte[] originalData = null;
            var compressedData = MakeInMemoryZip(ref originalData, method, compressionLevel, size, password, canSeek);

            var ms = new MemoryStream(compressedData);
            ms.Seek(0, SeekOrigin.Begin);

            using (var inStream = new ZipInputStream(ms))
            {
                var decompressedData = new byte[size];
                if (password != null)
                {
                    inStream.Password = password;
                }

                var entry2 = inStream.GetNextEntry();

                if ((entry2.Flags & 8) == 0)
                {
                    Assert.AreEqual(size, entry2.Size, "Entry size invalid");
                }

                var currentIndex = 0;

                if (size > 0)
                {
                    var count = decompressedData.Length;

                    while (true)
                    {
                        var numRead = inStream.Read(decompressedData, currentIndex, count);
                        if (numRead <= 0)
                        {
                            break;
                        }
                        currentIndex += numRead;
                        count -= numRead;
                    }
                }

                Assert.AreEqual(currentIndex, size, "Original and decompressed data different sizes");

                if (originalData != null)
                {
                    for (var i = 0; i < originalData.Length; ++i)
                    {
                        Assert.AreEqual(decompressedData[i], originalData[i],
                                        "Decompressed data doesnt match original, compression level: " +
                                        compressionLevel);
                    }
                }
            }
        }

        private string DescribeAttributes(FieldAttributes attributes)
        {
            var att = string.Empty;
            if ((FieldAttributes.Public & attributes) != 0)
            {
                att = att + "Public,";
            }

            if ((FieldAttributes.Static & attributes) != 0)
            {
                att = att + "Static,";
            }

            if ((FieldAttributes.Literal & attributes) != 0)
            {
                att = att + "Literal,";
            }

            if ((FieldAttributes.HasDefault & attributes) != 0)
            {
                att = att + "HasDefault,";
            }

            if ((FieldAttributes.InitOnly & attributes) != 0)
            {
                att = att + "InitOnly,";
            }

            if ((FieldAttributes.Assembly & attributes) != 0)
            {
                att = att + "Assembly,";
            }

            if ((FieldAttributes.FamANDAssem & attributes) != 0)
            {
                att = att + "FamANDAssembly,";
            }

            if ((FieldAttributes.FamORAssem & attributes) != 0)
            {
                att = att + "FamORAssembly,";
            }

            if ((FieldAttributes.HasFieldMarshal & attributes) != 0)
            {
                att = att + "HasFieldMarshal,";
            }

            return att;
        }

        private void TestLargeZip(string tempFile, int targetFiles)
        {
            const int BlockSize = 4096;

            var data = new byte[BlockSize];
            byte nextValue = 0;
            for (var i = 0; i < BlockSize; ++i)
            {
                nextValue = ScatterValue(nextValue);
                data[i] = nextValue;
            }

            using (var zFile = new ZipFile(tempFile))
            {
                Assert.AreEqual(targetFiles, zFile.Count);
                var readData = new byte[BlockSize];
                int readIndex;
                foreach (ZipEntry ze in zFile)
                {
                    var s = zFile.GetInputStream(ze);
                    readIndex = 0;
                    while (readIndex < readData.Length)
                    {
                        readIndex += s.Read(readData, readIndex, data.Length - readIndex);
                    }

                    for (var ii = 0; ii < BlockSize; ++ii)
                    {
                        Assert.AreEqual(data[ii], readData[ii]);
                    }
                }
                zFile.Close();
            }
        }

        //      [Test]
        //      [Category("Zip")]
        //      [Category("CreatesTempFile")]
        public void TestLargeZipFile()
        {
            var tempFile = @"g:\\tmp";
            tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
            TestLargeZip(tempFile, 8100);
        }

        //      [Test]
        //      [Category("Zip")]
        //      [Category("CreatesTempFile")]
        public void MakeLargeZipFile()
        {
            string tempFile = null;
            try
            {
                //            tempFile = Path.GetTempPath();
                tempFile = @"g:\\tmp";
            }
            catch (SecurityException)
            {
            }

            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                const int blockSize = 4096;

                var data = new byte[blockSize];
                byte nextValue = 0;
                for (var i = 0; i < blockSize; ++i)
                {
                    nextValue = ScatterValue(nextValue);
                    data[i] = nextValue;
                }

                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                Console.WriteLine("Starting at {0}", DateTime.Now);
                try
                {
                    //               MakeZipFile(tempFile, new String[] {"1", "2" }, int.MaxValue, "C1");
                    using (var fs = File.Create(tempFile))
                    {
                        var zOut = new ZipOutputStream(fs);
                        zOut.SetLevel(4);
                        const int TargetFiles = 8100;
                        for (var i = 0; i < TargetFiles; ++i)
                        {
                            var e = new ZipEntry(i.ToString());
                            e.CompressionMethod = CompressionMethod.Stored;

                            zOut.PutNextEntry(e);
                            for (var block = 0; block < 128; ++block)
                            {
                                zOut.Write(data, 0, blockSize);
                            }
                        }
                        zOut.Close();
                        fs.Close();

                        TestLargeZip(tempFile, TargetFiles);
                    }
                }
                finally
                {
                    Console.WriteLine("Starting at {0}", DateTime.Now);
                    //               File.Delete(tempFile);
                }
            }
        }

        private byte[] ZipZeroLength(ISerializable data)
        {
            var formatter = new XmlFormatter();
            var memStream = new MemoryStream();

            using (var zipStream = new ZipOutputStream(memStream))
            {
                zipStream.PutNextEntry(new ZipEntry("data"));
                formatter.Serialize(zipStream, data);
                zipStream.CloseEntry();
                zipStream.Close();
            }

            var result = memStream.ToArray();
            memStream.Close();

            return result;
        }

        private ISerializable UnZipZeroLength(byte[] zipped)
        {
            if (zipped == null)
            {
                return null;
            }

            object result = null;
            var formatter = new XmlFormatter();
            var memStream = new MemoryStream(zipped);
            using (var zipStream = new ZipInputStream(memStream))
            {
                var zipEntry = zipStream.GetNextEntry();
                if (zipEntry != null)
                {
                    result = formatter.Deserialize(zipStream);
                }
                zipStream.Close();
            }
            memStream.Close();

            return (ISerializable)result;
        }

        private void CheckNameConversion(string toCheck)
        {
            var intermediate = ZipConstants.ConvertToArray(toCheck);
            var final = ZipConstants.ConvertToString(intermediate);

            Assert.AreEqual(toCheck, final, "Expected identical result");
        }

        /// <summary>
        /// Adding an entry after the stream has Finished should fail
        /// </summary>
        [Test]
        [Category("Zip")]
        [ExpectedException(typeof (InvalidOperationException))]
        public void AddEntryAfterFinish()
        {
            var ms = new MemoryStream();
            var s = new ZipOutputStream(ms);
            s.Finish();
            s.PutNextEntry(new ZipEntry("dummyfile.tst"));
        }

        /// <summary>
        /// Basic compress/decompress test, no encryption, size is important here as its big enough
        /// to force multiple write to output which was a problem...
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicDeflated()
        {
            for (var i = 0; i <= 9; ++i)
            {
                ExerciseZip(CompressionMethod.Deflated, i, 50000, null, true);
            }
        }

        /// <summary>
        /// Basic compress/decompress test, with encryption, size is important here as its big enough
        /// to force multiple write to output which was a problem...
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicDeflatedEncrypted()
        {
            for (var i = 0; i <= 9; ++i)
            {
                ExerciseZip(CompressionMethod.Deflated, i, 50000, "Rosebud", true);
            }
        }

        /// <summary>
        /// Basic compress/decompress test, with encryption, size is important here as its big enough
        /// to force multiple write to output which was a problem...
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicDeflatedEncryptedNonSeekable()
        {
            for (var i = 0; i <= 9; ++i)
            {
                ExerciseZip(CompressionMethod.Deflated, i, 50000, "Rosebud", false);
            }
        }

        /// <summary>
        /// Basic compress/decompress test, no encryption, size is important here as its big enough
        /// to force multiple write to output which was a problem...
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicDeflatedNonSeekable()
        {
            for (var i = 0; i <= 9; ++i)
            {
                ExerciseZip(CompressionMethod.Deflated, i, 50000, null, false);
            }
        }

        /// <summary>
        /// Basic stored file test, no encryption.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicStored()
        {
            ExerciseZip(CompressionMethod.Stored, 0, 50000, null, true);
        }

        /// <summary>
        /// Basic stored file test, with encryption.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicStoredEncrypted()
        {
            ExerciseZip(CompressionMethod.Stored, 0, 50000, "Rosebud", true);
        }

        /// <summary>
        /// Basic stored file test, with encryption, non seekable output.
        /// NOTE this gets converted deflate level 0
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicStoredEncryptedNonSeekable()
        {
            ExerciseZip(CompressionMethod.Stored, 0, 50000, "Rosebud", false);
        }

        /// <summary>
        /// Basic stored file test, no encryption, non seekable output
        /// NOTE this gets converted to deflate level 0
        /// </summary>
        [Test]
        [Category("Zip")]
        public void BasicStoredNonSeekable()
        {
            ExerciseZip(CompressionMethod.Stored, 0, 50000, null, false);
        }

        /// <summary>
        /// Check that simply closing ZipOutputStream finishes the zip correctly
        /// </summary>
        [Test]
        [Category("Zip")]
        public void CloseOnlyHandled()
        {
            var ms = new MemoryStream();
            var s = new ZipOutputStream(ms);
            s.PutNextEntry(new ZipEntry("dummyfile.tst"));
            s.Close();

            Assert.IsTrue(s.IsFinished, "Output stream should be finished");
        }

        /// <summary>
        /// Check that GetNextEntry can handle the situation where part of the entry data has been read
        /// before the call is made.  ZipInputStream.CloseEntry wasnt handling this at all.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void ExerciseGetNextEntry()
        {
            var compressedData = MakeInMemoryZip(
                true,
                new RuntimeInfo(CompressionMethod.Deflated, 9, 50, null, true),
                new RuntimeInfo(CompressionMethod.Deflated, 2, 50, null, true),
                new RuntimeInfo(CompressionMethod.Deflated, 9, 50, null, true),
                new RuntimeInfo(CompressionMethod.Deflated, 2, 50, null, true),
                new RuntimeInfo(null, true),
                new RuntimeInfo(CompressionMethod.Stored, 2, 50, null, true),
                new RuntimeInfo(CompressionMethod.Deflated, 9, 50, null, true)
                );

            var ms = new MemoryStream(compressedData);
            ms.Seek(0, SeekOrigin.Begin);

            using (var inStream = new ZipInputStream(ms))
            {
                var buffer = new byte[10];

                ZipEntry entry;
                while ((entry = inStream.GetNextEntry()) != null)
                {
                    // Read a portion of the data, so GetNextEntry has some work to do.
                    inStream.Read(buffer, 0, 10);
                }
            }
        }

        /// <summary>
        /// Invalid passwords should be detected early if possible, non seekable stream
        /// </summary>
        [Test]
        [Category("Zip")]
        [ExpectedException(typeof (ZipException))]
        public void InvalidPasswordNonSeekable()
        {
            byte[] originalData = null;
            var compressedData = MakeInMemoryZip(ref originalData, CompressionMethod.Deflated, 3, 500, "Hola", false);

            var ms = new MemoryStream(compressedData);
            ms.Seek(0, SeekOrigin.Begin);

            var buf2 = new byte[originalData.Length];
            var pos = 0;

            var inStream = new ZipInputStream(ms);
            inStream.Password = "redhead";

            var entry2 = inStream.GetNextEntry();

            while (true)
            {
                var numRead = inStream.Read(buf2, pos, buf2.Length);
                if (numRead <= 0)
                {
                    break;
                }
                pos += numRead;
            }
        }

        /// <summary>
        /// Invalid passwords should be detected early if possible, seekable stream
        /// </summary>
        [Test]
        [Category("Zip")]
        [ExpectedException(typeof (ZipException))]
        public void InvalidPasswordSeekable()
        {
            byte[] originalData = null;
            var compressedData = MakeInMemoryZip(ref originalData, CompressionMethod.Deflated, 3, 500, "Hola", true);

            var ms = new MemoryStream(compressedData);
            ms.Seek(0, SeekOrigin.Begin);

            var buf2 = new byte[originalData.Length];
            var pos = 0;

            var inStream = new ZipInputStream(ms);
            inStream.Password = "redhead";

            var entry2 = inStream.GetNextEntry();

            while (true)
            {
                var numRead = inStream.Read(buf2, pos, buf2.Length);
                if (numRead <= 0)
                {
                    break;
                }
                pos += numRead;
            }
        }

        [Test]
        [Category("Zip")]
        public void MixedEncryptedAndPlain()
        {
            var compressedData = MakeInMemoryZip(true,
                                                 new RuntimeInfo(CompressionMethod.Deflated, 2, 1, null, true),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 9, 1, "1234", false),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 2, 1, null, false),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 9, 1, "1234", true)
                );

            var ms = new MemoryStream(compressedData);
            using (var inStream = new ZipInputStream(ms))
            {
                inStream.Password = "1234";

                var extractCount = 0;
                var extractIndex = 0;
                ZipEntry entry;
                var decompressedData = new byte[100];

                while ((entry = inStream.GetNextEntry()) != null)
                {
                    extractCount = decompressedData.Length;
                    extractIndex = 0;
                    while (true)
                    {
                        var numRead = inStream.Read(decompressedData, extractIndex, extractCount);
                        if (numRead <= 0)
                        {
                            break;
                        }
                        extractIndex += numRead;
                        extractCount -= numRead;
                    }
                }
                inStream.Close();
            }
        }

        [Test]
        [Category("Zip")]
        public void NameConversion()
        {
            CheckNameConversion("Hello");
            CheckNameConversion("a/b/c/d/e/f/g/h/SomethingLikeAnArchiveName.txt");
        }

        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void PartialStreamClosing()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                MakeZipFile(tempFile, new[]{"Farriera", "Champagne", "Urban myth"}, 10, "Aha");

                using (var zipFile = new ZipFile(tempFile))
                {
                    var stream = zipFile.GetInputStream(0);
                    stream.Close();

                    stream = zipFile.GetInputStream(1);
                    zipFile.Close();
                }
                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Regression test for problem where the password check would fail for an archive whose
        /// date was updated from the extra data.
        /// This applies to archives where the crc wasnt know at the time of encryption.
        /// The date of the entry is used in its place.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void PasswordCheckingWithDateInExtraData()
        {
            var ms = new MemoryStream();
            var checkTime = new DateTime(2010, 10, 16, 0, 3, 28);

            using (var zos = new ZipOutputStream(ms))
            {
                zos.IsStreamOwner = false;
                zos.Password = "secret";
                var ze = new ZipEntry("uno");
                ze.DateTime = new DateTime(1998, 6, 5, 4, 3, 2);

                var zed = new ZipExtraData();

                zed.StartNewEntry();

                zed.AddData(1);

                var delta = checkTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();
                var seconds = (int) delta.TotalSeconds;
                zed.AddLeInt(seconds);
                zed.AddNewEntry(0x5455);

                ze.ExtraData = zed.GetEntryData();
                zos.PutNextEntry(ze);
                zos.WriteByte(54);
            }

            ms.Position = 0;
            using (var zis = new ZipInputStream(ms))
            {
                zis.Password = "secret";
                var uno = zis.GetNextEntry();
                var theByte = (byte) zis.ReadByte();
                Assert.AreEqual(54, theByte);
                Assert.AreEqual(-1, zis.ReadByte());
                Assert.AreEqual(checkTime, uno.DateTime);
            }
        }

        /// <summary>
        /// Test for handling of serialized reference and value objects.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void SerializedObject()
        {
            var sampleDateTime = new SerializableDateTime(1853, 8, 26);
            ISerializable data = sampleDateTime;

            var zipped = ZipZeroLength(data);
            var rawObject = UnZipZeroLength(zipped);

            var returnedDateTime = (SerializableDateTime) rawObject;

            Assert.AreEqual(sampleDateTime, returnedDateTime);

            var sampleString = new SerializableString("Mary had a giant cat its ears were green and smelly");
            zipped = ZipZeroLength(sampleString);

            rawObject = UnZipZeroLength(zipped);

            var returnedString = rawObject as SerializableString;

            Assert.AreEqual(sampleString, returnedString);
        }

        /// <summary>
        /// Test for handling of zero lengths in compression using a formatter which
        /// will request reads of zero length...
        /// </summary>
        [Test]
        [Category("Zip")]
        public void SerializedObjectZeroLength()
        {
//            object data = new SerializableByteArray[0];
//            // This wont be zero length here due to serialisation.
//            var zipped = ZipZeroLength(data);
//            var o = UnZipZeroLength(zipped);
//
//            var returned = o as byte[];
//
//            Assert.IsNotNull(returned, "Expected a byte[]");
//            Assert.AreEqual(0, returned.Length);
        }

        /// <summary>
        /// Test setting file commment to a value that is too long
        /// </summary>
        [Test]
        [Category("Zip")]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void SetCommentOversize()
        {
            var ms = new MemoryStream();
            var s = new ZipOutputStream(ms);
            s.SetComment(new String('A', 65536));
        }

        [Test]
        [Category("Zip")]
        public void SkipEncryptedEntriesWithoutSettingPassword()
        {
            var compressedData = MakeInMemoryZip(true,
                                                 new RuntimeInfo("1234", true),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 2, 1, null, true),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 9, 1, "1234", true),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 2, 1, null, true),
                                                 new RuntimeInfo(null, true),
                                                 new RuntimeInfo(CompressionMethod.Stored, 2, 1, "4321", true),
                                                 new RuntimeInfo(CompressionMethod.Deflated, 9, 1, "1234", true)
                );

            var ms = new MemoryStream(compressedData);
            var inStream = new ZipInputStream(ms);

            ZipEntry entry;
            while ((entry = inStream.GetNextEntry()) != null)
            {
            }

            inStream.Close();
        }

        /// <summary>
        /// Check that when the output stream cannot seek that requests for stored
        /// are in fact converted to defalted level 0
        /// </summary>
        [Test]
        [Category("Zip")]
        public void StoredNonSeekableConvertToDeflate()
        {
            var ms = new MemoryStreamWithoutSeek();

            var outStream = new ZipOutputStream(ms);
            outStream.SetLevel(8);
            Assert.AreEqual(8, outStream.GetLevel(), "Compression level invalid");

            var entry = new ZipEntry("1.tst");
            entry.CompressionMethod = CompressionMethod.Stored;
            outStream.PutNextEntry(entry);
            Assert.AreEqual(0, outStream.GetLevel(), "Compression level invalid");

            AddRandomDataToEntry(outStream, 100);
            entry = new ZipEntry("2.tst");
            entry.CompressionMethod = CompressionMethod.Deflated;
            outStream.PutNextEntry(entry);
            Assert.AreEqual(8, outStream.GetLevel(), "Compression level invalid");
            AddRandomDataToEntry(outStream, 100);

            outStream.Close();
        }

        /// <summary>
        /// Check that adding more than the 2.0 limit for entry numbers is detected and handled
        /// </summary>
        [Test]
        [Category("Zip")]
        [Category("Long Running")]
        public void Stream_64KPlusOneEntries()
        {
            const int target = 65537;
            var ms = new MemoryStream();
            using (var s = new ZipOutputStream(ms))
            {
                for (var i = 0; i < target; ++i)
                {
                    s.PutNextEntry(new ZipEntry("dummyfile.tst"));
                }

                s.Finish();
                ms.Seek(0, SeekOrigin.Begin);
                using (var zipFile = new ZipFile(ms))
                {
                    Assert.AreEqual(target, zipFile.Count, "Incorrect number of entries stored");
                }
            }
        }

        /// <summary>
        /// Check that Unicode filename support works.
        /// </summary>
        [Test]
        [Category("Zip")]
        public void Stream_UnicodeEntries()
        {
            var ms = new MemoryStream();
            using (var s = new ZipOutputStream(ms))
            {
                s.IsStreamOwner = false;

                var sampleName = "\u03A5\u03d5\u03a3";
                var sample = new ZipEntry(sampleName);
                sample.IsUnicodeText = true;
                s.PutNextEntry(sample);

                s.Finish();
                ms.Seek(0, SeekOrigin.Begin);

                using (var zis = new ZipInputStream(ms))
                {
                    var ze = zis.GetNextEntry();
                    Assert.AreEqual(sampleName, ze.Name, "Expected name to match original");
                    Assert.IsTrue(ze.IsUnicodeText, "Expected IsUnicodeText flag to be set");
                }
            }
        }

        [Test]
        [Category("Zip")]
        public void UnicodeNameConversion()
        {
            var sample = "Hello world";

            byte[] rawData = Encoding.UTF8.GetBytes(sample);

            var converted = ZipConstants.ConvertToStringExt(0, rawData);
            Assert.AreEqual(sample, converted);

            converted = ZipConstants.ConvertToStringExt((int) GeneralBitFlags.UnicodeText, rawData);
            Assert.AreEqual(sample, converted);

            // This time use some greek characters
            sample = "\u03A5\u03d5\u03a3";
            rawData = Encoding.UTF8.GetBytes(sample);

            converted = ZipConstants.ConvertToStringExt((int) GeneralBitFlags.UnicodeText, rawData);
            Assert.AreEqual(sample, converted);
        }

        [Test]
        [Category("Zip")]
        [ExpectedException(typeof (NotSupportedException))]
        public void UnsupportedCompressionMethod()
        {
            var ze = new ZipEntry("HumblePie");
            var type = typeof (CompressionMethod);
            //			System.Reflection.FieldInfo[] info = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var info = type.GetFields();

            var aValue = CompressionMethod.Deflated;
            for (var i = 0; i < info.Length; i++)
            {
                var attributes = info[i].Attributes;
                DescribeAttributes(attributes);
                if ((FieldAttributes.Static & attributes) != 0)
                {
                    var obj = info[i].GetValue(null);
                    var bb = obj.ToString();
                    if (bb == null)
                    {
                        throw new Exception();
                    }
                }
                var x = string.Format("The value of {0} is: {1}",
                                      info[i].Name, info[i].GetValue(aValue));
            }

            ze.CompressionMethod = CompressionMethod.BZip2;
        }
    }

    [TestFixture]
    public class ZipExtraDataHandling : ZipBase
    {
        [Test]
        [Category("Zip")]
        public void BasicOperations()
        {
            var zed = new ZipExtraData(null);
            Assert.AreEqual(0, zed.Length);

            zed = new ZipExtraData(new byte[]{1, 0, 0, 0});
            Assert.AreEqual(4, zed.Length, "A length should be 4");

            var zed2 = new ZipExtraData();
            Assert.AreEqual(0, zed2.Length);

            zed2.AddEntry(1, new byte[]{});

            var data = zed.GetEntryData();
            for (var i = 0; i < data.Length; ++i)
            {
                Assert.AreEqual(zed2.GetEntryData()[i], data[i]);
            }

            Assert.AreEqual(4, zed2.Length, "A1 length should be 4");

            var findResult = zed.Find(2);
            Assert.IsFalse(findResult, "A - Shouldnt find tag 2");

            findResult = zed.Find(1);
            Assert.IsTrue(findResult, "A - Should find tag 1");
            Assert.AreEqual(0, zed.ValueLength, "A- Length of entry should be 0");
            Assert.AreEqual(-1, zed.ReadByte());
            Assert.AreEqual(0, zed.GetStreamForTag(1).Length, "A - Length of stream should be 0");

            zed = new ZipExtraData(new byte[]{1, 0, 3, 0, 1, 2, 3});
            Assert.AreEqual(7, zed.Length, "Expected a length of 7");

            findResult = zed.Find(1);
            Assert.IsTrue(findResult, "B - Should find tag 1");
            Assert.AreEqual(3, zed.ValueLength, "B - Length of entry should be 3");
            for (var i = 1; i <= 3; ++i)
            {
                Assert.AreEqual(i, zed.ReadByte());
            }
            Assert.AreEqual(-1, zed.ReadByte());

            var s = zed.GetStreamForTag(1);
            Assert.AreEqual(3, s.Length, "B.1 Stream length should be 3");
            for (var i = 1; i <= 3; ++i)
            {
                Assert.AreEqual(i, s.ReadByte());
            }
            Assert.AreEqual(-1, s.ReadByte());

            zed = new ZipExtraData(new byte[]{1, 0, 3, 0, 1, 2, 3, 2, 0, 1, 0, 56});
            Assert.AreEqual(12, zed.Length, "Expected a length of 12");

            findResult = zed.Find(1);
            Assert.IsTrue(findResult, "C.1 - Should find tag 1");
            Assert.AreEqual(3, zed.ValueLength, "C.1 - Length of entry should be 3");
            for (var i = 1; i <= 3; ++i)
            {
                Assert.AreEqual(i, zed.ReadByte());
            }
            Assert.AreEqual(-1, zed.ReadByte());

            findResult = zed.Find(2);
            Assert.IsTrue(findResult, "C.2 - Should find tag 2");
            Assert.AreEqual(1, zed.ValueLength, "C.2 - Length of entry should be 1");
            Assert.AreEqual(56, zed.ReadByte());
            Assert.AreEqual(-1, zed.ReadByte());

            s = zed.GetStreamForTag(2);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(56, s.ReadByte());
            Assert.AreEqual(-1, s.ReadByte());

            zed = new ZipExtraData();
            zed.AddEntry(7, new byte[]{33, 44, 55});
            findResult = zed.Find(7);
            Assert.IsTrue(findResult, "Add.1 should find new tag");
            Assert.AreEqual(3, zed.ValueLength, "Add.1 length should be 3");
            Assert.AreEqual(33, zed.ReadByte());
            Assert.AreEqual(44, zed.ReadByte());
            Assert.AreEqual(55, zed.ReadByte());
            Assert.AreEqual(-1, zed.ReadByte());

            zed.AddEntry(7, null);
            findResult = zed.Find(7);
            Assert.IsTrue(findResult, "Add.2 should find new tag");
            Assert.AreEqual(0, zed.ValueLength, "Add.2 length should be 0");

            zed.StartNewEntry();
            zed.AddData(0xae);
            zed.AddNewEntry(55);

            findResult = zed.Find(55);
            Assert.IsTrue(findResult, "Add.3 should find new tag");
            Assert.AreEqual(1, zed.ValueLength, "Add.3 length should be 1");
            Assert.AreEqual(0xae, zed.ReadByte());
            Assert.AreEqual(-1, zed.ReadByte());

            zed = new ZipExtraData();
            zed.StartNewEntry();
            zed.AddLeLong(0);
            zed.AddLeLong(-4);
            zed.AddLeLong(-1);
            zed.AddLeLong(long.MaxValue);
            zed.AddLeLong(long.MinValue);
            zed.AddLeLong(0x123456789ABCDEF0);
            zed.AddLeLong(unchecked((long) 0xFEDCBA9876543210));
            zed.AddNewEntry(567);

            s = zed.GetStreamForTag(567);
            var longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(0, longValue, "Expected long value of zero");

            longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(-4, longValue, "Expected long value of -4");

            longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(-1, longValue, "Expected long value of -1");

            longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(long.MaxValue, longValue, "Expected long value of MaxValue");

            longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(long.MinValue, longValue, "Expected long value of MinValue");

            longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(0x123456789abcdef0, longValue, "Expected long value of MinValue");

            longValue = ReadLong(s);
            Assert.AreEqual(longValue, zed.ReadLong(), "Read/stream mismatch");
            Assert.AreEqual(unchecked((long) 0xFEDCBA9876543210), longValue, "Expected long value of MinValue");
        }

        [Test]
        [Category("Zip")]
        public void Deleting()
        {
            var zed = new ZipExtraData();
            Assert.AreEqual(0, zed.Length);

            // Tag 1 Totoal length 10
            zed.AddEntry(1, new byte[]{10, 11, 12, 13, 14, 15});
            Assert.AreEqual(10, zed.Length, "Length should be 10");
            Assert.AreEqual(10, zed.GetEntryData().Length, "Data length should be 10");

            // Tag 2 total length  9
            zed.AddEntry(2, new byte[]{20, 21, 22, 23, 24});
            Assert.AreEqual(19, zed.Length, "Length should be 19");
            Assert.AreEqual(19, zed.GetEntryData().Length, "Data length should be 19");

            // Tag 3 Total Length 6
            zed.AddEntry(3, new byte[]{30, 31});
            Assert.AreEqual(25, zed.Length, "Length should be 25");
            Assert.AreEqual(25, zed.GetEntryData().Length, "Data length should be 25");

            zed.Delete(2);
            Assert.AreEqual(16, zed.Length, "Length should be 16");
            Assert.AreEqual(16, zed.GetEntryData().Length, "Data length should be 16");

            // Tag 2 total length  9
            zed.AddEntry(2, new byte[]{20, 21, 22, 23, 24});
            Assert.AreEqual(25, zed.Length, "Length should be 25");
            Assert.AreEqual(25, zed.GetEntryData().Length, "Data length should be 25");

            zed.AddEntry(3, null);
            Assert.AreEqual(23, zed.Length, "Length should be 23");
            Assert.AreEqual(23, zed.GetEntryData().Length, "Data length should be 23");
        }

        [Test]
        [Category("Zip")]
        public void ExceedSize()
        {
            var zed = new ZipExtraData();
            var buffer = new byte[65506];
            zed.AddEntry(1, buffer);
            Assert.AreEqual(65510, zed.Length);
            zed.AddEntry(2, new byte[21]);
            Assert.AreEqual(65535, zed.Length);

            var caught = false;
            try
            {
                zed.AddEntry(3, null);
            }
            catch
            {
                caught = true;
            }
            Assert.IsTrue(caught, "Expected an exception when max size exceeded");
            Assert.AreEqual(65535, zed.Length);

            zed.Delete(2);
            Assert.AreEqual(65510, zed.Length);

            caught = false;
            try
            {
                zed.AddEntry(2, new byte[22]);
            }
            catch
            {
                caught = true;
            }
            Assert.IsTrue(caught, "Expected an exception when max size exceeded");
            Assert.AreEqual(65510, zed.Length);
        }

        /// <summary>
        /// Extra data for separate entries should be unique to that entry
        /// </summary>
        [Test]
        [Category("Zip")]
        public void IsDataUnique()
        {
            var a = new ZipEntry("Basil");
            var extra = new byte[4];
            extra[0] = 27;
            a.ExtraData = extra;

            var b = (ZipEntry) a.Clone();
            b.ExtraData[0] = 89;
            Assert.IsTrue(b.ExtraData[0] != a.ExtraData[0],
                          "Extra data not unique " + b.ExtraData[0] + " " + a.ExtraData[0]);

            var c = (ZipEntry) a.Clone();
            c.ExtraData[0] = 45;
            Assert.IsTrue(a.ExtraData[0] != c.ExtraData[0],
                          "Extra data not unique " + a.ExtraData[0] + " " + c.ExtraData[0]);
        }

        [Test]
        [Category("Zip")]
        public void ReadOverrunInt()
        {
            var zed = new ZipExtraData(new byte[]{1, 0, 0, 0});
            Assert.AreEqual(4, zed.Length, "Length should be 4");
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            // Empty Tag
            var exceptionCaught = false;
            try
            {
                zed.ReadInt();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");

            // three bytes
            zed = new ZipExtraData(new byte[]{1, 0, 3, 0, 1, 2, 3});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            exceptionCaught = false;
            try
            {
                zed.ReadInt();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");

            zed = new ZipExtraData(new byte[]{1, 0, 7, 0, 1, 2, 3, 4, 5, 6, 7});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            zed.ReadInt();

            exceptionCaught = false;
            try
            {
                zed.ReadInt();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");
        }

        [Test]
        [Category("Zip")]
        public void ReadOverrunLong()
        {
            var zed = new ZipExtraData(new byte[]{1, 0, 0, 0});
            Assert.AreEqual(4, zed.Length, "Length should be 4");
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            // Empty Tag
            var exceptionCaught = false;
            try
            {
                zed.ReadLong();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");

            // seven bytes
            zed = new ZipExtraData(new byte[]{1, 0, 7, 0, 1, 2, 3, 4, 5, 6, 7});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            exceptionCaught = false;
            try
            {
                zed.ReadLong();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");

            zed = new ZipExtraData(new byte[]{1, 0, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            zed.ReadLong();

            exceptionCaught = false;
            try
            {
                zed.ReadLong();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");
        }

        [Test]
        [Category("Zip")]
        public void ReadOverrunShort()
        {
            var zed = new ZipExtraData(new byte[]{1, 0, 0, 0});
            Assert.AreEqual(4, zed.Length, "Length should be 4");
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            // Empty Tag
            var exceptionCaught = false;
            try
            {
                zed.ReadShort();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");

            // Single byte
            zed = new ZipExtraData(new byte[]{1, 0, 1, 0, 1});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            exceptionCaught = false;
            try
            {
                zed.ReadShort();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");

            zed = new ZipExtraData(new byte[]{1, 0, 2, 0, 1, 2});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            zed.ReadShort();

            exceptionCaught = false;
            try
            {
                zed.ReadShort();
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Expected EOS exception");
        }

        [Test]
        [Category("Zip")]
        public void Skipping()
        {
            var zed = new ZipExtraData(new byte[]{1, 0, 7, 0, 1, 2, 3, 4, 5, 6, 7});
            Assert.AreEqual(11, zed.Length, "Length should be 11");
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            Assert.AreEqual(7, zed.UnreadCount);
            Assert.AreEqual(4, zed.CurrentReadIndex);

            zed.ReadByte();
            Assert.AreEqual(6, zed.UnreadCount);
            Assert.AreEqual(5, zed.CurrentReadIndex);

            zed.Skip(1);
            Assert.AreEqual(5, zed.UnreadCount);
            Assert.AreEqual(6, zed.CurrentReadIndex);

            zed.Skip(-1);
            Assert.AreEqual(6, zed.UnreadCount);
            Assert.AreEqual(5, zed.CurrentReadIndex);

            zed.Skip(6);
            Assert.AreEqual(0, zed.UnreadCount);
            Assert.AreEqual(11, zed.CurrentReadIndex);

            var exceptionCaught = false;

            try
            {
                zed.Skip(1);
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Should fail to skip past end");

            Assert.AreEqual(0, zed.UnreadCount);
            Assert.AreEqual(11, zed.CurrentReadIndex);

            zed.Skip(-7);
            Assert.AreEqual(7, zed.UnreadCount);
            Assert.AreEqual(4, zed.CurrentReadIndex);

            try
            {
                zed.Skip(-1);
            }
            catch (ZipException)
            {
                exceptionCaught = true;
            }
            Assert.IsTrue(exceptionCaught, "Should fail to skip before beginning");
        }

        [Test]
        [Category("Zip")]
        public void TaggedDataHandling()
        {
            var tagData = new NTTaggedData();
            var modTime = tagData.LastModificationTime;
            var rawData = tagData.GetData();
            tagData.LastModificationTime = tagData.LastModificationTime + TimeSpan.FromSeconds(40);
            tagData.SetData(rawData, 0, rawData.Length);
            Assert.AreEqual(10, tagData.TagID, "TagID mismatch");
            Assert.AreEqual(modTime, tagData.LastModificationTime, "NT Mod time incorrect");

            tagData.CreateTime = DateTime.FromFileTimeUtc(0);
            tagData.LastAccessTime = new DateTime(9999, 12, 31, 23, 59, 59);
            rawData = tagData.GetData();

            var unixData = new ExtendedUnixData();
            modTime = unixData.ModificationTime;
            unixData.ModificationTime = modTime; // Ensure flag is set.

            rawData = unixData.GetData();
            unixData.ModificationTime += TimeSpan.FromSeconds(100);
            unixData.SetData(rawData, 0, rawData.Length);
            Assert.AreEqual(0x5455, unixData.TagID, "TagID mismatch");
            Assert.AreEqual(modTime, unixData.ModificationTime, "Unix mod time incorrect");
        }

        [Test]
        [Category("Zip")]
        public void UnreadCountValid()
        {
            var zed = new ZipExtraData(new byte[]{1, 0, 0, 0});
            Assert.AreEqual(4, zed.Length, "Length should be 4");
            Assert.IsTrue(zed.Find(1), "Should find tag 1");
            Assert.AreEqual(0, zed.UnreadCount);

            // seven bytes
            zed = new ZipExtraData(new byte[]{1, 0, 7, 0, 1, 2, 3, 4, 5, 6, 7});
            Assert.IsTrue(zed.Find(1), "Should find tag 1");

            for (var i = 0; i < 7; ++i)
            {
                Assert.AreEqual(7 - i, zed.UnreadCount);
                zed.ReadByte();
            }

            zed.ReadByte();
            Assert.AreEqual(0, zed.UnreadCount);
        }
    }

    [TestFixture]
    public class FastZipHandling : ZipBase
    {
        private const string ZipTempDir = "SharpZipLibTest";

        private void EnsureTestDirectoryIsEmpty(string baseDir)
        {
            var name = Path.Combine(baseDir, ZipTempDir);

            if (Directory.Exists(name))
            {
                Directory.Delete(name, true);
            }

            Directory.CreateDirectory(name);
        }

        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void Basics()
        {
            const string tempName1 = "a.dat";

            var target = new MemoryStream();

            var tempFilePath = GetTempFilePath();
            Assert.IsNotNull(tempFilePath, "No permission to execute this test?");

            var addFile = Path.Combine(tempFilePath, tempName1);
            MakeTempFile(addFile, 1);

            try
            {
                var fastZip = new FastZip();
                fastZip.CreateZip(target, tempFilePath, false, @"a\.dat", null);

                var archive = new MemoryStream(target.ToArray());
                using (var zf = new ZipFile(archive))
                {
                    Assert.AreEqual(1, zf.Count);
                    var entry = zf[0];
                    Assert.AreEqual(tempName1, entry.Name);
                    Assert.AreEqual(1, entry.Size);
                    Assert.IsTrue(zf.TestArchive(true));

                    zf.Close();
                }
            }
            finally
            {
                File.Delete(tempName1);
            }
        }

        [Test]
        [Category("Zip")]
        public void Encryption()
        {
            const string tempName1 = "a.dat";

            var target = new MemoryStream();

            var tempFilePath = GetTempFilePath();
            Assert.IsNotNull(tempFilePath, "No permission to execute this test?");

            var addFile = Path.Combine(tempFilePath, tempName1);
            MakeTempFile(addFile, 1);

            try
            {
                var fastZip = new FastZip();
                fastZip.Password = "Ahoy";

                fastZip.CreateZip(target, tempFilePath, false, @"a\.dat", null);

                var archive = new MemoryStream(target.ToArray());
                using (var zf = new ZipFile(archive))
                {
                    zf.Password = "Ahoy";
                    Assert.AreEqual(1, zf.Count);
                    var entry = zf[0];
                    Assert.AreEqual(tempName1, entry.Name);
                    Assert.AreEqual(1, entry.Size);
                    Assert.IsTrue(zf.TestArchive(true));
                    Assert.IsTrue(entry.IsCrypted);
                }
            }
            finally
            {
                File.Delete(tempName1);
            }
        }

        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void ExtractEmptyDirectories()
        {
            var tempFilePath = GetTempFilePath();
            Assert.IsNotNull(tempFilePath, "No permission to execute this test?");

            var name = Path.Combine(tempFilePath, "x.zip");

            EnsureTestDirectoryIsEmpty(tempFilePath);

            var targetDir = Path.Combine(tempFilePath, ZipTempDir + @"\floyd");
            using (var fs = File.Create(name))
            {
                using (var zOut = new ZipOutputStream(fs))
                {
                    zOut.PutNextEntry(new ZipEntry("floyd/"));
                }
            }

            var fastZip = new FastZip();
            fastZip.CreateEmptyDirectories = true;
            fastZip.ExtractZip(name, targetDir, "zz");

            File.Delete(name);
            Assert.IsTrue(Directory.Exists(targetDir), "Empty directory should be created");
        }

        [Test]
        [Category("Zip")]
        public void NonAsciiPasswords()
        {
            const string tempName1 = "a.dat";

            var target = new MemoryStream();

            var tempFilePath = GetTempFilePath();
            Assert.IsNotNull(tempFilePath, "No permission to execute this test?");

            var addFile = Path.Combine(tempFilePath, tempName1);
            MakeTempFile(addFile, 1);

            var password = "abc\u0066\u0393";
            try
            {
                var fastZip = new FastZip();
                fastZip.Password = password;

                fastZip.CreateZip(target, tempFilePath, false, @"a\.dat", null);

                var archive = new MemoryStream(target.ToArray());
                using (var zf = new ZipFile(archive))
                {
                    zf.Password = password;
                    Assert.AreEqual(1, zf.Count);
                    var entry = zf[0];
                    Assert.AreEqual(tempName1, entry.Name);
                    Assert.AreEqual(1, entry.Size);
                    Assert.IsTrue(zf.TestArchive(true));
                    Assert.IsTrue(entry.IsCrypted);
                }
            }
            finally
            {
                File.Delete(tempName1);
            }
        }
    }

    [TestFixture]
    public class ZipFileHandling : ZipBase
    {
        private void Compare(byte[] a, byte[] b)
        {
            Assert.AreEqual(a.Length, b.Length);
            for (var i = 0; i < a.Length; ++i)
            {
                Assert.AreEqual(a[i], b[i]);
            }
        }

        private void TryDeleting(byte[] master, int totalEntries, int additions, params string[] toDelete)
        {
            var ms = new MemoryStream();
            ms.Write(master, 0, master.Length);

            using (var f = new ZipFile(ms))
            {
                f.IsStreamOwner = false;
                Assert.AreEqual(totalEntries, f.Count);
                Assert.IsTrue(f.TestArchive(true));
                f.BeginUpdate(new MemoryArchiveStorage());

                for (var i = 0; i < additions; ++i)
                {
                    f.Add(new StringMemoryDataSource("Another great file"),
                          string.Format("Add{0}.dat", i + 1));
                }

                foreach (var name in toDelete)
                {
                    f.Delete(name);
                }
                f.CommitUpdate();

                // write stream to file to assist debugging.
                // WriteToFile(@"c:\aha.zip", ms.ToArray());

                var newTotal = totalEntries + additions - toDelete.Length;
                Assert.AreEqual(newTotal, f.Count,
                                string.Format("Expected {0} entries after update found {1}", newTotal, f.Count));
                Assert.IsTrue(f.TestArchive(true), "Archive test should pass");
            }
        }

        private void TryDeleting(byte[] master, int totalEntries, int additions, params int[] toDelete)
        {
            var ms = new MemoryStream();
            ms.Write(master, 0, master.Length);

            using (var f = new ZipFile(ms))
            {
                f.IsStreamOwner = false;
                Assert.AreEqual(totalEntries, f.Count);
                Assert.IsTrue(f.TestArchive(true));
                f.BeginUpdate(new MemoryArchiveStorage());

                for (var i = 0; i < additions; ++i)
                {
                    f.Add(new StringMemoryDataSource("Another great file"),
                          string.Format("Add{0}.dat", i + 1));
                }

                foreach (var i in toDelete)
                {
                    f.Delete(f[i]);
                }
                f.CommitUpdate();

                /* write stream to file to assist debugging.
								byte[] data = ms.ToArray();
								using ( FileStream fs = File.Open(@"c:\aha.zip", FileMode.Create, FileAccess.ReadWrite, FileShare.Read) ) {
									fs.Write(data, 0, data.Length);
								}
				*/
                var newTotal = totalEntries + additions - toDelete.Length;
                Assert.AreEqual(newTotal, f.Count,
                                string.Format("Expected {0} entries after update found {1}", newTotal, f.Count));
                Assert.IsTrue(f.TestArchive(true), "Archive test should pass");
            }
        }

        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void AddAndDeleteEntries()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            var addFile = Path.Combine(tempFile, "a.dat");
            MakeTempFile(addFile, 1);

            var addFile2 = Path.Combine(tempFile, "b.dat");
            MakeTempFile(addFile2, 259);

            tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");

            using (var f = ZipFile.Create(tempFile))
            {
                f.BeginUpdate();
                f.Add(addFile);
                f.Add(addFile2);
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true));
            }

            using (var f = new ZipFile(tempFile))
            {
                Assert.AreEqual(2, f.Count);
                Assert.IsTrue(f.TestArchive(true));
                f.BeginUpdate();
                f.Delete(f[0]);
                f.CommitUpdate();
                Assert.AreEqual(1, f.Count);
                Assert.IsTrue(f.TestArchive(true));
            }

            File.Delete(addFile);
            File.Delete(addFile2);
            File.Delete(tempFile);
        }

        [Test]
        [Category("Zip")]
        public void AddAndDeleteEntriesMemory()
        {
            var memStream = new MemoryStream();

            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;

                f.BeginUpdate(new MemoryArchiveStorage());
                f.Add(new StringMemoryDataSource("Hello world"), @"z:\a\a.dat");
                f.Add(new StringMemoryDataSource("Another"), @"\b\b.dat");
                f.Add(new StringMemoryDataSource("Mr C"), @"c\c.dat");
                f.Add(new StringMemoryDataSource("Mrs D was a star"), @"d\d.dat");
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true));
            }

            var master = memStream.ToArray();

            TryDeleting(master, 4, 1, @"z:\a\a.dat");
            TryDeleting(master, 4, 1, @"\a\a.dat");
            TryDeleting(master, 4, 1, @"a/a.dat");

            TryDeleting(master, 4, 0, 0);
            TryDeleting(master, 4, 0, 1);
            TryDeleting(master, 4, 0, 2);
            TryDeleting(master, 4, 0, 3);
            TryDeleting(master, 4, 0, 0, 1);
            TryDeleting(master, 4, 0, 0, 2);
            TryDeleting(master, 4, 0, 0, 3);
            TryDeleting(master, 4, 0, 1, 2);
            TryDeleting(master, 4, 0, 1, 3);
            TryDeleting(master, 4, 0, 2);

            TryDeleting(master, 4, 1, 0);
            TryDeleting(master, 4, 1, 1);
            TryDeleting(master, 4, 3, 2);
            TryDeleting(master, 4, 4, 3);
            TryDeleting(master, 4, 10, 0, 1);
            TryDeleting(master, 4, 10, 0, 2);
            TryDeleting(master, 4, 10, 0, 3);
            TryDeleting(master, 4, 20, 1, 2);
            TryDeleting(master, 4, 30, 1, 3);
            TryDeleting(master, 4, 40, 2);
        }

        [Test]
        [Category("Zip")]
        public void AddEncryptedEntriesToExistingArchive()
        {
            const string TestValue = "0001000";
            var memStream = new MemoryStream();
            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;
                f.UseZip64 = UseZip64.Off;

                var m = new StringMemoryDataSource(TestValue);
                f.BeginUpdate(new MemoryArchiveStorage());
                f.Add(m, "a.dat");
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true), "Archive test should pass");
            }

            using (var g = new ZipFile(memStream))
            {
                var ze = g[0];

                Assert.IsFalse(ze.IsCrypted, "Entry should NOT be encrypted");
                using (var r = new StreamReader(g.GetInputStream(0)))
                {
                    var data = r.ReadToEnd();
                    Assert.AreEqual(TestValue, data);
                }

                var n = new StringMemoryDataSource(TestValue);

                g.Password = "Axolotyl";
                g.UseZip64 = UseZip64.Off;
                g.IsStreamOwner = false;
                g.BeginUpdate();
                g.Add(n, "a1.dat");
                g.CommitUpdate();
                Assert.IsTrue(g.TestArchive(true), "Archive test should pass");
                ze = g[1];
                Assert.IsTrue(ze.IsCrypted, "New entry should be encrypted");
                using (var r = new StreamReader(g.GetInputStream(0)))
                {
                    var data = r.ReadToEnd();
                    Assert.AreEqual(TestValue, data);
                }
            }
        }

        [Test]
        [Category("Zip")]
        public void AddToEmptyArchive()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");
            if (tempFile != null)
            {
                var addFile = Path.Combine(tempFile, "a.dat");
                MakeTempFile(addFile, 1);

                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");

                using (var f = ZipFile.Create(tempFile))
                {
                    f.BeginUpdate();
                    f.Add(addFile);
                    f.CommitUpdate();
                    Assert.AreEqual(1, f.Count);
                    Assert.IsTrue(f.TestArchive(true));
                }

                using (var f = new ZipFile(tempFile))
                {
                    Assert.AreEqual(1, f.Count);
                    f.BeginUpdate();
                    f.Delete(f[0]);
                    f.CommitUpdate();
                    Assert.AreEqual(0, f.Count);
                    Assert.IsTrue(f.TestArchive(true));
                    f.Close();
                }

                File.Delete(addFile);
                File.Delete(tempFile);
            }
        }

        [Test]
        [Category("Zip")]
        public void ArchiveTesting()
        {
            byte[] originalData = null;
            var compressedData = MakeInMemoryZip(ref originalData, CompressionMethod.Deflated,
                                                 6, 1024, null, true);

            var ms = new MemoryStream(compressedData);
            ms.Seek(0, SeekOrigin.Begin);

            using (var testFile = new ZipFile(ms))
            {
                Assert.IsTrue(testFile.TestArchive(true), "Unexpected error in archive detected");

                var corrupted = new byte[compressedData.Length];
                Array.Copy(compressedData, corrupted, compressedData.Length);

                corrupted[123] = (byte) (~corrupted[123] & 0xff);
                ms = new MemoryStream(corrupted);
            }

            using (var testFile = new ZipFile(ms))
            {
                Assert.IsFalse(testFile.TestArchive(true), "Error in archive not detected");
            }
        }

        [Test]
        [Category("Zip")]
        public void BasicEncryption()
        {
            const string TestValue = "0001000";
            var memStream = new MemoryStream();
            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;
                f.Password = "Hello";

                var m = new StringMemoryDataSource(TestValue);
                f.BeginUpdate(new MemoryArchiveStorage());
                f.Add(m, "a.dat");
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true), "Archive test should pass");
            }

            using (var g = new ZipFile(memStream))
            {
                g.Password = "Hello";
                var ze = g[0];

                Assert.IsTrue(ze.IsCrypted, "Entry should be encrypted");
                using (var r = new StreamReader(g.GetInputStream(0)))
                {
                    var data = r.ReadToEnd();
                    Assert.AreEqual(TestValue, data);
                }
            }
        }

        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void BasicEncryptionToDisk()
        {
            const string TestValue = "0001000";
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");

            using (var f = ZipFile.Create(tempFile))
            {
                f.Password = "Hello";

                var m = new StringMemoryDataSource(TestValue);
                f.BeginUpdate();
                f.Add(m, "a.dat");
                f.CommitUpdate();
            }

            using (var f = new ZipFile(tempFile))
            {
                f.Password = "Hello";
                Assert.IsTrue(f.TestArchive(true), "Archive test should pass");
            }

            using (var g = new ZipFile(tempFile))
            {
                g.Password = "Hello";
                var ze = g[0];

                Assert.IsTrue(ze.IsCrypted, "Entry should be encrypted");
                using (var r = new StreamReader(g.GetInputStream(0)))
                {
                    var data = r.ReadToEnd();
                    Assert.AreEqual(TestValue, data);
                }
            }

            File.Delete(tempFile);
        }

        [Test]
        [Category("Zip")]
        public void CreateEmptyArchive()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");

                using (var f = ZipFile.Create(tempFile))
                {
                    f.BeginUpdate();
                    f.CommitUpdate();
                    Assert.IsTrue(f.TestArchive(true));
                    f.Close();
                }

                using (var f = new ZipFile(tempFile))
                {
                    Assert.AreEqual(0, f.Count);
                }
            }
        }

        [Test]
        [Category("Zip")]
        public void Crypto_AddEncryptedEntryToExistingArchiveDirect()
        {
            var ms = new MemoryStream();

            byte[] rawData;

            using (var testFile = new ZipFile(ms))
            {
                testFile.IsStreamOwner = false;
                testFile.BeginUpdate();
                testFile.Add(new StringMemoryDataSource("Aha"), "No1", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("And so it goes"), "No2", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("No3"), "No3", CompressionMethod.Stored);
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
                rawData = ms.ToArray();
            }

            using (var testFile = new ZipFile(ms))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                testFile.IsStreamOwner = false;

                testFile.BeginUpdate();
                testFile.Password = "pwd";
                testFile.Add(new StringMemoryDataSource("Zapata!"), "encrypttest.xml");
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));

                var entryIndex = testFile.FindEntry("encrypttest.xml", true);
                Assert.IsNotNull(entryIndex >= 0);
                Assert.IsTrue(testFile[entryIndex].IsCrypted);
            }
        }

        [Test]
        [Category("Zip")]
        public void Crypto_AddEncryptedEntryToExistingArchiveSafe()
        {
            var ms = new MemoryStream();

            byte[] rawData;

            using (var testFile = new ZipFile(ms))
            {
                testFile.IsStreamOwner = false;
                testFile.BeginUpdate();
                testFile.Add(new StringMemoryDataSource("Aha"), "No1", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("And so it goes"), "No2", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("No3"), "No3", CompressionMethod.Stored);
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
                rawData = ms.ToArray();
            }

            ms = new MemoryStream(rawData);

            using (var testFile = new ZipFile(ms))
            {
                Assert.IsTrue(testFile.TestArchive(true));

                testFile.BeginUpdate(new MemoryArchiveStorage(FileUpdateMode.Safe));
                testFile.Password = "pwd";
                testFile.Add(new StringMemoryDataSource("Zapata!"), "encrypttest.xml");
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));

                var entryIndex = testFile.FindEntry("encrypttest.xml", true);
                Assert.IsNotNull(entryIndex >= 0);
                Assert.IsTrue(testFile[entryIndex].IsCrypted);
            }
        }

        [Test]
        [Category("Zip")]
        public void EmbeddedArchive()
        {
            var memStream = new MemoryStream();
            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;

                var m = new StringMemoryDataSource("0000000");
                f.BeginUpdate(new MemoryArchiveStorage());
                f.Add(m, "a.dat");
                f.Add(m, "b.dat");
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true));
            }

            var rawArchive = memStream.ToArray();
            var pseudoSfx = new byte[1049 + rawArchive.Length];
            Array.Copy(rawArchive, 0, pseudoSfx, 1049, rawArchive.Length);

            memStream = new MemoryStream(pseudoSfx);
            using (var f = new ZipFile(memStream))
            {
                for (var index = 0; index < f.Count; ++index)
                {
                    var entryStream = f.GetInputStream(index);
                    var data = new MemoryStream();
                    StreamUtils.Copy(entryStream, data, new byte[128]);

                    var bytes = data.ToArray();
                    string contents = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    Assert.AreEqual("0000000", contents);
                }
            }
        }

        /// <summary>
        /// Check that ZipFile doesnt find entries when there is more than 64K of data at the end.
        /// </summary>
        /// <remarks>
        /// This may well be flawed but is the current behaviour.
        /// </remarks>
        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void FindEntriesInArchiveExtraData()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                var longComment = new String('A', 65535);
                var tempStream = File.Create(tempFile);
                MakeZipFile(tempStream, false, "", 1, 1, longComment);

                tempStream.WriteByte(85);
                tempStream.Close();

                var fails = false;
                try
                {
                    using (var zipFile = new ZipFile(tempFile))
                    {
                        foreach (ZipEntry e in zipFile)
                        {
                            var instream = zipFile.GetInputStream(e);
                            CheckKnownEntry(instream, 1);
                        }
                        zipFile.Close();
                    }
                }
                catch
                {
                    fails = true;
                }

                File.Delete(tempFile);
                Assert.IsTrue(fails, "Currently zip file wont be found");
            }
        }

        /// <summary>
        /// Check that ZipFile finds entries when its got a long comment
        /// </summary>
        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void FindEntriesInArchiveWithLongComment()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                var longComment = new String('A', 65535);
                MakeZipFile(tempFile, "", 1, 1, longComment);
                using (var zipFile = new ZipFile(tempFile))
                {
                    foreach (ZipEntry e in zipFile)
                    {
                        var instream = zipFile.GetInputStream(e);
                        CheckKnownEntry(instream, 1);
                    }
                    zipFile.Close();
                }
                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Test ZipFile Find method operation
        /// </summary>
        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void FindEntry()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                MakeZipFile(tempFile, new[]{"Farriera", "Champagne", "Urban myth"}, 10, "Aha");

                using (var zipFile = new ZipFile(tempFile))
                {
                    Assert.AreEqual(3, zipFile.Count, "Expected 1 entry");

                    var testIndex = zipFile.FindEntry("Farriera", false);
                    Assert.AreEqual(0, testIndex, "Case sensitive find failure");
                    Assert.IsTrue(string.Compare(zipFile[testIndex].Name, "Farriera", StringComparison.InvariantCulture) == 0);

                    testIndex = zipFile.FindEntry("Farriera", true);
                    Assert.AreEqual(0, testIndex, "Case insensitive find failure");
                    Assert.IsTrue(string.Compare(zipFile[testIndex].Name, "Farriera", StringComparison.InvariantCultureIgnoreCase) == 0);

                    testIndex = zipFile.FindEntry("urban mYTH", false);
                    Assert.AreEqual(-1, testIndex, "Case sensitive find failure");

                    testIndex = zipFile.FindEntry("urban mYTH", true);
                    Assert.AreEqual(2, testIndex, "Case insensitive find failure");
                    Assert.IsTrue(string.Compare(zipFile[testIndex].Name, "urban mYTH", StringComparison.InvariantCultureIgnoreCase) == 0);

                    testIndex = zipFile.FindEntry("Champane.", false);
                    Assert.AreEqual(-1, testIndex, "Case sensitive find failure");

                    testIndex = zipFile.FindEntry("Champane.", true);
                    Assert.AreEqual(-1, testIndex, "Case insensitive find failure");

                    zipFile.Close();
                }
                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Check that ZipFile class handles no entries in zip file
        /// </summary>
        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void HandlesNoEntries()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                MakeZipFile(tempFile, "", 0, 1, "Aha");

                using (var zipFile = new ZipFile(tempFile))
                {
                    Assert.AreEqual(0, zipFile.Count);
                    zipFile.Close();
                }

                File.Delete(tempFile);
            }
        }

        [Test]
        [Category("Zip")]
        public void NameFactory()
        {
            var memStream = new MemoryStream();
            var fixedTime = new DateTime(1981, 4, 3);
            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;
                ((ZipEntryFactory) f.EntryFactory).IsUnicodeText = true;
                ((ZipEntryFactory) f.EntryFactory).Setting = ZipEntryFactory.TimeSetting.Fixed;
                ((ZipEntryFactory) f.EntryFactory).FixedDateTime = fixedTime;
                ((ZipEntryFactory) f.EntryFactory).SetAttributes = 1;
                f.BeginUpdate(new MemoryArchiveStorage());

                var names = new[]{
                                     "\u030A\u03B0", // Greek
                                     "\u0680\u0685", // Arabic
                                 };

                foreach (var name in names)
                {
                    f.Add(new StringMemoryDataSource("Hello world"), name,
                          CompressionMethod.Deflated, true);
                }
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true));

                foreach (var name in names)
                {
                    var index = f.FindEntry(name, true);

                    Assert.IsTrue(index >= 0);
                    var found = f[index];
                    Assert.AreEqual(name, found.Name);
                    Assert.IsTrue(found.IsUnicodeText);
                    Assert.AreEqual(fixedTime, found.DateTime);
                    Assert.IsTrue(found.IsDOSEntry);
                }
            }
        }

        [Test]
        [Category("Zip")]
        public void NullStreamDetected()
        {
            ZipFile bad = null;
            FileStream nullStream = null;

            var nullStreamDetected = false;

            try
            {
                bad = new ZipFile(nullStream);
            }
            catch
            {
                nullStreamDetected = true;
            }

            Assert.IsTrue(nullStreamDetected, "Null stream should be detected in ZipFile constructor");
            Assert.IsNull(bad, "ZipFile instance should not be created");
        }

        /// <summary>
        /// Simple round trip test for ZipFile class
        /// </summary>
        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void RoundTrip()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            if (tempFile != null)
            {
                tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
                MakeZipFile(tempFile, "", 10, 1024, "");

                using (var zipFile = new ZipFile(tempFile))
                {
                    foreach (ZipEntry e in zipFile)
                    {
                        var instream = zipFile.GetInputStream(e);
                        CheckKnownEntry(instream, 1024);
                    }
                    zipFile.Close();
                }

                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Simple round trip test for ZipFile class
        /// </summary>
        [Test]
        [Category("Zip")]
        public void RoundTripInMemory()
        {
            var storage = new MemoryStream();
            MakeZipFile(storage, false, "", 10, 1024, "");

            using (var zipFile = new ZipFile(storage))
            {
                foreach (ZipEntry e in zipFile)
                {
                    var instream = zipFile.GetInputStream(e);
                    CheckKnownEntry(instream, 1024);
                }
                zipFile.Close();
            }
        }

        [Test]
        [Category("Zip")]
        public void UnicodeNames()
        {
            var memStream = new MemoryStream();
            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;

                f.BeginUpdate(new MemoryArchiveStorage());

                var names = new[]{
                                     "\u030A\u03B0", // Greek
                                     "\u0680\u0685", // Arabic
                                 };

                foreach (var name in names)
                {
                    f.Add(new StringMemoryDataSource("Hello world"), name,
                          CompressionMethod.Deflated, true);
                }
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true));

                foreach (var name in names)
                {
                    var index = f.FindEntry(name, true);

                    Assert.IsTrue(index >= 0);
                    var found = f[index];
                    Assert.AreEqual(name, found.Name);
                }
            }
        }

        [Test]
        [Category("Zip")]
        public void UpdateCommentOnlyInMemory()
        {
            var ms = new MemoryStream();

            using (var testFile = new ZipFile(ms))
            {
                testFile.IsStreamOwner = false;
                testFile.BeginUpdate();
                testFile.Add(new StringMemoryDataSource("Aha"), "No1", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("And so it goes"), "No2", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("No3"), "No3", CompressionMethod.Stored);
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
            }

            using (var testFile = new ZipFile(ms))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                Assert.AreEqual("", testFile.ZipFileComment);
                testFile.IsStreamOwner = false;

                testFile.BeginUpdate();
                testFile.SetComment("Here is my comment");
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
            }

            using (var testFile = new ZipFile(ms))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                Assert.AreEqual("Here is my comment", testFile.ZipFileComment);
            }
        }

        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void UpdateCommentOnlyOnDisk()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            tempFile = Path.Combine(tempFile, "SharpZipTest.Zip");
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            using (var testFile = ZipFile.Create(tempFile))
            {
                testFile.BeginUpdate();
                testFile.Add(new StringMemoryDataSource("Aha"), "No1", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("And so it goes"), "No2", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("No3"), "No3", CompressionMethod.Stored);
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
            }

            using (var testFile = new ZipFile(tempFile))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                Assert.AreEqual("", testFile.ZipFileComment);

                testFile.BeginUpdate(new DiskArchiveStorage(testFile, FileUpdateMode.Direct));
                testFile.SetComment("Here is my comment");
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
            }

            using (var testFile = new ZipFile(tempFile))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                Assert.AreEqual("Here is my comment", testFile.ZipFileComment);
            }
            File.Delete(tempFile);

            // Variant using indirect updating.
            using (var testFile = ZipFile.Create(tempFile))
            {
                testFile.BeginUpdate();
                testFile.Add(new StringMemoryDataSource("Aha"), "No1", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("And so it goes"), "No2", CompressionMethod.Stored);
                testFile.Add(new StringMemoryDataSource("No3"), "No3", CompressionMethod.Stored);
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
            }

            using (var testFile = new ZipFile(tempFile))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                Assert.AreEqual("", testFile.ZipFileComment);

                testFile.BeginUpdate();
                testFile.SetComment("Here is my comment");
                testFile.CommitUpdate();

                Assert.IsTrue(testFile.TestArchive(true));
            }

            using (var testFile = new ZipFile(tempFile))
            {
                Assert.IsTrue(testFile.TestArchive(true));
                Assert.AreEqual("Here is my comment", testFile.ZipFileComment);
            }
            File.Delete(tempFile);
        }

        /// <summary>
        /// Check that adding too many entries is detected and handled
        /// </summary>
        [Test]
        [Category("Zip")]
        [Category("CreatesTempFile")]
        public void Zip64Entries()
        {
            var tempFile = GetTempFilePath();
            Assert.IsNotNull(tempFile, "No permission to execute this test?");

            const int target = 65537;

            using (var zipFile = ZipFile.Create(Path.GetTempFileName()))
            {
                zipFile.BeginUpdate();

                for (var i = 0; i < target; ++i)
                {
                    var ze = new ZipEntry(i.ToString());
                    ze.CompressedSize = 0;
                    ze.Size = 0;
                    zipFile.Add(ze);
                }
                zipFile.CommitUpdate();

                Assert.IsTrue(zipFile.TestArchive(true));
                Assert.AreEqual(target, zipFile.Count, "Incorrect number of entries stored");
            }
        }

        [Test]
        [Category("Zip")]
        [Explicit]
        public void Zip64Offset()
        {
            // TODO: Test to check that a zip64 offset value is loaded correctly.
            // Changes in ZipEntry to CentralHeaderRequiresZip64 and LocalHeaderRequiresZip64
            // were not quite correct...
        }

        [Test]
        [Category("Zip")]
        public void Zip64Useage()
        {
            var memStream = new MemoryStream();
            using (var f = new ZipFile(memStream))
            {
                f.IsStreamOwner = false;
                f.UseZip64 = UseZip64.On;

                var m = new StringMemoryDataSource("0000000");
                f.BeginUpdate(new MemoryArchiveStorage());
                f.Add(m, "a.dat");
                f.Add(m, "b.dat");
                f.CommitUpdate();
                Assert.IsTrue(f.TestArchive(true));
            }

            var rawArchive = memStream.ToArray();

            var pseudoSfx = new byte[1049 + rawArchive.Length];
            Array.Copy(rawArchive, 0, pseudoSfx, 1049, rawArchive.Length);

            memStream = new MemoryStream(pseudoSfx);
            using (var f = new ZipFile(memStream))
            {
                for (var index = 0; index < f.Count; ++index)
                {
                    var entryStream = f.GetInputStream(index);
                    var data = new MemoryStream();
                    StreamUtils.Copy(entryStream, data, new byte[128]);

                    var bytes = data.ToArray();
                    string contents = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    Assert.AreEqual("0000000", contents);
                }
            }
        }
    }

    [TestFixture]
    public class ZipEntryFactoryHandling : ZipBase
    {
        // TODO: Complete testing for ZipEntryFactory

        [Test]
        [Category("Zip")]
        public void CreateInMemoryValues()
        {
            var tempFile = "bingo:";

            // Note the seconds returned will be even!
            var epochTime = new DateTime(1980, 1, 1);
            var createTime = new DateTime(2100, 2, 27, 11, 07, 56);
            var lastWriteTime = new DateTime(2050, 11, 3, 7, 23, 32);
            var lastAccessTime = new DateTime(2050, 11, 3, 0, 42, 12);

            var factory = new ZipEntryFactory();
            ZipEntry entry;
            int combinedAttributes;

            var startTime = DateTime.Now;

            factory.Setting = ZipEntryFactory.TimeSetting.CreateTime;
            factory.GetAttributes = ~((int) FileAttributes.ReadOnly);
            factory.SetAttributes = (int) FileAttributes.ReadOnly;
            combinedAttributes = (int) FileAttributes.ReadOnly;

            entry = factory.MakeFileEntry(tempFile, false);
            Assert.IsTrue(TestHelper.CompareDosDateTimes(startTime, entry.DateTime) <= 0, "Create time failure");
            Assert.AreEqual(entry.ExternalFileAttributes, combinedAttributes);
            Assert.AreEqual(-1, entry.Size);

            factory.FixedDateTime = startTime;
            factory.Setting = ZipEntryFactory.TimeSetting.Fixed;
            entry = factory.MakeFileEntry(tempFile, false);
            Assert.AreEqual(0, TestHelper.CompareDosDateTimes(startTime, entry.DateTime), "Access time failure");
            Assert.AreEqual(-1, entry.Size);

            factory.Setting = ZipEntryFactory.TimeSetting.LastWriteTime;
            entry = factory.MakeFileEntry(tempFile, false);
            Assert.IsTrue(TestHelper.CompareDosDateTimes(startTime, entry.DateTime) <= 0, "Write time failure");
            Assert.AreEqual(-1, entry.Size);
        }

        [Test]
        [Category("Zip")]
        public void Defaults()
        {
            var testStart = DateTime.Now;
            var f = new ZipEntryFactory();
            Assert.IsNotNull(f.NameTransform);
            Assert.AreEqual(-1, f.GetAttributes);
            Assert.AreEqual(0, f.SetAttributes);
            Assert.AreEqual(ZipEntryFactory.TimeSetting.LastWriteTime, f.Setting);

            Assert.LessOrEqual(testStart, f.FixedDateTime);
            Assert.GreaterOrEqual(DateTime.Now, f.FixedDateTime);

            f = new ZipEntryFactory(ZipEntryFactory.TimeSetting.LastAccessTimeUtc);
            Assert.IsNotNull(f.NameTransform);
            Assert.AreEqual(-1, f.GetAttributes);
            Assert.AreEqual(0, f.SetAttributes);
            Assert.AreEqual(ZipEntryFactory.TimeSetting.LastAccessTimeUtc, f.Setting);
            Assert.LessOrEqual(testStart, f.FixedDateTime);
            Assert.GreaterOrEqual(DateTime.Now, f.FixedDateTime);

            var fixedDate = new DateTime(1999, 1, 2);
            f = new ZipEntryFactory(fixedDate);
            Assert.IsNotNull(f.NameTransform);
            Assert.AreEqual(-1, f.GetAttributes);
            Assert.AreEqual(0, f.SetAttributes);
            Assert.AreEqual(ZipEntryFactory.TimeSetting.Fixed, f.Setting);
            Assert.AreEqual(fixedDate, f.FixedDateTime);
        }
    }
}