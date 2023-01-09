using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ma3012receive
{
    [Serializable]
    public abstract class SerializationBase : ICloneable
    {
        // Methods
        protected SerializationBase()
        {
            this.SetDefaults();
        }

        public object Clone()
        {
            MemoryStream stream = new MemoryStream();
            this.Serialize(stream);
            stream.Seek(0L, SeekOrigin.Begin);
            object obj2 = Deserialize(stream);
            stream.Close();
            return obj2;
        }

        public static object Deserialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        public static object Deserialize(string path)
        {
            FileStream stream = null;
            Exception exception = null;
            object obj2 = null;
            try
            {
                stream = File.OpenRead(path);
                obj2 = Deserialize(stream);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            if (exception != null)
            {
                throw exception;
            }
            return obj2;
        }

        public static object DeserializeFromByteArray(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0L, SeekOrigin.Begin);
            object obj2 = Deserialize(stream);
            stream.Close();
            return obj2;
        }

        public static object DeserializeFromString(string base64String)
        {
            return DeserializeFromByteArray(Convert.FromBase64String(base64String));
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext sc)
        {
            this.SetDefaults();
        }

        public void Serialize(Stream stream)
        {
            new BinaryFormatter().Serialize(stream, this);
        }

        public void Serialize(string path)
        {
            FileStream stream = null;
            Exception exception = null;
            try
            {
                stream = File.OpenWrite(path);
                this.Serialize(stream);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            if (exception != null)
            {
                throw exception;
            }
        }

        public byte[] SerializeToByteArray()
        {
            MemoryStream stream = new MemoryStream();
            this.Serialize(stream);
            stream.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return buffer;
        }

        public string SerializeToString()
        {
            byte[] inArray = this.SerializeToByteArray();
            return Convert.ToBase64String(inArray, 0, inArray.Length);
        }

        protected abstract void SetDefaults();
    }

}
