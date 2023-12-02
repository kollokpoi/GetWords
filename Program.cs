// See https://aka.ms/new-console-template for more information
using System.Linq;
using System.Text;

Console.WriteLine("Hello, World!");
List<string> allWords = new();
List<string> filesReaded = new();
string russianCharacters = "йцукенгшщзхъфывапрлджэячсмитьбю";
string englishCharacters = "qwertyuiopasdfghjklzxcvbnm";
string banCharacters = "1234567890-=\\!@#$%^&*()_+|/'][`~·‧־꡵";
List<string> acceptedExtentions = new()
{
    ".pdf",
    ".txt",
    ".doc",
    ".docx",
    ".html",
    ".testlog",
    ".xml",
    ".sgm"
};

string baseFolder = @"C:\";
GetWords();
using StreamWriter russianWriter = new(@"D:\Words.txt", true, Encoding.UTF8);
using StreamWriter englishWriter = new(@"D:\EnglishWords.txt", true, Encoding.UTF8);
using StreamWriter filesReadedWriter = new(@"D:\FilesReaded.txt", true, Encoding.UTF8);

ScanFolder(baseFolder);

void GetWords()
{
    var streamReader = new StreamReader(@"D:\Words.txt", Encoding.UTF8);
    while (!streamReader.EndOfStream)
    {
        var str = streamReader.ReadLine();

        if (str == null)
            continue;
        allWords.Add(str);
    }
    streamReader.Close();
    streamReader = new StreamReader(@"D:\EnglishWords.txt", Encoding.UTF8);
    while (!streamReader.EndOfStream)
    {
        var str = streamReader.ReadLine();

        if (str == null)
            continue;
        allWords.Add(str);
    }
    streamReader.Close();
    streamReader = new StreamReader(@"D:\FilesReaded.txt", Encoding.UTF8);
    while (!streamReader.EndOfStream)
    {
        var str = streamReader.ReadLine();

        if (str == null)
            continue;
        var fileName = str.Split(":")[0];
        if (fileName!=null)
            filesReaded.Add(fileName);
    }
    streamReader.Close();
}

Console.ReadLine();


async void WriteWords(List<FileInfo> files)
{
    int position = 0;
    foreach (FileInfo file in files)
    {
        position++;
        if (!acceptedExtentions.Contains(file.Extension))
            continue;

        var streamReader = null as StreamReader;
        try
        {
            streamReader  = new StreamReader(file.FullName, Encoding.UTF8);
        }
        catch
        {}
        if (streamReader is null)
            continue;
        while (!streamReader.EndOfStream)
        {
            var str = null as string;
            try
            {
                str = streamReader.ReadLine();
            }
            catch
            {}

            if (str == null)
                continue;
            var words = str.Replace(".", " ").Replace(",", " ").Replace("<", " ").Replace(">", " ").Replace("=", " ").Replace("=", " ")
                .Replace("\"", " ").Replace("}", " ").Replace("{", " ").Replace("!", " ").Replace("/", " ")
                .Replace("?", " ").Replace(":", " ").Replace(";", " ").Replace("﹀", " ").Replace("᱾", " ")
                .Replace("⟆", " ").Replace("܉", " ").Replace("٬", " ").Replace("*", " ").Replace("-", " ").Split(' '); 

            foreach (var word in words)
            {
                if (allWords.Contains(word))
                    continue;
                bool goodWord = true;
                bool english = true;
                bool russian = true;
                int i = 0;
                foreach (var character in word)
                {
                    if (banCharacters.Contains(character))
                    {
                        goodWord = false;
                        break;
                    }
                    if (Char.IsDigit(character))
                    {
                        goodWord = false;
                        break;
                    }
                    if (char.IsUpper(character) && i!=0)
                    {
                        goodWord = false;
                        break;
                    }
                    if (!(russianCharacters.Contains(Char.ToLower(character)) || englishCharacters.Contains(Char.ToLower(character))))
                    {
                        goodWord = false;
                        break;
                    }
                    if (Char.IsLetter(character))
                    {
                        if (russianCharacters.Contains(Char.ToLower(character)) || englishCharacters.Contains(Char.ToLower(character)))
                        {
                            if (!russianCharacters.Contains(Char.ToLower(character)))
                            {
                                russian = false;
                            }
                            if (!englishCharacters.Contains(Char.ToLower(character)))
                            {
                                english = false;
                            }
                        }
                        else
                        {
                            goodWord = false;
                        }
                    }
                    else if (!Char.IsPunctuation(character))
                    {
                        goodWord = false;
                    }
                    i++;
                }
                if (goodWord)
                {
                    Console.WriteLine(word);
                    try
                    {
                        if (russian)
                        {
                            await russianWriter.WriteLineAsync(word);
                            await russianWriter.FlushAsync();
                        }
                        else if (english)
                        {
                            await englishWriter.WriteLineAsync(word);
                            await englishWriter.FlushAsync();
                        }

                    }
                    catch { };

                    allWords.Add(word);
                }
            }
        }
        
        if (!filesReaded.Contains(file.FullName))
        {
            try
            {
                await filesReadedWriter.WriteLineAsync($"{file.FullName} : {Math.Round((double)streamReader.BaseStream.Length / (1024), 2)}кб");
            }
            catch 
            {}
            
            filesReaded.Add(file.FullName);
        }
        streamReader.Close();
    }
}

List<FileInfo> ScanFolder(string path)
{
    List<FileInfo> filesList = new List<FileInfo>();
    int lastCount = 1;
    try
    {
        var directories = Directory.GetDirectories(path);
        if (directories.Length > 0)
        {
            foreach (var item in directories)
            {
                filesList.AddRange(ScanFolder(item));

                if (path == baseFolder)
                {
                    List<FileInfo> filesToWrite = new List<FileInfo>();
                    for (int i = lastCount-1; i < filesList.Count; i++)
                    {
                        filesToWrite.Add(filesList[i]);
                    }
                    Task.Run(()=> WriteWords(filesToWrite));
                    lastCount = filesList.Count;
                }
            }
        }
        var files = Directory.GetFiles(path);
        foreach (var item in files)
        {
            var fileInfo = new FileInfo(item);
            if (acceptedExtentions.Contains(fileInfo.Extension))
            {
                Console.WriteLine(fileInfo.FullName);
                filesList.Add(fileInfo);
            }
        }
        return filesList;
    }
    catch
    {
        return filesList;
    }
}