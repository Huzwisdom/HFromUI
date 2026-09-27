using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HFromUI.HConvert
{
    public class HBytes : IEnumerable
    {
        /// <summary>bytes 字段。</summary>
        private List<byte> bytes = new List<byte>();
        public byte this[int index]
        {
            get
            {
                return GetByte(index);
            }
            set
            {
                SetByte(index,value);
            }
        }
        public byte[] this[int start, int length]
        {
            get
            {
                return GetBytes(start, length);
            }
            set
            {
                for (int i = 0; i < length; i++)
                {
                    SetByte(start+i,value[i]);
                }
            }
        }
        public HBytes()
        {

        }
        public HBytes(byte[] contents)
        {
            Load(contents);
        }
        public HBytes(string content, char split, int Base = 16)
        {
            Clear();
            Add(content, split, Base);
        }
        public HBytes(string content, Encoding encoding)
        {
            Clear();
            Add(content, encoding);
        }
        /// <summary>获取 enumerator。</summary>
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < bytes.Count; i++)
            {
                yield return bytes[i];
            }
        }
        public List<byte> Bytes
        {
           set { bytes = value; }
           get {
                return bytes;
            }
        }
        /// <summary>数量。</summary>
        public int Count {
            get { return bytes.Count; }
        }
        /// <summary>加载。</summary>
        public void Load(byte[] contents)
        {
            Clear();
            bytes = contents.ToList();
        }
        /// <summary>添加。</summary>
        public void Add(byte content)
        {
            bytes.Add(content);
        }
        public void Add(HBytes content)
        {
            bytes.AddRange(content.bytes);
        }
        /// <summary>添加。</summary>
        public void Add(string content, int Base = 10)
        {
            bytes.Add(ToByte(content, Base));
        }
        /// <summary>添加。</summary>
        public void Add(string content, Encoding encoding)
        {
           Add(encoding.GetBytes(content));
        }
        /// <summary>添加。</summary>
        public void Add(string content, char split = ' ', int Base = 16)
        {
            string[] contents = content.Split(new char[] { split }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in contents)
            {
                Add(content, Base);
            }
        }
        /// <summary>添加。</summary>
        public void Add(byte[] contents)
        {
            bytes.AddRange(contents);
        }
        /// <summary>添加。</summary>
        public void Add(List<byte> contents)
        {
            bytes.AddRange(contents.ToArray());
        }
        /// <summary>添加。</summary>
        public void Add(short value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add(ushort value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add( int value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add( uint value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add( float value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add(Int64 value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add(UInt64 value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>添加。</summary>
        public void Add(double value, int mode=0)
        {
            Add(BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>AddStartEnd 方法。</summary>
        public void AddStartEnd(byte contentstart = 2, byte contentend = 3)
        {
            bytes.Insert(0, contentstart);
            bytes.Add(contentend);
        }
        /// <summary>RemoveStartEnd 方法。</summary>
        public bool RemoveStartEnd(byte contentstart = 2, byte contentend = 3)
        {
            if (bytes == null)
            {
                return false;
            }
            if (bytes.Count < 2)
            {
                return false;
            }
            if (bytes[0] != contentstart || bytes[bytes.Count - 1] != contentend)
            {
                return false;
            }
            Remove(0);
            Remove(bytes.Count - 1);
            return true;
        }
        /// <summary>获取 bool。</summary>
        public bool GetBool(int index)
        {
            return Convert.ToBoolean(bytes[index]);
        }
        /// <summary>获取 bool。</summary>
        public bool GetBool(int index,int BoolIndex)
        {
            return HBytes.FindBool(BoolIndex, bytes[index]); 
        }
        /// <summary>获取 bools。</summary>
        public bool[] GetBools(int start, int length=1)
        {
            return HBytes.ToBools(GetBytes(start,length));
        }
        /// <summary>获取 byte。</summary>
        public byte GetByte(int index)
        {
            return bytes[index];
        }
        /// <summary>获取 bytes。</summary>
        public byte[] GetBytes(int start, int length, bool IsList = false)
        {
            if (IsList)
            {
                List<byte> BytesBuffer = new List<byte>();
                for (int i = start; i < start + length; i++)
                {
                    BytesBuffer.Add(bytes[i]);
                }
                return BytesBuffer.ToArray();
            }
            else
            {
                byte[] bytesBuffer = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytesBuffer[i] = bytes[start + length];
                }
                return bytesBuffer;
            }

        }
        /// <summary>获取 short。</summary>
        public short GetShort(int startIndex, int mode=0)
        {
            return BytesToInt16(BytesHighLowConvertNew(GetBytes(startIndex, 2), mode));
        }
        /// <summary>获取 ushort。</summary>
        public ushort GetUshort(int startIndex, int mode=0)
        {
            return BytesToUint16(BytesHighLowConvertNew(GetBytes(startIndex, 2), mode));
        }
        /// <summary>获取 int。</summary>
        public int GetInt(int startIndex, int mode=0)
        {
            return BytesToInt(BytesHighLowConvertNew(GetBytes(startIndex, 4), mode));
        }
        /// <summary>获取 float。</summary>
        public float GetFloat(int startIndex, int mode=0)
        {
            return BitConverter.ToSingle(BytesHighLowConvertNew(GetBytes(startIndex, 4), mode), 0);
        }
        /// <summary>获取 uint。</summary>
        public uint GetUint(int startIndex, int mode=0)
        {
            return BytesToUint(BytesHighLowConvertNew(GetBytes(startIndex, 4), mode));
        }
        /// <summary>获取 int64。</summary>
        public Int64 GetInt64(int startIndex, int mode=0)
        {
            return BytesToInt64(BytesHighLowConvertNew(GetBytes(startIndex, 8), mode));
        }
        /// <summary>获取 double。</summary>
        public double GetDouble(int startIndex, int mode=0)
        {
            return BytesToDouble(BytesHighLowConvertNew(GetBytes(startIndex, 8), mode));
        }
        /// <summary>获取 uint64。</summary>
        public UInt64 GetUint64(int startIndex, int mode=0)
        {
            return BytesToUint64(BytesHighLowConvertNew(GetBytes(startIndex, 8), mode));
        }
        /// <summary>获取 string。</summary>
        public string GetString(int startIndex,int length, Encoding encoding, bool isRemove=true)
        {
            return ToString(GetBytes(startIndex, length), encoding, isRemove);
        }
        /// <summary>设置 bool。</summary>
        public void SetBool(int index,bool value)
        {
            bytes[index]= Convert.ToByte(value);
        }
        /// <summary>设置 byte。</summary>
        public void SetByte(int index, byte value)
        {
             bytes[index]= value;
        }
        /// <summary>设置 bytes。</summary>
        public void SetBytes(int index, byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                bytes[index + i] = value[i];
            }
        }
        /// <summary>设置 short。</summary>
        public void SetShort(int startIndex, short value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 ushort。</summary>
        public void SetUshort(int startIndex, ushort value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 int。</summary>
        public void SetInt(int startIndex, int value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 uint。</summary>
        public void SetUint(int startIndex, uint value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 float。</summary>
        public void SetFloat(int startIndex, float value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 int64。</summary>
        public void SetInt64(int startIndex, Int64 value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 uint64。</summary>
        public void SetUint64(int startIndex, UInt64 value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 double。</summary>
        public void SetDouble(int startIndex, double value, int mode=0)
        {
            Update(startIndex, BytesHighLowConvertNew(ToBytes(value), mode));
        }
        /// <summary>设置 string。</summary>
        public void SetString(int startIndex,string value, Encoding encoding, bool isRemove = true)
        {
            if (isRemove)
            {
                Update(startIndex, encoding.GetBytes(value.Replace("\r", "").Replace("\n", "").Replace("\0", "")));
            }
            else
            {
                Update(startIndex, encoding.GetBytes(value));
            }
        }
        /// <summary>插入。</summary>
        public void Insert(int index, byte content)
        {
            bytes.Insert(index, content);
        }
        /// <summary>插入。</summary>
        public void Insert(int index, byte[] contents)
        {
            bytes.InsertRange(index, contents);
        }
        /// <summary>插入。</summary>
        public void Insert(int index,List<byte> contents)
        {
            bytes.InsertRange(index, contents.ToArray());
        }
        /// <summary>清空。</summary>
        public void Clear()
        {
            bytes.Clear();
        }
        /// <summary>移除。</summary>
        public void Remove(int index)
        {
            bytes.RemoveAt(index);
        }
        /// <summary>移除。</summary>
        public void Remove(byte content)
        {
            bytes.Remove(content);
        }
        /// <summary>移除。</summary>
        public void Remove(int index, int length)
        {
            bytes.RemoveRange(index, length);
        }
        /// <summary>更新。</summary>
        public void Update(int index, byte content)
        {
            bytes[index] = content;
        }
        /// <summary>更新。</summary>
        public void Update(int index, byte[] contents)
        {
            if (contents == null)
            {
                return;
            }
            for (int i = 0; i < contents.Length; i++)
            {
                bytes[index + i] = contents[i];
            }
        }
        public string ToString(Encoding encoding,bool isRemove)
        {
            if (isRemove)
            {
                return encoding.GetString(bytes.ToArray()).Replace("\r","").Replace("\n", "").Replace("\0", "");
            }
            else
            {
                return encoding.GetString(bytes.ToArray());
            }
        }
        /// <summary>转换为 Strings。</summary>
        public string[] ToStrings(int Base = 16)
        {
            List<string> ListStrings = new List<string>();
            for (int i = 0; i < bytes.Count; i++)
            {
                ListStrings.Add(Convert.ToString(bytes[i], Base));
            }
            return ListStrings.ToArray();
        }
        public string ToString(char split = ' ')
        {
            string[] Strings = ToStrings();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Strings)
            {
                stringBuilder.Append(item); stringBuilder.Append(split);
            }
            return stringBuilder.ToString().TrimEnd(split);
        }
        /// <summary>ListFindBool 方法。</summary>
        public bool ListFindBool(int listIndex, int byteIndex)
        {
            return FindBool(byteIndex, bytes[listIndex]);
        }
        /// <summary>FindIndex 方法。</summary>
        public int FindIndex(byte content)
        {
            return bytes.FindIndex(x => x == content);
        }
        /// <summary>FindLastIndex 方法。</summary>
        public int FindLastIndex(byte content)
        {
            return bytes.FindLastIndex(x => x == content);
        }
        /// <summary>克隆。</summary>
        public HBytes Clone()
        {
            return new HBytes(this.Bytes.ToArray());
        }
        public static string ToString(byte[] contents,Encoding encoding, bool isRemove)
        {
            if (isRemove)
            {
                return encoding.GetString(contents.ToArray()).Replace("\r", "").Replace("\n", "").Replace("\0", "");
            }
            else
            {
                return encoding.GetString(contents.ToArray());
            }
        }
        /// <summary>BytesHighLowConvertNew 方法。</summary>
        public static byte[] BytesHighLowConvertNew(byte[] contents, int mode=0)
        {
            if (contents == null)
            {
                return contents;
            }
            byte[] bytesBuffer = new byte[contents.Length];
            switch (contents.Length)
            {
                case 2:

                    switch (mode)
                    {
                        case 1:
                            bytesBuffer[0] = contents[1];
                            bytesBuffer[1] = contents[0];
                            break;
                        case 3:
                            bytesBuffer[0] = contents[1];
                            bytesBuffer[1] = contents[0];
                            break;
                        default:
                            bytesBuffer = contents;
                            break;
                    }
                    break;
                case 4:
                    switch (mode)
                    {
                        case 0:
                            bytesBuffer = contents;
                            break;
                        case 1:
                            bytesBuffer[0] = contents[1];
                            bytesBuffer[1] = contents[0];
                            bytesBuffer[2] = contents[3];
                            bytesBuffer[3] = contents[2];
                            break;
                        case 2:
                            bytesBuffer[0] = contents[2];
                            bytesBuffer[1] = contents[3];
                            bytesBuffer[2] = contents[0];
                            bytesBuffer[3] = contents[1];
                            break;
                        case 3:
                            bytesBuffer[0] = contents[3];
                            bytesBuffer[1] = contents[2];
                            bytesBuffer[2] = contents[1];
                            bytesBuffer[3] = contents[0];
                            break;
                        default:
                            bytesBuffer = contents;
                            break;
                    }
                    break;
                default:
                    bytesBuffer=contents;
                    break;
            }
            return bytesBuffer;
        }
        /// <summary>BytesHighLowConvert 方法。</summary>
        public static byte[] BytesHighLowConvert(byte[] contents, int mode=0)
        {
            if (contents == null)
            {
                return contents;
            }
            byte byteBuffer = 0;
            switch (contents.Length)
            {
                case 2:

                    switch (mode)
                    {
                        case 1:
                            byteBuffer = contents[1];
                            contents[1] = contents[0];
                            contents[0] = byteBuffer;
                            break;
                        case 3:
                            byteBuffer = contents[1];
                            contents[1] = contents[0];
                            contents[0] = byteBuffer;
                            break;
                        default:
                            break;
                    }
                    break;
                case 4:
                    switch (mode)
                    {
                        case 0:
                            break;
                        case 1:
                            byteBuffer = contents[1];
                            contents[1] = contents[0];
                            contents[0] = byteBuffer;
                            byteBuffer = contents[3];
                            contents[3] = contents[2];
                            contents[2] = byteBuffer;
                            break;
                        case 2:
                            byteBuffer = contents[1];
                            contents[1] = contents[3];
                            contents[3] = byteBuffer;
                            byteBuffer = contents[2];
                            contents[2] = contents[0];
                            contents[0] = byteBuffer;
                            break;
                        case 3:
                            byteBuffer = contents[1];
                            contents[1] = contents[2];
                            contents[2] = byteBuffer;
                            byteBuffer = contents[0];
                            contents[0] = contents[3];
                            contents[3] = byteBuffer;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return contents;
        }
        /// <summary>RandomBytes 方法。</summary>
        public static byte[] RandomBytes(int count)
        {
            if (count <= 0)
            {
                return null;
            }
            byte[] byteRandomBuffer = new byte[count];
            System.Random random = new System.Random();
            random.NextBytes(byteRandomBuffer);
            return byteRandomBuffer;
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(bool[] contents)
        { 
            List<byte> bytes = new List<byte>();
            int number = contents.Length / 8 + (contents.Length % 8>0?1:0);
            for (int i = 0; i < number; i++)
            {
                if (i == number - 1)
                {
                    List<bool> bools = new List<bool>();
                    for (int j = i * 8; j < contents.Length - i * 8; j++)
                    {
                        bools.Add(contents[j]);
                    }
                    bytes.Add(GetByte(bools.ToArray()));
                }
                else
                {
                    List<bool> bools = new List<bool>();
                    for (int j = i * 8; j < i * 8 + 8; j++)
                    {
                        bools.Add(contents[j]);
                    }
                    bytes.Add(GetByte(bools.ToArray()));
                }
            }
            return bytes.ToArray();
        }
        /// <summary>获取 byte。</summary>
        public static byte GetByte(bool[] contents)
        {
            byte byteBuffer = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i])
                {
                    byteBuffer += (byte)Math.Pow(2,i);
                }
            }
            return byteBuffer;
        }
        /// <summary>获取 ushort。</summary>
        public static ushort GetUshort(bool[] contents)
        {
            ushort byteBuffer = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i])
                {
                    byteBuffer += (ushort)Math.Pow(2, i);
                }
            }
            return byteBuffer;
        }
        /// <summary>获取 uint。</summary>
        public static uint GetUint(bool[] contents)
        {
            uint byteBuffer = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i])
                {
                    byteBuffer += (uint)Math.Pow(2, i);
                }
            }
            return byteBuffer;
        }
        /// <summary>获取 int。</summary>
        public static int GetInt(bool[] contents)
        {
            return (int)GetUint(contents);
        }
        /// <summary>获取 float。</summary>
        public static float GetFloat(bool[] contents)
        {
            byte[] byteDoubleBuffer = new byte[8];
            List<bool> ListBoolBuffer = new List<bool>();
            for (int i = 0; i <= 32; i++)
            {
                switch (i)
                {
                    case 8:
                    case 16:
                    case 24:
                    case 32:
                        byteDoubleBuffer[i / 8 - 1] = GetByte(ListBoolBuffer.ToArray());
                        ListBoolBuffer.Clear();
                        if (i != 32)
                        {
                            ListBoolBuffer.Add(contents[i]);
                        }
                        break;
                    default:
                        ListBoolBuffer.Add(contents[i]);
                        break;
                }

            }
            return BitConverter.ToSingle(byteDoubleBuffer, 0);
        }
        /// <summary>获取 short。</summary>
        public static short GetShort(bool[] contents)
        {
            return (short)GetUshort(contents);
        }
        /// <summary>获取 uint64。</summary>
        public static UInt64 GetUint64(bool[] contents)
        {
            UInt64 byteBuffer = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i])
                {
                    byteBuffer += (UInt64)Math.Pow(2, i);
                }
            }
            return byteBuffer;
        }
        /// <summary>获取 int64。</summary>
        public static Int64 GetInt64(bool[] contents)
        {
            return (Int64)GetUint64(contents);
        }
        /// <summary>获取 double。</summary>
        public static double GetDouble(bool[] contents)
        {
            byte[] byteDoubleBuffer = new byte[8];
            List<bool> ListBoolBuffer = new List<bool>();
            for (int i = 0; i <= 64; i++)
            {
                switch (i)
                {
                    case 8:
                    case 16:
                    case 24:
                    case 32:
                    case 40:
                    case 48:
                    case 56:
                    case 64:
                        byteDoubleBuffer[i/8-1] = GetByte(ListBoolBuffer.ToArray());
                        ListBoolBuffer.Clear();
                        if (i!=64)
                        {
                            ListBoolBuffer.Add(contents[i]);
                        }
                        break;
                    default:
                        ListBoolBuffer.Add(contents[i]);
                        break;
                }
              
            }
       
            return BitConverter.ToDouble(byteDoubleBuffer,0);
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, byte content)
        {
            return ((content >> Index) & 1) == 1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, int content)
        {
            return ((content >> Index) & 1)==1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, uint content)
        {
            return ((content >> Index) & 1) == 1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, short content)
        {
            return ((content >> Index) & 1) == 1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, ushort content)
        {
            return ((content >> Index) & 1) == 1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, Int64 content)
        {
            return ((content >> Index) & 1) == 1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, UInt64 content)
        {
            return ((content >> Index) & 1) == 1;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, double content)
        {
            byte[] byteDouBuffer= BitConverter.GetBytes(content);
            if (Index>=0&& Index<8)
            {
                return FindBool(Index, byteDouBuffer[0]);
            }
            else if (Index >= 8 && Index < 16)
            {
                return FindBool(Index-8, byteDouBuffer[1]);
            }
            else if (Index >= 16 && Index < 24)
            {
                return FindBool(Index-16, byteDouBuffer[2]);
            }
            else if (Index >= 24 && Index < 32)
            {
                return FindBool(Index-24, byteDouBuffer[3]);
            }
            else if (Index >= 32 && Index < 40)
            {
                return FindBool(Index-32, byteDouBuffer[4]);
            }
            else if (Index >= 40 && Index < 48)
            {
                return FindBool(Index-40, byteDouBuffer[5]);
            }
            else if (Index >= 48 && Index < 56)
            {
                return FindBool(Index-48, byteDouBuffer[6]);
            }
            else if (Index >= 56 && Index < 64)
            {
                return FindBool(Index-56, byteDouBuffer[7]);
            }
            return false;
        }
        /// <summary>FindBool 方法。</summary>
        public static bool FindBool(int Index, float content)
        {
            byte[] byteDouBuffer = BitConverter.GetBytes(content);
            if (Index >= 0 && Index < 8)
            {
                return FindBool(Index, byteDouBuffer[0]);
            }
            else if (Index >= 8 && Index < 16)
            {
                return FindBool(Index - 8, byteDouBuffer[1]);
            }
            else if (Index >= 16 && Index < 24)
            {
                return FindBool(Index - 16, byteDouBuffer[2]);
            }
            else if (Index >= 24 && Index < 32)
            {
                return FindBool(Index - 24, byteDouBuffer[3]);
            }
            return false;
        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(byte[] content, bool IsList = false)
        {
            List<bool> boolListBuffer = new List<bool>();
            foreach (byte b in content) {
                boolListBuffer.AddRange(ToBools(b,IsList));
            }
            return boolListBuffer.ToArray();
        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(byte content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 8; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[8];
                for (int i = 0; i < 8; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(short content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 16; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(ushort content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 16; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[16];
                for (int i = 0; i < 16; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(uint content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 32; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[32];
                for (int i = 0; i < 32; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(int content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 32; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[32];
                for (int i = 0; i < 32; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(float content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 32; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[32];
                for (int i = 0; i < 32; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }
        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(Int64 content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 64; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[64];
                for (int i = 0; i < 64; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(double content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 64; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[64];
                for (int i = 0; i < 64; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }
        }
        /// <summary>转换为 Bools。</summary>
        public static bool[] ToBools(UInt64 content, bool IsList = false)
        {
            if (IsList)
            {
                List<bool> boolListBuffer = new List<bool>();
                for (int i = 0; i < 64; i++)
                {
                    boolListBuffer.Add(FindBool(i, content));
                }
                return boolListBuffer.ToArray();
            }
            else
            {
                bool[] boolArraysBuffer = new bool[64];
                for (int i = 0; i < 64; i++)
                {
                    boolArraysBuffer[i] = FindBool(i, content);
                }
                return boolArraysBuffer;
            }

        }
        /// <summary>ByteToInt 方法。</summary>
        public static int ByteToInt(byte content)
        {
            return Convert.ToInt32(content);
        }
        /// <summary>BytesToDouble 方法。</summary>
        public static double BytesToDouble(byte[] contents)
        {
            return BitConverter.ToDouble(contents,0);
        }
        /// <summary>BytesToDouble 方法。</summary>
        public static double BytesToDouble(byte[] contents, int offset = 0)
        {
            return BitConverter.ToDouble(contents, offset);
        }
        /// <summary>BytesToInt64 方法。</summary>
        public static Int64 BytesToInt64(byte[] contents, int offset = 0, bool isReverse = false)
        {
            return
                   (((Int64)contents[(offset + 0) % 8] << (isReverse ? 48 : 56))
                   | ((Int64)contents[(offset + 1) % 8] << (isReverse ? 56 : 48))
                   | ((Int64)contents[(offset + 2) % 8] << (isReverse ? 32 : 40))
                   | (Int64)contents[(offset + 3) % 8] << (isReverse ? 40 : 32))

                   | (((Int64)contents[(offset + 4) % 8] << (isReverse ? 16 : 24))
                   | ((Int64)contents[(offset + 5) % 8] << (isReverse ? 24 : 16))
                   | ((Int64)contents[(offset + 6) % 8] << (isReverse ? 0 : 8))
                   | (Int64)contents[(offset + 7) % 8] << (isReverse ? 8 : 0));
        }
        /// <summary>BytesToInt64 方法。</summary>
        public static Int64 BytesToInt64(byte[] contents)
        {
            return
                   (((Int64)contents[0] << 56)
                   | ((Int64)contents[1] << 48)
                   | ((Int64)contents[2] << 40)
                   | (Int64)contents[3] << 32)

                   | (((Int64)contents[4] << 24)
                   | ((Int64)contents[5] << 16)
                   | ((Int64)contents[6] << 8)
                   | (Int64)contents[7]);
        }
        /// <summary>BytesToUint64 方法。</summary>
        public static UInt64 BytesToUint64(byte[] contents, int offset = 0, bool isReverse = false)
        {
            return
                   (((UInt64)contents[(offset + 0) % 8] << (isReverse ? 48 : 56))
                   | ((UInt64)contents[(offset + 1) % 8] << (isReverse ? 56 : 48))
                   | ((UInt64)contents[(offset + 2) % 8] << (isReverse ? 32 : 40))
                   | (UInt64)contents[(offset + 3) % 8] << (isReverse ? 40 : 32))

                   | (((UInt64)contents[(offset + 4) % 8] << (isReverse ? 16 : 24))
                   | ((UInt64)contents[(offset + 5) % 8] << (isReverse ? 24 : 16))
                   | ((UInt64)contents[(offset + 6) % 8] << (isReverse ? 0 : 8))
                   | (UInt64)contents[(offset + 7) % 8] << (isReverse ? 8 : 0));
        }
        /// <summary>BytesToUint64 方法。</summary>
        public static UInt64 BytesToUint64(byte[] contents)
        {
            return
                   (((UInt64)contents[0] << 56)
                   | ((UInt64)contents[1] << 48)
                   | ((UInt64)contents[2] << 40)
                   | (UInt64)contents[3] << 32)

                   | (((UInt64)contents[4] << 24)
                   | ((UInt64)contents[5] << 16)
                   | ((UInt64)contents[6] << 8)
                   | (UInt64)contents[7]);
        }
        /// <summary>BytesToInt 方法。</summary>
        public static int BytesToInt(byte[] contents, int offset = 0, bool isReverse = false)
        {
            return (((int)contents[(offset + 0) % 4] << (isReverse ? 16 : 24))
                   | ((int)contents[(offset + 1) % 4] << (isReverse ? 24 : 16))
                   | ((int)contents[(offset + 2) % 4] << (isReverse ? 0 : 8))
                   | (int)contents[(offset + 3) % 4] << (isReverse ? 8 : 0));
        }
        /// <summary>BytesToInt 方法。</summary>
        public static int BytesToInt(byte[] contents)
        {
            return (((int)contents[0] << 24)
                   | ((int)contents[1] << 16)
                   | ((int)contents[2] << 8)
                   | (int)contents[3]);
        }
        /// <summary>BytesToFloat 方法。</summary>
        public static float BytesToFloat(byte[] contents, int offset = 0)
        {
            return BitConverter.ToSingle(contents, offset);
        }
        /// <summary>BytesToFloat 方法。</summary>
        public static float BytesToFloat(byte[] contents)
        {
            return BitConverter.ToSingle(contents, 0);
        }
        /// <summary>BytesToUint 方法。</summary>
        public static uint BytesToUint(byte[] contents, int offset = 0, bool isReverse = false)
        {
            return (((uint)contents[(offset + 0) % 4] << (isReverse ? 16 : 24))
                   | ((uint)contents[(offset + 1) % 4] << (isReverse ? 24 : 16))
                   | ((uint)contents[(offset + 2) % 4] << (isReverse ? 0 : 8))
                   | (uint)contents[(offset + 3) % 4] << (isReverse ? 8 : 0));
        }
        /// <summary>BytesToUint 方法。</summary>
        public static uint BytesToUint(byte[] contents)
        {
            return (((uint)contents[0] << 24)
                   | ((uint)contents[1] << 16)
                   | ((uint)contents[2] << 8)
                   | (uint)contents[3]);
        }
        /// <summary>BytesToInt16 方法。</summary>
        public static short BytesToInt16(byte[] contents, int offset = 0)
        {
            return (Int16)((contents[(offset) % 2] << 8) | contents[(offset + 1) % 2]);
        }
        /// <summary>BytesToInt16 方法。</summary>
        public static short BytesToInt16(byte[] contents)
        {
            return (Int16)((contents[0] << 8) | contents[1]);
        }
        /// <summary>BytesToUint16 方法。</summary>
        public static ushort BytesToUint16(byte[] contents, int offset = 0)
        {
            return (UInt16)((contents[(offset) % 2] << 8) | contents[(offset + 1) % 2]);
        }
        /// <summary>BytesToUint16 方法。</summary>
        public static ushort BytesToUint16(byte[] contents)
        {
            return (UInt16)((contents[0] << 8) | contents[1]);
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(uint value, int offset = 0, bool isReverse = false)
        {
            if (isReverse)
            {
                return new byte[] {
                 (byte)(value >> ((offset)%4+2)*8),
                 (byte)(value >> ((offset)%4+3)*8),
                 (byte)(value >> ((offset)%4+0)*8),
                 (byte)(value >> ((offset)%4+1)*8),
                 };
            }
            else
            {
                return new byte[] {
                 (byte)(value >> ((offset)%4+3)*8),
                 (byte)(value >> ((offset)%4+2)*8),
                 (byte)(value >> ((offset)%4+1)*8),
                 (byte)(value >> ((offset)%4+0)*8),
                 };
            }
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(uint value)
        {
            return new byte[] {
                 (byte)(value >> 3*8),
                 (byte)(value >> 2*8),
                 (byte)(value >> 1*8),
                 (byte)(value ),
                 };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(int value, int offset = 0, bool isReverse = false)
        {
            if (isReverse)
            {
                return new byte[] {
                 (byte)(value >> ((offset)%4+2)*8),
                 (byte)(value >> ((offset)%4+3)*8),
                 (byte)(value >> ((offset)%4+0)*8),
                 (byte)(value >> ((offset)%4+1)*8),
                 };
            }
            else
            {
                return new byte[] {
                 (byte)(value >> ((offset)%4+3)*8),
                 (byte)(value >> ((offset)%4+2)*8),
                 (byte)(value >> ((offset)%4+1)*8),
                 (byte)(value >> ((offset)%4+0)*8),
                 };
            }
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(int value)
        {
            return new byte[] {
                 (byte)(value >> 3*8),
                 (byte)(value >> 2*8),
                 (byte)(value >> 1*8),
                 (byte)(value ),
                 };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(Int64 value, int offset = 0, bool isReverse = false)
        {
            if (isReverse)
            {
                return new byte[] {
                 (byte)(value >> ((offset)%8+6)*8),
                 (byte)(value >> ((offset)%8+7)*8),
                 (byte)(value >> ((offset)%8+4)*8),
                 (byte)(value >> ((offset)%8+5)*8),
                 (byte)(value >> ((offset)%8+2)*8),
                 (byte)(value >> ((offset)%8+3)*8),
                 (byte)(value >> ((offset)%8+0)*8),
                 (byte)(value >> ((offset)%8+1)*8),
                 };
            }
            else
            {
                return new byte[] {
                 (byte)(value >> ((offset)%8+7)*8),
                 (byte)(value >> ((offset)%8+6)*8),
                 (byte)(value >> ((offset)%8+5)*8),
                 (byte)(value >> ((offset)%8+4)*8),
                 (byte)(value >> ((offset)%8+3)*8),
                 (byte)(value >> ((offset)%8+2)*8),
                 (byte)(value >> ((offset)%8+1)*8),
                 (byte)(value >> ((offset)%8+0)*8),
                 };
            }
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(Int64 value)
        {
            return new byte[] {
                 (byte)(value >> 7*8),
                 (byte)(value >> 6*8),
                 (byte)(value >> 5*8),
                 (byte)(value >> 4*8),
                 (byte)(value >> 3*8),
                 (byte)(value >> 2*8),
                 (byte)(value >> 1*8),
                 (byte)( value),
                 };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(UInt64 value, int offset = 0, bool isReverse = false)
        {
            if (isReverse)
            {
                return new byte[] {
                 (byte)(value >> ((offset)%8+6)*8),
                 (byte)(value >> ((offset)%8+7)*8),
                 (byte)(value >> ((offset)%8+4)*8),
                 (byte)(value >> ((offset)%8+5)*8),
                 (byte)(value >> ((offset)%8+2)*8),
                 (byte)(value >> ((offset)%8+3)*8),
                 (byte)(value >> ((offset)%8+0)*8),
                 (byte)(value >> ((offset)%8+1)*8),
                 };
            }
            else
            {
                return new byte[] {
                 (byte)(value >> ((offset)%8+7)*8),
                 (byte)(value >> ((offset)%8+6)*8),
                 (byte)(value >> ((offset)%8+5)*8),
                 (byte)(value >> ((offset)%8+4)*8),
                 (byte)(value >> ((offset)%8+3)*8),
                 (byte)(value >> ((offset)%8+2)*8),
                 (byte)(value >> ((offset)%8+1)*8),
                 (byte)(value >> ((offset)%8+0)*8),
                 };
            }
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(UInt64 value)
        {
            return new byte[] {
                 (byte)(value >> 7*8),
                 (byte)(value >> 6*8),
                 (byte)(value >> 5*8),
                 (byte)(value >> 4*8),
                 (byte)(value >> 3*8),
                 (byte)(value >> 2*8),
                 (byte)(value >> 1*8),
                 (byte)( value),
                 };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(byte value)
        {
            return new byte[] { value };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(float value)
        {
            return BitConverter.GetBytes(value); 
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(float value,int mode=0)
        {
            return BytesHighLowConvertNew(ToBytes(value),mode); 
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(short value, int mode = 0)
        {
            return BytesHighLowConvertNew(ToBytes(value), mode);
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(ushort value, int mode = 0)
        {
            return BytesHighLowConvertNew(ToBytes(value), mode);
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(int value, int mode = 0)
        {
            return BytesHighLowConvertNew(ToBytes(value), mode);
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(uint value, int mode = 0)
        {
            return BytesHighLowConvertNew(ToBytes(value), mode);
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(short value, bool isReverse = false)
        {
            return new byte[]{
                     (byte)(value >> (isReverse?0:8)),
                      (byte)(value >> (isReverse?8:0))
                };
        }

        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(short value)
        {
            return new byte[]{
                     (byte)(value >> 8),
                      (byte)(value >> 0)
                };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(ushort value, bool isReverse = false)
        {
            return new byte[]{
                     (byte)(value >>(isReverse?0:8)),
                      (byte)(value >> (isReverse?8:0))
                };
        }
        /// <summary>转换为 Bytes。</summary>
        public static byte[] ToBytes(ushort value)
        {
            return new byte[]{
                     (byte)(value >>8),
                      (byte)(value >> 0)
                };
        }
        /// <summary>转换为 Byte。</summary>
        public static byte ToByte(string value, int Base = 10)
        {
            return  Convert.ToByte(value, Base);
        }
        /// <summary>ByteToString 方法。</summary>
        public static string ByteToString(byte value, int Base = 10)
        {
            return Convert.ToString(value, Base);
        }

        /// <summary>RemoveLetterGetNumber 方法。</summary>
        public static int RemoveLetterGetNumber(string value,bool mode=false)
        {
            if (mode)
            {
                if (value.Contains("."))
                {
                    string[] parts = value.Split('.');
                    string buffer = parts[parts.Length - 1].ToUpper().Replace(@"Q", "").Replace(@"W", "").Replace(@"E", "").Replace(@"R", "").Replace(@"T", "").Replace(@"Y", "").Replace(@"U", "")
        .Replace(@"I", "").Replace(@"O", "").Replace(@"P", "").Replace(@"A", "").Replace(@"S", "").Replace(@"D", "").Replace(@"F", "").Replace(@"G", "").Replace(@"H", "").Replace(@"J", "")
        .Replace(@"K", "").Replace(@"L", "").Replace(@"Z", "").Replace(@"X", "").Replace(@"C", "").Replace(@"V", "").Replace(@"B", "").Replace(@"N", "").Replace(@"M", "")
        .Replace(" ", "").Replace("\0", "").Replace("\r", "").Replace("\n", "");
                    return Convert.ToInt32(buffer);
                }
                else
                {
                    string buffer = value.ToUpper().Replace(@"Q", "").Replace(@"W", "").Replace(@"E", "").Replace(@"R", "").Replace(@"T", "").Replace(@"Y", "").Replace(@"U", "")
        .Replace(@"I", "").Replace(@"O", "").Replace(@"P", "").Replace(@"A", "").Replace(@"S", "").Replace(@"D", "").Replace(@"F", "").Replace(@"G", "").Replace(@"H", "").Replace(@"J", "")
        .Replace(@"K", "").Replace(@"L", "").Replace(@"Z", "").Replace(@"X", "").Replace(@"C", "").Replace(@"V", "").Replace(@"B", "").Replace(@"N", "").Replace(@"M", "")
        .Replace(" ", "").Replace("\0", "").Replace("\r", "").Replace("\n", "");
                    return Convert.ToInt32(buffer);
                }
            }
            else
            {
                if (value.Contains("."))
                {
                    string[] parts = value.Split('.');
                    return Convert.ToInt32(parts[parts.Length - 1]);
                }
                else {
                    return Convert.ToInt32(Regex.Replace(value, @"[^0-9]+", ""));
                }
            }

        }

        /// <summary>获取 indexBoolFirstTrue。</summary>
        public static int GetIndexBoolFirstTrue(byte[] values)
        {
            bool[] bools = ToBools(values);
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>获取 indexBoolFirstTrue。</summary>
        public static int GetIndexBoolFirstTrue(bool[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>判断是否 Equal。</summary>
        public static bool IsEqual(byte[] valuesA, byte[] valuesB)
        {
            if (valuesA==null&& valuesB == null)
            {
                return true;
            }
            if (valuesA == null || valuesB == null)
            {
                return false;
            }
            if (valuesA.Length!= valuesB.Length)
            {
                return false;
            }
            bool isequal=true;
            for (int i = 0; i < valuesA.Length; i++) {
                if (valuesA[i]!= valuesB[i])
                {
                    isequal = false;
                    break;
                }
            }
            return isequal;
        }
#region 异或校验和

/// <summary>
/// 计算当前所有字节的逐字节异或校验和（1 字节）。
/// </summary>
public byte GetXorChecksum()
{
    byte cs = 0;
    for (int i = 0; i < bytes.Count; i++)
    {
        cs ^= bytes[i];
    }
    return cs;
}

/// <summary>
/// 计算指定范围字节的逐字节异或校验和（1 字节）。
/// </summary>
public byte GetXorChecksum(int start, int length)
{
    byte cs = 0;
    for (int i = start; i < start + length; i++)
    {
        cs ^= bytes[i];
    }
    return cs;
}

/// <summary>
/// 获取 2 字节异或校验和字段。
/// 默认大端：00 CS；highFirst=false 小端：CS 00。
/// </summary>
public byte[] GetXorChecksumBytes(bool highFirst = true)
{
    byte cs = GetXorChecksum();
    return highFirst ? new byte[] { 0x00, cs } : new byte[] { cs, 0x00 };
}

/// <summary>
/// 将 2 字节异或校验和追加到当前字节末尾。
/// 默认大端：00 CS。
/// </summary>
public void AddXorChecksum(bool highFirst = true)
{
    Add(GetXorChecksumBytes(highFirst));
}

/// <summary>
/// 校验当前字节末尾 2 字节是否为前面所有字节的异或校验和。
/// 默认大端：00 CS。
/// </summary>
public bool VerifyXorChecksum(bool highFirst = true)
{
    if (bytes.Count < 2)
        return false;

    byte cs = 0;
    for (int i = 0; i < bytes.Count - 2; i++)
    {
        cs ^= bytes[i];
    }

    if (highFirst)
        return bytes[bytes.Count - 2] == 0x00 && bytes[bytes.Count - 1] == cs;
    else
        return bytes[bytes.Count - 2] == cs && bytes[bytes.Count - 1] == 0x00;
}

#endregion
    }
}
