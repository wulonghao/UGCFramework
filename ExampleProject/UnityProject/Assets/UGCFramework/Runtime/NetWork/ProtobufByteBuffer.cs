using System;
using System.Collections.Generic;

namespace UGCF.Network
{
    /// <summary>
    /// 字节缓冲处理类，本类仅处理大字节序
    /// 警告，本类非线程安全
    /// </summary>
    public class ProtobufByteBuffer
    {
        //字节缓存区
        private byte[] buf;
        //读取索引
        private int readIndex = 0;
        //写入索引
        private int writeIndex = 0;
        //读取索引标记
        private int markReadIndex = 0;
        //写入索引标记
        private int markWirteIndex = 0;
        //缓存区字节数组的长度
        private int capacity;

        //对象池
        private static List<ProtobufByteBuffer> pool = new List<ProtobufByteBuffer>();
        private static int poolMaxCount = 200;
        //此对象是否池化
        private bool isPool = false;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="capacity">初始容量</param>
        private ProtobufByteBuffer(int capacity)
        {
            buf = new byte[capacity];
            this.capacity = capacity;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="bytes">初始字节数组</param>
        private ProtobufByteBuffer(byte[] bytes)
        {
            buf = bytes;
            this.capacity = bytes.Length;
            this.readIndex = 0;
            this.writeIndex = bytes.Length + 1;
        }

        /// <summary>
        /// 构建一个capacity长度的字节缓存区ProtobufByteBuffer对象
        /// </summary>
        /// <param name="capacity">初始容量</param>
        /// <returns>ProtobufByteBuffer对象</returns>
        public static ProtobufByteBuffer Allocate(int capacity)
        {
            return new ProtobufByteBuffer(capacity);
        }

        /// <summary>
        /// 构建一个以bytes为字节缓存区的ProtobufByteBuffer对象，一般不推荐使用
        /// </summary>
        /// <param name="bytes">初始字节数组</param>
        /// <returns>ProtobufByteBuffer对象</returns>
        public static ProtobufByteBuffer Allocate(byte[] bytes)
        {
            return new ProtobufByteBuffer(bytes);
        }

        /// <summary>
        /// 获取一个池化的ProtobufByteBuffer对象，池化的对象必须在调用Dispose后才会推入池中，否则此方法等同于Allocate(int capacity)方法，此方法为线程安全的
        /// </summary>
        /// <param name="capacity">ProtobufByteBuffer对象的初始容量大小，如果缓存池中没有对象，则对象的容量大小为此值，否则为池中对象的实际容量值</param>
        /// <returns></returns>
        public static ProtobufByteBuffer GetFromPool(int capacity)
        {
            lock (pool)
            {
                ProtobufByteBuffer bbuf;
                if (pool.Count == 0)
                {
                    bbuf = Allocate(capacity);
                    bbuf.isPool = true;
                    return bbuf;
                }
                int lastIndex = pool.Count - 1;
                bbuf = pool[lastIndex];
                pool.RemoveAt(lastIndex);
                if (!bbuf.isPool)
                {
                    bbuf.isPool = true;
                }
                return bbuf;
            }
        }

        /// <summary>
        /// 根据length长度，确定大于此leng的最近的2次方数，如length=7，则返回值为8
        /// </summary>
        /// <param name="length">参考容量</param>
        /// <returns>比参考容量大的最接近的2次方数</returns>
        private int FixLength(int length)
        {
            int n = 2;
            int b = 2;
            while (b < length)
            {
                b = 2 << n;
                n++;
            }
            return b;
        }

        /// <summary>
        /// 翻转字节数组，如果本地字节序列为低字节序列，则进行翻转以转换为高字节序列
        /// </summary>
        /// <param name="bytes">待转为高字节序的字节数组</param>
        /// <returns>高字节序列的字节数组</returns>
        private byte[] flip(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// 确定内部字节缓存数组的大小
        /// </summary>
        /// <param name="currLen">当前容量</param>
        /// <param name="futureLen">将来的容量</param>
        /// <returns>将来的容量</returns>
        private int FixSizeAndReset(int currLen, int futureLen)
        {
            if (futureLen > currLen)
            {
                //以原大小的2次方数的两倍确定内部字节缓存区大小
                int size = FixLength(currLen) * 2;
                if (futureLen > size)
                {
                    //以将来的大小的2次方的两倍确定内部字节缓存区大小
                    size = FixLength(futureLen) * 2;
                }
                byte[] newbuf = new byte[size];
                Array.Copy(buf, 0, newbuf, 0, currLen);
                buf = newbuf;
                capacity = newbuf.Length;
            }
            return futureLen;
        }

        /// <summary>
        /// 将bytes字节数组从startIndex开始的length字节写入到此缓存区
        /// </summary>
        /// <param name="bytes">待写入的字节数据</param>
        /// <param name="startIndex">写入的开始位置</param>
        /// <param name="length">写入的长度</param>
        public void WriteBytes(byte[] bytes, int startIndex, int length)
        {
            int offset = length - startIndex;
            if (offset <= 0) return;
            int total = offset + writeIndex;
            int len = buf.Length;
            FixSizeAndReset(len, total);
            for (int i = writeIndex, j = startIndex; i < total; i++, j++)
            {
                buf[i] = bytes[j];
            }
            writeIndex = total;
        }

        /// <summary>
        /// 将字节数组中从0到length的元素写入缓存区
        /// </summary>
        /// <param name="bytes">待写入的字节数据</param>
        /// <param name="length">写入的长度</param>
        public void WriteBytes(byte[] bytes, int length)
        {
            WriteBytes(bytes, 0, length);
        }

        /// <summary>
        /// 将字节数组全部写入缓存区
        /// </summary>
        /// <param name="bytes">待写入的字节数据</param>
        public void WriteBytes(byte[] bytes)
        {
            WriteBytes(bytes, bytes.Length);
        }

        /// <summary>
        /// 将一个ProtobufByteBuffer的有效字节区写入此缓存区中
        /// </summary>
        /// <param name="buffer">待写入的字节缓存区</param>
        public void Write(ProtobufByteBuffer buffer)
        {
            if (buffer == null) return;
            if (buffer.ReadableBytes() <= 0) return;
            WriteBytes(buffer.ToArray());
        }

        /// <summary>
        /// 写入一个int16数据
        /// </summary>
        /// <param name="value">short数据</param>
        public void WriteShort(short value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个ushort数据
        /// </summary>
        /// <param name="value">ushort数据</param>
        public void WriteUshort(ushort value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int32数据
        /// </summary>
        /// <param name="value">int数据</param>
        public void WriteInt(int value)
        {
            //byte[] array = new byte[4];
            //for (int i = 3; i >= 0; i--)
            //{
            //    array[i] = (byte)(value & 0xff);
            //    value = value >> 8;
            //}
            //Array.Reverse(array);
            //Write(array);
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint32数据
        /// </summary>
        /// <param name="value">uint数据</param>
        public void WriteUint(uint value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个int64数据
        /// </summary>
        /// <param name="value">long数据</param>
        public void WriteLong(long value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个uint64数据
        /// </summary>
        /// <param name="value">ulong数据</param>
        public void WriteUlong(ulong value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个float数据
        /// </summary>
        /// <param name="value">float数据</param>
        public void WriteFloat(float value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个byte数据
        /// </summary>
        /// <param name="value">byte数据</param>
        public void WriteByte(byte value)
        {
            int afterLen = writeIndex + 1;
            int len = buf.Length;
            FixSizeAndReset(len, afterLen);
            buf[writeIndex] = value;
            writeIndex = afterLen;
        }

        /// <summary>
        /// 写入一个byte数据
        /// </summary>
        /// <param name="value">byte数据</param>
        public void WriteByte(int value)
        {
            byte b = (byte)value;
            WriteByte(b);
        }

        /// <summary>
        /// 写入一个double类型数据
        /// </summary>
        /// <param name="value">double数据</param>
        public void WriteDouble(double value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个字符
        /// </summary>
        /// <param name="value"></param>
        public void WriteChar(char value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 写入一个布尔型数据
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(bool value)
        {
            WriteBytes(flip(BitConverter.GetBytes(value)));
        }

        /// <summary>
        /// 读取一个字节
        /// </summary>
        /// <returns>字节数据</returns>
        public byte ReadByte()
        {
            byte b = buf[readIndex];
            readIndex++;
            return b;
        }

        /// <summary>
        /// 读取一个字节并转为int类型的数据
        /// </summary>
        /// <returns>int数据</returns>
        public int ReadByteToInt()
        {
            byte b = ReadByte();
            return (int)b;
        }

        /// <summary>
        /// 获取从index索引处开始len长度的字节
        /// </summary>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private byte[] Get(int index, int len)
        {
            byte[] bytes = new byte[len];
            Array.Copy(buf, index, bytes, 0, len);
            return flip(bytes);
        }

        /// <summary>
        /// 从读取索引位置开始读取len长度的字节数组
        /// </summary>
        /// <param name="len">待读取的字节长度</param>
        /// <returns>字节数组</returns>
        private byte[] Read(int len)
        {
            byte[] bytes = Get(readIndex, len);
            readIndex += len;
            return bytes;
        }

        /// <summary>
        /// 读取一个uint16数据
        /// </summary>
        /// <returns>ushort数据</returns>
        public ushort ReadUshort()
        {
            return BitConverter.ToUInt16(Read(2), 0);
        }

        /// <summary>
        /// 读取一个int16数据
        /// </summary>
        /// <returns>short数据</returns>
        public short ReadShort()
        {
            return BitConverter.ToInt16(Read(2), 0);
        }

        /// <summary>
        /// 读取一个uint32数据
        /// </summary>
        /// <returns>uint数据</returns>
        public uint ReadUint()
        {
            return BitConverter.ToUInt32(Read(4), 0);
        }

        /// <summary>
        /// 读取一个int32数据
        /// </summary>
        /// <returns>int数据</returns>
        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(4), 0);
        }

        /// <summary>
        /// 读取一个uint64数据
        /// </summary>
        /// <returns>ulong数据</returns>
        public ulong ReadUlong()
        {
            return BitConverter.ToUInt64(Read(8), 0);
        }

        /// <summary>
        /// 读取一个long数据
        /// </summary>
        /// <returns>long数据</returns>
        public long ReadLong()
        {
            return BitConverter.ToInt64(Read(8), 0);
        }

        /// <summary>
        /// 读取一个float数据
        /// </summary>
        /// <returns>float数据</returns>
        public float ReadFloat()
        {
            return BitConverter.ToSingle(Read(4), 0);
        }

        /// <summary>
        /// 读取一个double数据
        /// </summary>
        /// <returns>double数据</returns>
        public double ReadDouble()
        {
            return BitConverter.ToDouble(Read(8), 0);
        }

        /// <summary>
        /// 读取一个字符
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            return BitConverter.ToChar(Read(2), 0);
        }

        /// <summary>
        /// 读取布尔型数据
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            return BitConverter.ToBoolean(Read(1), 0);
        }

        /// <summary>
        /// 从读取索引位置开始读取len长度的字节到disbytes目标字节数组中
        /// </summary>
        /// <param name="disbytes">读取的字节将存入此字节数组</param>
        /// <param name="disstart">目标字节数组的写入索引</param>
        /// <param name="len">读取的长度</param>
        public void ReadBytes(byte[] disbytes, int disstart, int len)
        {
            int size = disstart + len;
            for (int i = disstart; i < size; i++)
            {
                disbytes[i] = this.ReadByte();
            }
        }

        /// <summary>
        /// 获取一个字节
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte GetByte(int index)
        {
            return buf[index];
        }

        /// <summary>
        /// 获取全部字节
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return buf;
        }

        /// <summary>
        /// 获取一个双精度浮点数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public double GetDouble(int index)
        {
            return BitConverter.ToDouble(Get(0, 8), 0);
        }

        /// <summary>
        /// 获取一个浮点数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public float GetFloat(int index)
        {
            return BitConverter.ToSingle(Get(0, 4), 0);
        }

        /// <summary>
        /// 获取一个长整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public long GetLong(int index)
        {
            return BitConverter.ToInt64(Get(0, 8), 0);
        }

        /// <summary>
        /// 获取一个整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public int GetInt(int index)
        {
            return BitConverter.ToInt32(Get(0, 4), 0);
        }

        /// <summary>
        /// 获取一个短整形数据，不改变数据内容
        /// </summary>
        /// <param name="index">字节索引</param>
        /// <returns></returns>
        public int GetShort(int index)
        {
            return BitConverter.ToInt16(Get(0, 2), 0);
        }


        /// <summary>
        /// 清除已读字节并重建缓存区
        /// </summary>
        public void DiscardReadBytes()
        {
            if (readIndex <= 0) return;
            int len = buf.Length - readIndex;
            byte[] newbuf = new byte[len];
            Array.Copy(buf, readIndex, newbuf, 0, len);
            buf = newbuf;
            writeIndex -= readIndex;
            markReadIndex -= readIndex;
            if (markReadIndex < 0)
            {
                markReadIndex = readIndex;
            }
            markWirteIndex -= readIndex;
            if (markWirteIndex < 0 || markWirteIndex < readIndex || markWirteIndex < markReadIndex)
            {
                markWirteIndex = writeIndex;
            }
            readIndex = 0;
        }

        /// <summary>
        /// 清空此对象，但保留字节缓存数组（空数组）
        /// </summary>
        public void Clear()
        {
            buf = new byte[buf.Length];
            readIndex = 0;
            writeIndex = 0;
            markReadIndex = 0;
            markWirteIndex = 0;
            capacity = buf.Length;
        }

        /// <summary>
        /// 释放对象，清除字节缓存数组，如果此对象为可池化，那么调用此方法将会把此对象推入到池中等待下次调用
        /// </summary>
        public void Dispose()
        {
            readIndex = 0;
            writeIndex = 0;
            markReadIndex = 0;
            markWirteIndex = 0;
            if (isPool)
            {
                lock (pool)
                {
                    if (pool.Count < poolMaxCount)
                    {
                        pool.Add(this);
                    }
                }
            }
            else
            {
                capacity = 0;
                buf = null;
            }
        }

        /// <summary>
        /// 设置/获取读指针位置
        /// </summary>
        public int ReaderIndex
        {
            get
            {
                return readIndex;
            }
            set
            {
                if (value < 0) return;
                readIndex = value;
            }
        }

        /// <summary>
        /// 设置/获取写指针位置
        /// </summary>
        public int WriterIndex
        {
            get
            {
                return writeIndex;
            }
            set
            {
                if (value < 0) return;
                writeIndex = value;
            }
        }

        /// <summary>
        /// 标记读取的索引位置
        /// </summary>
        public void MarkReaderIndex()
        {
            markReadIndex = readIndex;
        }

        /// <summary>
        /// 标记写入的索引位置
        /// </summary>
        public void MarkWriterIndex()
        {
            markWirteIndex = writeIndex;
        }

        /// <summary>
        /// 将读取的索引位置重置为标记的读取索引位置
        /// </summary>
        public void ResetReaderIndex()
        {
            readIndex = markReadIndex;
        }

        /// <summary>
        /// 将写入的索引位置重置为标记的写入索引位置
        /// </summary>
        public void ResetWriterIndex()
        {
            writeIndex = markWirteIndex;
        }

        /// <summary>
        /// 可读的有效字节数
        /// </summary>
        /// <returns>可读的字节数</returns>
        public int ReadableBytes()
        {
            return writeIndex - readIndex;
        }

        /// <summary>
        /// 获取可读的字节数组
        /// </summary>
        /// <returns>字节数据</returns>
        public byte[] ToArray()
        {
            byte[] bytes = new byte[writeIndex];
            Array.Copy(buf, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        /// 获取缓存区容量大小
        /// </summary>
        /// <returns>缓存区容量</returns>
        public int GetCapacity()
        {
            return this.capacity;
        }

        /// <summary>
        /// 简单的数据类型
        /// </summary>
        public enum LengthType
        {
            //byte类型
            BYTE,
            //short类型
            SHORT,
            //int类型
            INT
        }

        /// <summary>
        /// 写入一个数据
        /// </summary>
        /// <param name="value">待写入的数据</param>
        /// <param name="type">待写入的数据类型</param>
        public void WriteValue(int value, LengthType type)
        {
            switch (type)
            {
                case LengthType.BYTE:
                    this.WriteByte(value);
                    break;
                case LengthType.SHORT:
                    this.WriteShort((short)value);
                    break;
                default:
                    this.WriteInt(value);
                    break;
            }
        }

        /// <summary>
        /// 读取一个值，值类型根据type决定，int或short或byte
        /// </summary>
        /// <param name="type">值类型</param>
        /// <returns>int数据</returns>
        public int ReadValue(LengthType type)
        {
            switch (type)
            {
                case LengthType.BYTE:
                    return ReadByteToInt();
                case LengthType.SHORT:
                    return (int)ReadShort();
                default:
                    return ReadInt();
            }
        }

        /// <summary>
        /// 写入一个字符串
        /// </summary>
        /// <param name="content">待写入的字符串</param>
        /// <param name="lenType">写入的字符串长度类型</param>
        public void WriteUTF8String(string content, LengthType lenType)
        {
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(content);
            int max;
            if (lenType == LengthType.BYTE)
            {
                WriteByte(bytes.Length);
                max = byte.MaxValue;
            }
            else if (lenType == LengthType.SHORT)
            {
                WriteShort((short)bytes.Length);
                max = short.MaxValue;
            }
            else
            {
                WriteInt(bytes.Length);
                max = int.MaxValue;
            }
            if (bytes.Length > max)
            {
                WriteBytes(bytes, 0, max);
            }
            else
            {
                WriteBytes(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// 读取一个字符串
        /// </summary>
        /// <param name="len">需读取的字符串长度</param>
        /// <returns>字符串</returns>
        public string ReadUTF8String(int len)
        {
            byte[] bytes = new byte[len];
            this.ReadBytes(bytes, 0, len);
            return System.Text.UTF8Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 读取一个字符串
        /// </summary>
        /// <param name="lenType">字符串长度类型</param>
        /// <returns>字符串</returns>
        public string ReadUTF8String(LengthType lenType)
        {
            int len = ReadValue(lenType);
            return ReadUTF8String(len);
        }

        /// <summary>
        /// 复制一个对象，具有与原对象相同的数据，不改变原对象的数据
        /// </summary>
        /// <returns></returns>
        public ProtobufByteBuffer Copy()
        {
            return Copy(0);
        }

        public ProtobufByteBuffer Copy(int startIndex)
        {
            if (buf == null)
            {
                return new ProtobufByteBuffer(16);
            }
            byte[] target = new byte[buf.Length - startIndex];
            Array.Copy(buf, startIndex, target, 0, target.Length);
            ProtobufByteBuffer buffer = new ProtobufByteBuffer(target.Length);
            buffer.WriteBytes(target);
            return buffer;
        }
    }
}