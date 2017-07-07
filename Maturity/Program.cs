using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Microsoft.VisualBasic.FileIO; // imported assembly for use of TextFieldParser class

namespace Maturity
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            const int colNumber = 6;                // Number of columns in the perceived CSV table
            
            List<string> policyNumbers = new List<string>();
            List<decimal> maturityValues = new List<decimal>();
            string selectedPath = "";

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Contents of selected folder. Please select .csv file to read!");
                foreach (var path in Directory.GetFiles(selectedPath=fbd.SelectedPath))
                {
                    //Console.WriteLine(path); // full path
                    Console.WriteLine(System.IO.Path.GetFileName(path)); // file name
                }
                string[] csvFiles = System.IO.Directory.GetFiles(fbd.SelectedPath, "*.csv");
                Console.WriteLine("");
                foreach (var csvFile in csvFiles)
                {
                    Console.WriteLine(csvFile);
                }
                Console.WriteLine("");
                Console.WriteLine("Please type in the name of the .csv file.");
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
                // Map CSV data to model, here. If files arrived with or without headings, extra logic would be used. Omitted for now
                var policyList = new List<Models.Policy>();
                /* the Policy List is going to contain as many policies as there are lines of data in the CSV file. This number
                   is the total number of data items in the file, divided by the number of columns minus 1 for the headers line */
                var rows = csvList.Count()/colNumber - 1;   // .. in our case 60 items divided by 6 (10) minus the header line, that is 9 rows
                var rowCounter = 0;
                var itemCounter = 0;
                var rowArray = new ArrayList();

                foreach (var item in csvList)
                {
                    if (rowCounter > 0)         // skip header row
                    {
                        rowArray.Add(item);   // Store data item in the array. After having read a row, this array will look like this:
                                                        // { A100001,01/06/1986,10000,Y,1000,40 }
                    }
                    itemCounter++;
                    if (rowCounter > 0 && itemCounter == colNumber)     // we've just filled the array with a row of data
                    {
                        var policyNumber = (string) rowArray[0];
                        string myString = rowArray[1].ToString();
                        DateTime policyStartDate = DateTime.ParseExact(myString, "dd/MM/yyyy", null).Date;
                        var premiums = Convert.ToInt32(rowArray[2]);
                        var membership = (rowArray[3].ToString() == "Y");
                        var discretionaryBonus = Convert.ToInt32(rowArray[4]);
                        var upliftPerecentage = Convert.ToDecimal(rowArray[5]);

                        Models.Policy policy = new Models.Policy()
                        {
                            PolicyNumber = policyNumber,
                            PolicyStartDate = policyStartDate,
                            Premiums = premiums,
                            Membership = membership,
                            DiscretionaryBonus = discretionaryBonus,
                            UpliftPerecentage = upliftPerecentage
                        };
                        policyList.Add(policy);
                        rowArray.Clear();                   // empty the array to prepare it for next use with a new row
                    }
                    if (itemCounter == colNumber) // when itemCounter gets to (e.g.) 5, 6 items would have been read, or, one row
                    {
                        itemCounter = 0;                // reset the item counter
                        rowCounter++;                   // increase the row counter
                    }
                }

                // Display the policy list, partly as an indication that the CSV file process is done....
                Console.WriteLine("Policy  Start Date Premiums Memb. Bonus Uplifting");
                foreach (var item in policyList)
                {
                    Console.WriteLine(item.PolicyNumber + " " + item.PolicyStartDate.ToString("d") + " " + item.Premiums +
                                      "    " + item.Membership + ((item.Membership) ? "   " : "  ") +
                                      item.DiscretionaryBonus +
                                      " " + item.UpliftPerecentage);
                    // ... and then calculate the value for each of them
                    var value = 0m;
                    // First find out what Policy Type we have, by dissecting the Policy Number
                    switch (item.PolicyNumber[0])
                    {
                        case (char)Models.PolicyType.A:
                            // its an A type. Calculate as such
                            value = CalculatePolicyValue(item, Models.PolicyType.A);
                            break;
                        case (char)Models.PolicyType.B:
                            // its a B type
                            value = CalculatePolicyValue(item, Models.PolicyType.B);
                            break;
                        case (char)Models.PolicyType.C:
                            // its a C type
                            value = CalculatePolicyValue(item, Models.PolicyType.C);
                            break;
                    }
                    policyNumbers.Add(item.PolicyNumber);
                    maturityValues.Add(value);
                    
                }
                Console.WriteLine("Hit Enter to generate XML file!");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("You have not entered a filename, or file does not exist. Press any key to exit");
                Console.ReadLine();
            }
            if (policyNumbers.Count > 0 && maturityValues.Count > 0)
            {
                var xmlFilenamePath = Path.Combine(selectedPath, "MaturityValues.xml");
                WriteToXml(policyNumbers, maturityValues, xmlFilenamePath);
            }

        }

        /// <summary>
        /// Creates an XML file containing policy numbers with their maturity values
        /// </summary>
        /// <param name="policyNumbers">List of policy numbers</param>
        /// <param name="maturityValues">List of respective maturity values to the above policies</param>
        /// <param name="filename">Full path and name of generated XML filename</param>
        private static void WriteToXml(List<string> policyNumbers, List<decimal> maturityValues, string filename)
        {
            var counter = policyNumbers.Count;
            using (XmlWriter writer = XmlWriter.Create(filename))
            {
                writer.WriteStartElement("maturityValuesList");
                for (var i = 0; i < counter; i++)
                {
                    writer.WriteStartElement("policy");
                    writer.WriteElementString("policyNumber", policyNumbers[i]);
                    writer.WriteElementString("maturityValue", maturityValues[i].ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        /// <summary>
        /// Calculates policy value based on its type
        /// </summary>
        /// <param name="policy">The policy</param>
        /// <param name="type">The policy type</param>
        private static decimal CalculatePolicyValue(Models.Policy policy, Models.PolicyType type)
        {
            var managementFee = 0m;
            var uplift = 0m;
            var bonus = 0m;
            switch (type)
            {
                case Models.PolicyType.A:
                    managementFee = (decimal)Models.ManagementFee.A / 100;
                    uplift = (decimal) policy.UpliftPerecentage / 100 + 1;
                    if (policy.PolicyStartDate < new DateTime(1990, 01, 01))
                    {
                        bonus = policy.DiscretionaryBonus;
                    }
                    return Math.Round(((policy.Premiums - (policy.Premiums * managementFee) + bonus) * uplift),2);
                    break;
                case Models.PolicyType.B:
                    managementFee = (decimal)Models.ManagementFee.B / 100;
                    uplift = (decimal)policy.UpliftPerecentage / 100 + 1;
                    if (policy.Membership)
                    {
                        bonus = policy.DiscretionaryBonus;
                    }
                    return Math.Round(((policy.Premiums - (policy.Premiums * managementFee) + bonus) * uplift), 2);
                    break;
                case Models.PolicyType.C:
                    managementFee = (decimal)Models.ManagementFee.C / 100;
                    uplift = (decimal)policy.UpliftPerecentage / 100 + 1;
                    if ((policy.PolicyStartDate >= new DateTime(1990, 01, 01)) && (policy.Membership))
                    {
                        bonus = policy.DiscretionaryBonus;
                    }
                    return Math.Round(((policy.Premiums - (policy.Premiums * managementFee) + bonus) * uplift), 2);
                    break;
                default:
                    return 0m;
                    break;
            }
        }

        /// <summary>
        /// Breaks up a CSV file into its constituents for processing
        /// </summary>
        /// <param name="pathAndFlename">the full address and filename of the file</param>
        /// <returns>An array of all the CSV file components</returns>
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
