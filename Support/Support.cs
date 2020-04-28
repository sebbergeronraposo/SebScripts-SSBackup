using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Support
{
    //Code double checked on 2/21/2020
    public static class Support 
    {
        #region methods
        public static bool CheckPath(string Path, PathType PathType)
        {
            try
            {
                switch (PathType)
                {
                    case PathType.Directory:
                        if (Directory.Exists(Path))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case PathType.File:
                        if (File.Exists(Path))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        return false;
                }
            }
            catch(Exception EX)
            {
                throw EX; 
            }
            
        }
        internal static void CreateFolder(string Path)
        {
            try
            {
                Directory.CreateDirectory(Path);
            }
            catch (Exception EX)
            {
                throw EX;
            }
        }       
        public static void Encrypt(string FilePath)
        {
            try
            {
                File.Encrypt(FilePath); 
            }
            catch (Exception ex)
            {
                throw ex; 
            }
        }
        public static void Decrypt(string FilePath)
        {
            try
            {
                File.Decrypt(FilePath);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static void Serialize(string[] items, string filePath)
        {
            if (items.Length == 0 || items is null) return; 
            try
            {
                using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter binarySerializer = new BinaryWriter(fileStream, Encoding.Unicode))
                    {
                        binarySerializer.Write(string.Join("|", items));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; 
            }

        }
        public static string[] Deserialize(string filePath)
        {
            string fileContent;
            string[] stringSeparator = new string[] { "|" };
            try
            {
                using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryDeserializer = new BinaryReader(fileStream, Encoding.Unicode))
                    {
                        fileContent = binaryDeserializer.ReadString();
                    }

                }
                return fileContent.Split(stringSeparator, StringSplitOptions.None);
            }
            
            catch (Exception ex)
            {
                throw ex; 
            }

        }
        public static Dictionary<string, double> TimeConverter(double Time)
        {
            Dictionary<string, double> TimeDictionary = new Dictionary<string, double>();
            TimeDictionary.Add("ms", Time);
            if (Time / 1000 < 1)
            {
                return TimeDictionary;
            }
            TimeDictionary.Add("s", Time / 1000);
            if ((Time / 1000) / 60 < 1)
            {
                return TimeDictionary;
            }
            TimeDictionary.Add("m", (Time / 1000) / 60);
            if (((Time / 1000) / 60) / 60 < 1)
            {
                return TimeDictionary;
            }
            TimeDictionary.Add("h", ((Time / 1000) / 60) / 60);
            return TimeDictionary;
        }
        public static Dictionary<string, double> ByteConverter(long numberOfBytes)
        {
            Dictionary<string, double> dataDictionary = new Dictionary<string, double>();
            long absolutenumberOfBytes = (numberOfBytes < 0 ? -numberOfBytes : numberOfBytes);

            if (absolutenumberOfBytes <= 0x400)
            {
                dataDictionary.Add("Bytes", 0);
                return dataDictionary;
            }
            else
            {
                dataDictionary.Add("Bytes", (double)absolutenumberOfBytes);
            }
            if (absolutenumberOfBytes <= 0x100000)
            {
                dataDictionary.Add("KB", (double)absolutenumberOfBytes / 1024);
                return (dataDictionary);
            }
            else
            {
                dataDictionary.Add("KB", (double)absolutenumberOfBytes / 1024);
            }
            if (absolutenumberOfBytes <= 0x40000000)
            {
                dataDictionary.Add("MB", (double)(numberOfBytes >> 10) / 1024);
                return dataDictionary;
            }
            else
            {
                dataDictionary.Add("MB", (double)(numberOfBytes >> 10) / 1024);
            }

            if (absolutenumberOfBytes <= 0x10000000000)
            {
                dataDictionary.Add("GB", (double)(numberOfBytes >> 20) / 1024);
                return dataDictionary;
            }
            else
            {
                dataDictionary.Add("GB", (double)(numberOfBytes >> 20) / 1024);
                dataDictionary.Add("TB", (double)(numberOfBytes >> 30) / 1024);
                return dataDictionary;
            }
        }
        public static string[] ReturnFrequency(string ConfigTime)
        {
            return new string[] { ConfigTime.Substring(0, 3), ConfigTime.Substring(3, 2), ConfigTime.Substring(5, 2), ConfigTime.Substring(7, 3) };

        }
        #endregion     
    }
}





