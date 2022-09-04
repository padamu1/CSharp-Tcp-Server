using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace CSharpTcpServer.Core.Util
{
    public class DataFormat
    {
        public byte eventCode;
        public Dictionary<byte, object>? data;
    }
    public class ByteUtillity
    {
        public static DataFormat ByteToObject(byte[] buffer)
        {
            try
            {
                string data = Encoding.UTF8.GetString(buffer);
                if (data == null)
                {
                    return null;
                }
                DataFormat? dataFormat = JsonConvert.DeserializeObject<DataFormat>(data);

                Console.WriteLine(data);
                Console.WriteLine(dataFormat?.eventCode);
                return dataFormat;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }

        public static byte[] ObjectToByte(object obj)
        {
            try
            {
                string data = JsonConvert.SerializeObject(obj);
                byte[] byteData = Encoding.UTF8.GetBytes(data);
                return byteData;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return null;
        }
    }
}
