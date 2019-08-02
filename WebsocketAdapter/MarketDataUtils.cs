using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;


namespace WebsocketAdapter
{
    public class MarketDataUtils
    {
        public static string TimeStampToString(DateTime timeStamp)
        {
            return timeStamp.ToString("dd-MM-yyyy HH:mm:ss.fff");
        }

        public static string StringListToString(List<string> value,string separator=",")
        {
            return string.Join(separator, value);
        }

        public static byte[] StringToByteArray(string based64EncodedString)
        {
            return Convert.FromBase64String(based64EncodedString);
        }

        public static async Task<string> UnpackByteToJsonString(byte[] buffer)
        {
            string jsonString;
            using (var compressedStream = new MemoryStream(buffer))
            {
                using (var zipStream = new GZipStream((Stream)compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        await zipStream.CopyToAsync((Stream)resultStream).ConfigureAwait(false);
                        jsonString = Encoding.UTF8.GetString(resultStream.ToArray());
                    }
                }
            }

            return jsonString;
        }
    }
}
