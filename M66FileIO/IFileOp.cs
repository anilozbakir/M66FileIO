
using System.Threading.Tasks;

namespace M66FileIO
{
    internal interface IFileOp
    {
        Task<FileInf[]> ListFiles();
        Task<string> GetContent(string filename);
        Task<string> OpenFile(string filename);
        Task<bool> WriteFile(string content);
        Task<bool> AppendFile(string content);
        Task<bool> ReadFile(int index,int length);
        Task<bool> CloseFile();
        Task<bool> DeleteFile(string filename);
    }
}