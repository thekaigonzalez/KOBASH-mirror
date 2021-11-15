using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

void Error(string text)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("error: ");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine(text);
}

void Warning(string text)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("WARN: ");

    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine(text);
}

void Log(string text)
{
    Console.ForegroundColor = ConsoleColor.DarkBlue;
    Console.Write("log: ");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine(text);
}

string PassPrompt()
{
    string password = "";
    while (true)
    {
        var key = System.Console.ReadKey(true);
        if (key.Key == ConsoleKey.Enter)
            break;
        password += key.KeyChar;
    }

    return password;
}

#pragma warning disable CS8321 // Local function is declared but never used
void RunExecutable(string path, string args = "")
#pragma warning restore CS8321 // Local function is declared but never used
{
   
    // Use ProcessStartInfo class
    ProcessStartInfo startInfo = new();

    startInfo.CreateNoWindow = false;
    startInfo.UseShellExecute = false;
    startInfo.FileName = path;
    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
    startInfo.Arguments = args;
    // startInfo.UseShellExecute = true;
    // startInfo.UserName = "";
    
    try
    {
        // Start the process with the info we specified.
        // Call WaitForExit and then the using statement will close.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
       // Run the external process & wait for it to finish
        using (Process proc = Process.Start(startInfo))
        {
            proc.WaitForExit();

            
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    catch
    {
        // Log error.
    }
}

var SecurityKey = "admin";

if (!File.Exists("normal/check.key"))
{

    Log("Add a custom Cipher key");

    SecurityKey = Console.ReadLine();
}

//This method is used to convert the plain text to Encrypted/Un-Readable Text format.
string EncryptPlainTextToCipherText(string PlainText)
{
    // Getting the bytes of Input String.
    byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);

    MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
    //Gettting the bytes from the Security Key and Passing it to compute the Corresponding Hash Value.
    byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
    //De-allocatinng the memory after doing the Job.
    objMD5CryptoService.Clear();

    var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
    //Assigning the Security key to the TripleDES Service Provider.
    objTripleDESCryptoService.Key = securityKeyArray;
    //Mode of the Crypto service is Electronic Code Book.
    objTripleDESCryptoService.Mode = CipherMode.ECB;
    //Padding Mode is PKCS7 if there is any extra byte is added.
    objTripleDESCryptoService.Padding = PaddingMode.PKCS7;


    var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
    //Transform the bytes array to resultArray
    byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
    objTripleDESCryptoService.Clear();
    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
}

//This method is used to convert the Encrypted/Un-Readable Text back to readable  format.
string DecryptCipherTextToPlainText(string CipherText)
{
    byte[] toEncryptArray = Convert.FromBase64String(CipherText);
    MD5CryptoServiceProvider objMD5CryptoService = new();

    //Gettting the bytes from the Security Key and Passing it to compute the Corresponding Hash Value.
    byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
    objMD5CryptoService.Clear();

    var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
    //Assigning the Security key to the TripleDES Service Provider.
    objTripleDESCryptoService.Key = securityKeyArray;
    //Mode of the Crypto service is Electronic Code Book.
    objTripleDESCryptoService.Mode = CipherMode.ECB;
    //Padding Mode is PKCS7 if there is any extra byte is added.
    objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

    var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
    //Transform the bytes array to resultArray
    byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
    objTripleDESCryptoService.Clear();

    //Convert and return the decrypted data/byte into string format.
    return UTF8Encoding.UTF8.GetString(resultArray);
}

void loadPath(string path)
{
    var path_file = File.ReadAllLines(path);

    foreach (string line in path_file)
    {
        Console.WriteLine(line);
    }
}

void firstTimeSetup()
{
    bool alre = true;
    Log("starting setup");
    if (!Directory.Exists("etc"))
    {
        alre = false;
        Warning("firstTimeSetup:47:Program.cs: 'etc' not found: fixing corrupt.");

        Directory.CreateDirectory("etc");
    } 
    
    if (!Directory.Exists("normal"))
    {
        Warning("firstTimeSetup():78:Program.cs: warning: there won't be any commands due to the lack of 'normal' directory. use the command-get tool to manage commands.");

        alre = false;

        Directory.CreateDirectory("normal");
        Directory.CreateDirectory("normal/bin");
    }

    if (!Directory.Exists("local"))
    {
        Error("no 'local' directory found. trying to save...");
        Directory.CreateDirectory("local");
    }


    if (!File.Exists("normal/access.key"))
    {
        

        Error("using " + Environment.UserName + " as the visor user");
        Error("Saving Encrypted UserName...");

        File.WriteAllText("normal/access.key", EncryptPlainTextToCipherText(Environment.UserName));

        Log("saved username");

        System.Console.Write("enter a secret key: ");
        
        string password = PassPrompt();
        Console.Write("\n");
        File.WriteAllText("normal/check.key", EncryptPlainTextToCipherText(password));

        Log("Successfully logged.");

    }

    if (!File.Exists("etc/paths.txt"))
    {
        string m = Environment.CurrentDirectory.ToString() + "/normal/bin".ToString();
        File.WriteAllText("etc/paths.txt", m);
        Error("cannot find file paths");
        alre = false;
    }

    try {
    if (!File.Exists("C:\\pathregistry.txt")) {
    
        Error("kobash: Fixing registry...");

        Log("Backing up...");

        File.Copy("etc/paths.txt", "C:\\pathregister.txt");

        alre = false;
    }
    } catch (Exception) {
        Log("registry already exists");
    }

    if (alre)
    {
        Log("Nothing's corrupt");
    }
}

string RunExpression(string expr)
{
    int state = 0;

    List<string> arr = new();
    List<string> archive_array = new();
    string buffer = "";

    string fname = "";
    for (int i = 0; i < expr.Length; ++ i)
    {
        if (expr[i] == ' ' && state == 0)
        {
            fname = buffer;
            /*Console.WriteLine("Func " + fname);*/
            state = 1;
            buffer = "";
        }
        else if (expr[i] == ' ' && state == 1)
        {
            arr.Add(buffer);
            /*Console.WriteLine("arg " + buffer);*/
            buffer = "";
        }
        else if (expr[i] == '%' && state == 1) {
            state = 999;
        }
        else if (expr[i] == '"' && state == 1)
        {
            state = 299;
        } 
        else if (expr[i] == '"' && state == 299)
        {
            state = 1;
        }
        else
        {
            buffer += expr[i];
        }
    }

    if (buffer.Length > 0 && state == 1)
    {
        arr.Add(buffer);
        state = 0;
        buffer = "";
    } else if (buffer.Length >0 && state == 0)
    {
        fname = buffer.Trim();
        state = 0;
        buffer = "";
    }
    archive_array = new(arr);
    // Console.WriteLine("archived " + archive_array[0]);
    for (int i = 0; i < arr.Count; ++ i)
    {
        arr[i] = "\"" + arr[i] + "\"";
    }
    bool found = false;
    try {
        if (File.Exists("etc/paths.txt")) {
            foreach (string s in File.ReadAllLines("etc/paths.txt"))
            {
                if (File.Exists(s + "/" + fname + ".exe"))
                {
                    found = true;
                    RunExecutable(s + "/" + fname + ".exe", string.Join(' ', arr));
                    break;
                }
            }
        }

        if (File.Exists("C:\\pathregister.txt") && found == false) {
            foreach (string s in File.ReadAllLines("C:\\pathregister.txt"))
            {
                if (File.Exists(s + "/" + fname + ".exe"))
                {
                    found = true;
                    RunExecutable(s + "/" + fname + ".exe", string.Join(' ', arr));
                    break;
                }
            }
        }
    } catch (Exception) {
        Console.WriteLine("kobash: Fixing registry...");

        Console.WriteLine("Backing up...");

        File.Copy("etc/paths.txt", "C:\\pathregister.txt");
    }

    if (!found)
    {
        if (fname == "cd")
        {
            Console.WriteLine("archive cd in to " + archive_array[0]);
            Directory.SetCurrentDirectory(archive_array[0]);
        } else if (fname == "clear") {
            Console.Clear();
        } else {
            Error("command not found!");
        }
    }
    
    /*RunExecutable(fname, string.Join(' ', arr));*/
    return "clear";
}

firstTimeSetup();
Console.Write("MINGW32\n");
while (true)
{
    Console.ForegroundColor = ConsoleColor.Magenta;

    Console.ForegroundColor = ConsoleColor.Green;

    Console.Write(Environment.CurrentDirectory);

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(" $ ");
    Console.ForegroundColor = ConsoleColor.Gray;
    RunExpression(Console.ReadLine());
}