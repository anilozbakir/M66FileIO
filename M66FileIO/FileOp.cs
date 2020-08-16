using Print = System.Console;

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace M66FileIO
{
    public class FileOp : IFileOp
    {
        private SerialPort serialport;
        public string prefix;
        public bool debug;
        private OperationStates _operationState;
        public string fileForWrite;
        public string lastcontent;

        public OperationStates operationState { get { return _operationState; } set {
                this._operationState = value;
                Console.WriteLine($"operation {operationState}");
            } }
      //  public OperationStates operationState;
        public List<FileInf> files { get; private set; }
        public bool resultOP { get; private set; }
        public bool FilenameOK { get; set; }

        public FileOp(SerialPort sp)
        {
            this.serialport = sp;
        }



        public async Task<string> GetContent(string filename)
        {
            var str = await this.Send($"\n\rAT+QFDWL=\"{filename}\"\n\r", 1000);
            if (str != null)
            {
                string filecontent1 = "CONNECT\x0d";
                string filecontent2 = "[+]QFDWL:";
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
                var regexFiles1 = new Regex(filecontent1, options);
                var regexFiles2 = new Regex(filecontent2, options);
                var matchFiles1 = regexFiles1.Matches(str);
                var matchFiles2 = regexFiles2.Matches(str);
                List<FileInf> list = new List<FileInf>();
                var dic = files.ToDictionary(x => x.name, x => x);
                if (matchFiles1.Count > 0 && matchFiles2.Count > 0)
                {
                    if (matchFiles1[0].Index >= 0 && matchFiles2[0].Index >= 0 && (matchFiles2[0].Index >= matchFiles1[0].Index))
                    {
                        int start = matchFiles1[0].Index + filecontent1.Length;
                        string sSub = str.Substring(start, matchFiles2[0].Index - start);

                        if (dic.TryGetValue(filename, out FileInf fo))
                        {
                            fo.content = Trim(sSub); ;
                            return Trim(sSub);
                        }
                    }
                }

            }
            return null;
        }

        private string Trim(string sSub)
        {
            return sSub.Replace('\n', ' ').Replace('\r', ' ').Trim();
        }

        public async Task<FileInf[]> ListFiles()
        {

            var str = await this.Send("\n\rAT+QFLST\n\r", 1000);
            if (str != null)
            {

                string filenameEx = "\"([\\w| |\x2E]+)+\",";
                string filelength = ",[\\d]+\r";
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
                var regexFiles = new Regex(@filenameEx, options);
                var regexLength = new Regex(@filelength, options);
                var matchFiles = regexFiles.Matches(str);
                var matchLength = regexLength.Matches(str);
                int i = 0;
                List<FileInf> list = new List<FileInf>();
                while (i < matchFiles.Count && i < matchLength.Count)
                {
                    list.Add(new FileInf(matchFiles[i].Value.Replace('\"', ' ').Replace(',', ' ').Trim(), matchLength[i].Value.Replace('\"', ' ').Replace(',', ' ').Trim()));
                    i++;
                }
                string s= i > 1 ? "s":"";
                Console.WriteLine($"{i} file{s} found");
                this.files = list;
                return list.ToArray();
                //  return new Task<FileInf[]>(() => { return list.ToArray(); });
            }
            else
            {
                this.files = new List<FileInf>();
                return null;
            }
        }
        public void ConsoleList()
        { int count = 1;
            Console.WriteLine("");
            foreach (FileInf file in this.files)
            {
                Console.WriteLine($" {count++} - {file.name}");

            }
        }
        private async Task<string> Send(string v, int timeout)
        {
            int timeoutCount = 0;
            if (serialport != null && serialport.IsOpen)
            {
                this.serialport.Write(v);
                if (debug)
                {
                    Console.WriteLine(v);
                }
                while (true)
                {
                Thread.Sleep(10);  
                        if (serialport.BytesToRead > 0)
                    {//there is a  new packet.

                        Thread.Sleep(50);//wait for 50ms more to finish 
                        var buff = new byte[serialport.BytesToRead];
                        this.serialport.Read(buff, 0, serialport.BytesToRead);
                        if (debug)
                        {
                            Console.WriteLine(Encoding.ASCII.GetString(buff));
                        }
                        return Encoding.ASCII.GetString(buff);
                    }   
                    timeoutCount++;
                    if (timeoutCount > (timeout / 10)) return null;
                }
             
               
              

            }
            return null;
        }

        public async Task<string> OpenFile(string filename)
        {
            var str = await this.Send($"\n\r AT + QFOPEN = \"{filename}\"\n\r", 1000);
            if (str != null)
            {
                string Prefix = "[+]QFOPEN:";
                string handle = ":.[\\d]+";
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
                var regexFiles = new Regex(@Prefix, options);
                var regexLength = new Regex(@handle, options);
                var prefixmatch = regexFiles.Matches(str);
                var handlenumber = regexLength.Matches(str);
                if (prefixmatch.Count > 0 && handlenumber.Count > 0 && handlenumber[0].Index >= 0 && handlenumber[0].Length > 0)
                {
                    string prefix = Trim(handlenumber[0].Value.Replace(':', ' '));
                    this.prefix = prefix;
                    Console.WriteLine($"handle of file {prefix}\n\r");
                    this.resultOP = true;
                    return prefix;
                }
                
            }
            this.resultOP = false;
            return null;
        }
        public async Task<bool> WriteFile( string content)
        {
            this.resultOP = false;
            if (prefix != null)
            {
                List<char> buff = new List<char>();
                buff.AddRange(content.ToArray());
                buff.Add('\0');
                var str = await this.Send($"\n\r AT + QFWRITE = {prefix},{buff.Count}\n\r", 1000);
                if (str.Contains("CONNECT"))
                {
                    
                    
                    str = await this.Send($" {new string(buff.ToArray())}\n\r", 1000);
                    if (str.Contains("+QFWRITE:"))
                    {
                      
                        this.CloseFile().Wait();
                        this.resultOP = true ;
                        return true;
                    }
                }



            }
            this.resultOP = false;
            return false;

        }

        public async Task<bool> AppendFile( string content)
        {

            if (prefix != null)
            {
                var str = await this.Send($"\n\r AT+QFSEEK = {prefix},0,2\n\r", 1000);
                if (str.Contains("OK"))
                {
                    return await WriteFile(content);
                }


            }
            this.resultOP = false;
            return false;
        }
        public async Task<bool> DeleteFile(string filename)
        {
            var str = await this.Send($"\n\r AT+QFDEL= \"{filename}\"\n\r", 1000);
            if (str.Contains("OK"))
            {
               
                this.CloseFile().Wait();
                this.resultOP = true;
                return true;
            }
            return false;
        }
    
        public async Task<bool> CloseFile()
        {
            if (prefix != null)
            {
                var str = await this.Send($"\n\r AT+QFCLOSE= {prefix}\n\r", 1000);
                if (str.Contains("OK"))
                {
                    this.resultOP = true;
                    return true;
                }
            }
            else
            {
                Console.WriteLine("no file to close");
            }
            this.resultOP = false;
            return false;
        }
        public async Task<bool> Close()
        {
            var str = await this.Send($"\n\r AT+QFCLOSE=\n\r", 1000);
            if (str.Contains("OK"))
            {
                this.resultOP = true;
                return true;
            }
            //foreach (var item in files)
            //{

            //    if (prefix != null)
            //    {
            //        var str = await this.Send($"\n\r AT+QFCLOSE= {prefix}\n\r", 1000);
            //        if (str.Contains("OK"))
            //        {


            //        }
            //    }  
            //}

            return false;
        }
        public async Task<bool> ReadFile(int index,int length)
        {

            if (prefix != null)
            {
                var str = await this.Send($"\n\r AT+QFSEEK = {prefix},{index},0\n\r", 1000);
                if (str.Contains("OK"))
                {
                      str = await this.Send($"\n\r AT+QFREAD = {prefix},{length}\n\r", 1000);
                    this.resultOP = true;
                    this.CloseFile().Wait();
                    return true;
                }


            }
            this.resultOP = false;
            return false;
        }
        public async Task<bool> ReadFileContent(int index, int length)
        {

            if (prefix != null)
            {
                var str = await this.Send($"\n\r AT+QFSEEK = {prefix},{index},0\n\r", 1000);
                if (str.Contains("OK"))
                {
                    str = await this.Send($"\n\r AT+QFREAD = {prefix},{length}\n\r", 1000);
                    string Prefix = "CONNECT[ ]+[0-9]+[ ]*[\n\r]";
                    string handle = "OK[\n\r]";
                    RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
                    var regexFiles = new Regex(@Prefix, options);
                    var regexLength = new Regex(@handle, options);
                    var prefixmatch = regexFiles.Matches(str);
                    var handlenumber = regexLength.Matches(str);
                    if (prefixmatch.Count > 0 && handlenumber.Count > 0 && handlenumber[0].Index >= 0 && handlenumber[0].Length > 0)
                    {
                        int start = prefixmatch[0].Index + prefixmatch[0].Length;
                        this.lastcontent = str.Substring(start, handlenumber[0].Index- start);
                        this.resultOP = true;
                        this.CloseFile().Wait();
                        return this.resultOP;
                    }


                }
            }
            this.resultOP = false;
            return false;
        }
    }
}
