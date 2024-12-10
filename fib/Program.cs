using System.CommandLine;

using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

//to save changes :  dotnet publish "C:\Users\user1\Desktop\לימודים\פרקטיקוד\fib\fib\fib.csproj" -o publish
//to run this command :  fib boundle
//to create rsp file : fib create-rsp -o "fileName.rsp"
//to run rsp file : fib @fileName.rsp

//create options
var outputOption = new Option<FileInfo>("--output", "file path and name");
outputOption.AddAlias("-o");

var languageOption = new Option<string>("--language", "language files to boundle") 
{ IsRequired = true }.FromAmong("csharp", "c", "cpp", "java", "js", "html", "css", "scss", "ts", "sql", "python", "all");
languageOption.AddAlias("-l");

var noteOption = new Option<bool>("--note", "write the file name");
noteOption.AddAlias("-n");

var sortOption = new Option<string>("--sort", "The order of copying the files").FromAmong("abc", "type");
sortOption.SetDefaultValue("abc");
sortOption.AddAlias("-s");

var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "remove empty lines from source file before copy it to destination file");
removeEmptyLinesOption.AddAlias("-rel");

var authorOption = new Option<string>("--author", "write the name of the author in the add of the boundle file");
authorOption.AddAlias("-a");

//create commands
var boundleCommand = new Command("boundle", "boundle code files to a single file");
var rspComand = new Command("create-rsp", "create respond file with values to run");

//set options to commands
boundleCommand.AddOption(outputOption);
boundleCommand.AddOption(languageOption);
boundleCommand.AddOption(noteOption);
boundleCommand.AddOption(sortOption);
boundleCommand.AddOption(removeEmptyLinesOption);
boundleCommand.AddOption(authorOption);

rspComand.AddOption(outputOption);

//create languages dictinary
Dictionary<string, string> languages = new Dictionary<string, string>
        {
            { "csharp",".cs" },
            { "c", ".c" },
            { "cpp", ".cpp" },
            { "java", ".java" },
            { "js",".js" },
            { "html",".html" },
            { "css",".css" },
            { "scss",".scss" },
            { "ts",".ts" },
            { "sql", ".sql" },
            { "python",".python" },
            { "all","all" }
        };

//set functions to commands
rspComand.SetHandler((output) =>
{
    try
    {
        //create rsp file
        var rspFile = File.Create(output.FullName);
        rspFile.Close();
        File.AppendAllText(output.FullName, "boundle ");

        string path, language = "", sort = "", author;
        int isNote = -1, isRel = -1;

        //get values to command
        Console.WriteLine("enter path to boundle file");
        path = Console.ReadLine();
        do
        {
            Console.WriteLine("choose one of the flowing languages:");
            languages.Keys.ToList().ForEach(Console.WriteLine);
            language = Console.ReadLine();
        }
        while (!languages.ContainsKey(language));
        do
        {
            Console.WriteLine("Do you want to write the source file at the top of the file content?\n choose 1 if yes and 0 if not");
            isNote = int.Parse(Console.ReadLine());
        }
        while (isNote != 0 && isNote != 1);
        do
        {
            Console.WriteLine("how do you want to sort the files?\n choose one of the flowing options : abc, type");
            sort = Console.ReadLine();
        }
        while (sort != "abc" && sort != "type");
        do
        {
            Console.WriteLine("Do you want to delete blank lines before copying??\n choose 1 if yes and 0 if not");
            isRel = int.Parse(Console.ReadLine());
        }
        while (isRel != 0 && isRel != 1);
        Console.WriteLine("do you want to write the author name in the  top of the file?\n if yes enter author name if not enter 0");
        author = Console.ReadLine();

        //write command
        File.AppendAllText(output.FullName, "-o "+path+" ");   
        File.AppendAllText(output.FullName, "-l " + language + " "); 
        if (isNote == 1)
        {
            File.AppendAllText(output.FullName, "-n ");
        }
        File.AppendAllText(output.FullName, "-s "+sort+" ");
        if (isRel == 1)
        {
            File.AppendAllText(output.FullName, "-rel ");
        }
        if (author != "0")
            File.AppendAllText(output.FullName, "-a "+author);
    }
    catch 
    {
        Console.WriteLine("Error: file path is invalid");
    }
    
},outputOption);

boundleCommand.SetHandler((output,language,note,sort,rel,author) =>
{
    try
    {
        //create file in path
        var newFile = File.Create(output.FullName);
        newFile.Close();

        //find the selected language
        string selectedLang = languages[language];
        string fileType = language == "all"?"*.*":"*"+selectedLang;

        //get all files from the current directory
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        IEnumerable<FileInfo> files = directory.GetFiles(fileType, SearchOption.AllDirectories).Where(f => languages.ContainsValue(f.Extension));

        //sort the files according to --sort option
        if (sort == "abc")
            files = files.OrderBy(file => file.Name);
        else
            files = files.OrderBy(file => file.Extension);

        if (author != null)
            File.AppendAllText(output.FullName, "// author name: " + author + "\n\n");

        //copy the files valus to the new file
        foreach (FileInfo file in files)
        {
                if (note)
                {
                    File.AppendAllText(output.FullName,"//file sourc: "+ Path.GetRelativePath(output.FullName, file.FullName) + "\n");
                }
                if (rel)
                {
                    File.AppendAllLines(output.FullName, File.ReadAllLines(file.FullName).Where(l => !string.IsNullOrWhiteSpace(l)));
                }
                else
                {
                    File.AppendAllText(output.FullName, File.ReadAllText(file.FullName));
                }
                File.AppendAllText(output.FullName, "\n\n");
        }
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: file path is invalid");
    }
}, outputOption,languageOption,noteOption,sortOption,removeEmptyLinesOption,authorOption);

//create root command
var rootCommand = new RootCommand("root command for files boundler CLI");
rootCommand.AddCommand(boundleCommand);
rootCommand.AddCommand(rspComand);
rootCommand.InvokeAsync(args);

