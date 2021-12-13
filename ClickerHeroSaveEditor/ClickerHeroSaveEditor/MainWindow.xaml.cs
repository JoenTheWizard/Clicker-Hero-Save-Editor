using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace ClickerHeroSaveEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<byte> sepBuff;
        public MainWindow()
        {
            InitializeComponent();
            sepBuff = new List<byte>();
        }
        private void decode_Click(object sender, RoutedEventArgs e)
        {
            try {
                byte[] inp = Convert.FromBase64String(inputTx.Text);
                inp = GetSkipped(inp, 0x17);
                string outputRes = "";
                Stream stream = Decompress(inp);
                using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    outputRes = streamReader.ReadToEnd();
                }
                outputTx.Text = outputRes;
            } catch (Exception ex) {
                outputTx.Text = ex.Message;
            }
        }
        public byte[] GetSkipped(byte[] ba, int index)
        {
            List<byte> vs = new List<byte>();
            int i = 0;
            foreach (byte b in ba)
            {
                if (i > index)
                    vs.Add(b);
                else
                    sepBuff.Add(b);
                i++;
            }
            byte[] newByteArr = new byte[vs.Count];
            for (int j = 0; j < newByteArr.Length; j++)
                newByteArr[j] = vs[j];

            return newByteArr;
        }
        public static Stream Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using (var compressedStream = new MemoryStream(data))
            using (var inputStream = new InflaterInputStream(compressedStream))
            {
                inputStream.CopyTo(outputStream);
                outputStream.Position = 0;
                return outputStream;
            }
        }
        private void encode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var compress = new Func<byte[], byte[]>(a =>
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        using (var compressor =
                               new Ionic.Zlib.ZlibStream(ms,
                                                          Ionic.Zlib.CompressionMode.Compress,
                                                          Ionic.Zlib.CompressionLevel.BestCompression))
                        {
                            compressor.Write(a, 0, a.Length);
                        }
                        return ms.ToArray();
                    }
                });
                byte[] Compressed = compress(Encoding.ASCII.GetBytes(outputTx.Text));
                byte[] constantData = new byte[sepBuff.Count];
                for (int i = 0; i < sepBuff.Count; i++)
                    constantData[i] = sepBuff[i];

                List<byte> byteRES = new List<byte>();
                foreach (byte b in constantData)
                    byteRES.Add(b);
                foreach (byte b in Compressed)
                    byteRES.Add(b);

                byte[] result = new byte[byteRES.Count];
                for (int i = 0; i < byteRES.Count; i++)
                    result[i] = byteRES[i];

                inputTx.Text = Convert.ToBase64String(result);
            } catch (Exception ex) {
                inputTx.Text = ex.Message;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }
    }
}
