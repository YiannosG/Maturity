using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO; // imported assembly for use of TextFieldParser class

namespace Maturity
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Contents of selected folder. Please select .csv file to read!");
                foreach (var path in Directory.GetFiles(fbd.SelectedPath))
                {
                    //Console.WriteLine(path); // full path
                    Console.WriteLine(System.IO.Path.GetFileName(path)); // file name
                }
            }
            else
            {
                Console.WriteLine("Error with folder dialogue window");
                
            }
            string filename = Console.ReadLine();
            var pathAndFlename = Path.Combine(fbd.SelectedPath, filename);
            Console.WriteLine(File.Exists(pathAndFlename) ? "File exists." : "File does not exist.");

            if (File.Exists(pathAndFlename))
            {
                var csvList = HandleCsvFile(pathAndFlename);
                // Map CSV data to model, here

            }
            else
            {
                Console.WriteLine("You have not entered a filename, or file does not exist. Press any key to exit");
                Console.ReadLine();
            }
            
        }

        private static List<string> HandleCsvFile(string pathAndFlename)
        {
            List<string> listA = new List<string>();
            using (TextFieldParser parser = new TextFieldParser(pathAndFlename))
            {
                parser.Delimiters = new string[] { "," };
                while (true)
                {
                    string[] parts = parser.ReadFields();
                    if (parts == null)
                    {
                        break;
                    }
                    listA.AddRange(parts);
                }
            }
            return listA;
        }
    }
}
