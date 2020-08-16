using System;
namespace M66FileIO
{
    public class FileInf
    {
        public string name;
        public string length;
        public int lenghtInt { get { return Convert.ToInt32(length); } }
        public string content;
        public FileInf(string value1, string value2)
        {
            this.name = value1;
            this.length = value2;
        }
    }
}