using System.Net;

namespace BoggleAssist
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (!Init.Initialised) { };

            BoggleDictionary Boggle = new();

            bool _gameEnded = false;
            bool _admin = false;
            while (!_gameEnded)
            {
                Utils.MenuOptions(_admin);
                string _option = Console.ReadLine() + "";
                Console.Clear();

                switch (_option)
                {
                    case "1":
                        Console.WriteLine("Enter Word: ");
                        string _check = (Console.ReadLine() + "").ToLower();
                        Word Valid;
                        try
                        {
                            Valid = Boggle._dictionary.Where(x => x._word == _check).ToArray()[0];
                        }
                        catch { Valid = null; }

                        if (Valid == null) { Utils.EnterContinue($"{_check} isn't a valid word.", true); }
                        else
                        {
                            Utils.EnterContinue($"{_check} is a valid word, worth {Valid._val} points.", true);
                        }
                        break;
                    case "2":
                        throw new NotImplementedException();
                    case "admin123":
                        _admin = true;
                        Utils.EnterContinue("Admin status granted.", true);
                        break;
                    case "0":
                        Environment.Exit(0);
                        break;
                    default:
                        Utils.EnterContinue("Invalid input.", true);
                        break;
                }

                Console.Clear();
                Thread.Sleep(100);
            }
        }

        class Word
        {
            public string _word;
            public int _val;

            public Word(string _word, int _val)
            {
                this._word = _word;
                this._val = _val;
            }
        }

        class BoggleDictionary
        {
            public List<Word> _dictionary = new();
            public static string[] _tempWords = File.ReadAllLines(Init._wordFile);

            public BoggleDictionary()
            {
                foreach(string _word in _tempWords) 
                {
                    _dictionary.Add(new Word(_word, GetValue(_word)));
                }
            }

            private static int GetValue(string word)
            {
                char[] _chars = word.ToCharArray();
                int _effectiveLength = 1;
                for (int i = 0; i < _chars.Length - 1; i++)
                {
                    if (_chars[i] == 'q')
                    {
                        if (_chars[i+1] != 'u')
                        {
                            _effectiveLength++;
                        }
                    }
                    else
                    {
                        _effectiveLength++;
                    }
                }
                return _effectiveLength switch
                {
                    <= 2 => 0,
                    <= 4 => 1,
                    <= 5 => 2,
                    <= 6 => 3,
                    <= 7 => 4,
                    _ => 11,
                };
            }
        }

        static class Init
        {
            public readonly static string _configFile = "assistconfig.txt";
            public readonly static string _wordFile = "words.txt";
            public static string[] _configArgs;
            public static List<Args> _args = new();
            public static bool Initialised = false;

            static Init()
            {
                try
                {
                    _configArgs = File.ReadAllLines(_configFile);
                    File.Exists(_wordFile);

                    if (_configArgs[0].Split(" ")[0] == "wordfileintegrity=")
                    {
                        if (_configArgs[0].Split(" ")[1] == "1")
                        {
                            _args.Add(Args.wordintegrity);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        throw new Exception();
                    }

                    Initialised = true;
                }
                catch { FirstBoot(); }
            }

            private static void FirstBoot()
            {
                Thread _loadingTextThread = new(() => Utils.df_LoadingText("Starting first boot sequence", true, 100));
                _loadingTextThread.Start();

                WebClient _client = new WebClient();
                byte[] _rawWords = _client.DownloadData("https://raw.githubusercontent.com/PrestonDJ/boggleWordsTxt/main/Words");
                _client.Dispose();


                string convertedWords = System.Text.Encoding.UTF8.GetString(_rawWords);
                string[] _finalWords = convertedWords.Split("\n");

                File.WriteAllLines(_wordFile, _finalWords);

                string[] _defaultargs = new string[]
                {
                    "wordfileintegrity= 1",
                };

                File.WriteAllLines(_configFile, _defaultargs);
                _configArgs = _defaultargs;


                Utils._disposeFlag = true;
                Initialised = true;
            }
        }

        static class ExceptionHandler
        {
            public static Exception _exception;

            static ExceptionHandler()
            {
                Thread ExceptionThread = new Thread(new ThreadStart(Handler));
                ExceptionThread.Start();
            }

            private static void Handler()
            {
                while (true)
                {
                    if(_exception != null)
                    {
                        Console.WriteLine(_exception.Message);
                        Environment.Exit(1);
                    }
                }
            }
        }

        static class Utils
        {
            public static bool _disposeFlag = false;

            public static void df_LoadingText(string text, bool bootmode, int speedMs)
            {
                while (!_disposeFlag)
                {
                    Console.WriteLine(text + ".");
                    Thread.Sleep(speedMs);
                    Console.Clear();
                    Console.WriteLine(text + "..");
                    Thread.Sleep(speedMs);
                    Console.Clear();
                    Console.WriteLine(text + "...");
                    Thread.Sleep(speedMs);
                    Console.Clear();
                }
                if (bootmode) { _disposeFlag = true; }
                EnterContinue("First boot process successfully initiated.", true);
            }
            public static void EnterContinue(string text, bool clear)
            {
                if (clear) { Console.Clear(); }
                if (text != "") { Console.WriteLine(text); }
                Console.WriteLine("Press enter to continue.");
                Console.ReadLine();
                Console.Clear();
            }
            public static void MenuOptions(bool admin)
            {

                Console.WriteLine("1 - Check Word");
                if(admin)
                {
                    Console.WriteLine("2 - Add Word to Dictionary");
                }
                Console.WriteLine("0 - Exit Program");
            }
        }

        public enum Args
        {
            wordintegrity,
        }
    }
}