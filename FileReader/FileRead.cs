using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace FileReader
{
    class FileRead
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    throw new Exception("No filename specified.");
                }

                string filename = args[0];
                string extension = String.Empty;

                if (!filename.Contains("."))
                {
                    throw new Exception("Enter valid file.  Valid files: .csv, .json or .txt");
                }
                extension = filename.ToLower().Split('.').LastOrDefault();
                
                IFileProcessor _fileprocessor;
                IDestination _destination;

                string destination = string.Empty;
                if (args[1] != null)
                {
                    destination = args[1];
                }
                switch (extension)
                {
                    case "json":
                        _fileprocessor = new JsonFileInput();
                        break;
                    case "txt":
                        _fileprocessor = new TextFileInput();
                        break;
                    case "csv":
                        _fileprocessor = new CSVFileInput();
                        break;
                    default:
                        throw new Exception("Enter valid file.  Valid files: csv, json or txt.");
                }
                
                FileHandler _fileHandler = new FileHandler(_fileprocessor);
                Person _person = _fileHandler.ProcessInputFile(filename);

                switch (destination)
                {
                    case "/d":
                        _destination = new WriteToDB();
                        break;
                    case "/e":
                        _destination = new SendEmail();
                        break;
                    default:
                        _destination = new ConsolePrintout();
                        break;
                }
                DestinationHandler _destinationHandler = new DestinationHandler(_destination);
                _destinationHandler.RedirectToDestination(_person);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public interface IFileProcessor
    {
        Person Process(string filename);
    }   
    public class CSVFileInput : IFileProcessor
    {
        const string firstname = "FirstName:";
        const string lastname = "LastName:";
        public Person Process(string filename)
        {
            Person _person = new Person();
            string[] arr;
            var lines = File.ReadLines(filename);
            foreach (string line in lines)
            {
                arr= line.Split(',');
                if (arr.Length < 2)
                { continue;  }

                if (arr[0].Contains(firstname))
                {
                    _person.FirstName = arr[0].Replace(firstname, "").Trim();
                }
                if (arr[1].Contains(lastname))
                {
                    _person.LastName = arr[1].Replace(lastname, "").Trim();
                }
            }
            return _person;
        }
    }
    public class TextFileInput : IFileProcessor
    {
        const string firstname = "FirstName:";
        const string lastname = "LastName:";

        public Person Process(string filename)
        {
            Person _person = new Person();
            var lines = File.ReadLines(filename);
            foreach (string line in lines)
            {
                if (line.Contains(firstname))
                {
                    _person.FirstName = line.Replace(firstname, "").Trim();
                }
                if (line.Contains(lastname))
                {
                    _person.LastName = line.Replace(lastname, "").Trim();
                }
            }
            return _person;
        }
    }
    public class JsonFileInput : IFileProcessor
    {
        public Person Process(string filename)
        {           
            string json = File.ReadAllText(filename);
            Person _person = JsonConvert.DeserializeObject<Person>(json);

            return _person;
        }
    }
    public class FileHandler
    {
        private readonly IFileProcessor _inputfile;

        public FileHandler(IFileProcessor inputfile)
        {
            this._inputfile = inputfile;
        }
        public Person ProcessInputFile(string filename)
        {
            
           return this._inputfile.Process(filename);
            
        }
    }

    public interface IDestination
    {
        void Redirect(Person _person);
    }
    public class ConsolePrintout : IDestination
    {     
        public void Redirect(Person _person)
        {
            Console.WriteLine(_person.FirstName);
            Console.WriteLine(_person.LastName);
            Console.ReadLine();
        }
    }
    public class WriteToDB : IDestination
    {
        public void Redirect(Person _person)
        {
            Console.WriteLine("Inserting into DB");
            Console.WriteLine("INSERT INTO dbo.Person(FirstName, LastName) SELECT '" + _person.FirstName.Trim() + "','" + _person.LastName.Trim() + "'");
            Console.ReadLine();
        }
    }
    public class SendEmail : IDestination
    {
        public void Redirect(Person _person)
        {
            Console.WriteLine("Sending Email to " + _person.FirstName + " " + _person.LastName);
            Console.ReadLine();
        }
    }
    public class DestinationHandler
    {
        private readonly IDestination _destination;

        public DestinationHandler(IDestination destination)
        {
            this._destination = destination;
        }
        public void RedirectToDestination(Person _person)
        {
            this._destination.Redirect(_person);

        }
    }
}
