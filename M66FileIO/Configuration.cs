using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.IO;
namespace M66FileIO
{
    
    public class Configuration
    {
        
        public Dictionary<string, string> files;
        [JsonProperty]
        public string defaultdownloadfolder;
        [JsonProperty]
        public string defaultuploadfolder;
        [JsonProperty]
        public string ComPort;
        [JsonProperty]
        public int BaudRate;
        [JsonConstructor]
        public Configuration(string defaultdownloadfolder, string defaultuploadfolder,string Comport,int Baud)
        {
             
            this.defaultdownloadfolder = defaultdownloadfolder;
            this.defaultuploadfolder = defaultuploadfolder;
            this.ComPort = Comport;
            this.BaudRate = Baud;
            if (!Directory.Exists(defaultdownloadfolder))
            {
                Directory.CreateDirectory(defaultdownloadfolder);
                Console.WriteLine($"Created directory {defaultuploadfolder} for download");
            }
            if (!Directory.Exists(defaultuploadfolder))
            {
                Directory.CreateDirectory(defaultuploadfolder);
                Console.WriteLine($"Created directory {defaultuploadfolder} for upload");
            }
            else
            {
                this.files = new Dictionary<string, string>();
                var files=Directory.GetFiles(defaultuploadfolder);
                foreach (var file in files)
                {
                    var list= file.Split('\\');
                    string filename = list.Last();
                    this.files.Add(filename, file);
                }
            }
        }
        public string ConsoleList()
        {
            int count = 1; ;
            string finalString = "";
            foreach (var item in files)
            {
                string line = $"{count++} - {item.Key} :{item.Value}";
                Console.WriteLine(line);
                finalString += line;
            }
            return finalString;
        }
      
    }
}
