﻿using System.Text.RegularExpressions;

namespace GameMarketAPIServer.Utilities
{
    public class Tools
    {

        public static string ReadFromFile(string filePath)
        {
           // string results = "";

            try
            {
                string fileContent = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(fileContent))
                {
                    Console.WriteLine("File empty");
                    return fileContent;
                }
                else
                {
                    return fileContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
        public static List<string> ReadFromSQLFile(string filePath)
        {
            List<string> results = new List<string>();

            try
            {
                string fileContent = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(fileContent))
                {
                    Console.WriteLine("File is empty");
                }

                else
                {
                    results = fileContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToList();
                }
                return results;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File faild to open");
            }
            return results;
        }


        public static bool isSequel(string title1, string title2)
        {
            var regex = new Regex(@"\d+$");
            var match1 = regex.Match(title1);
            var match2 = regex.Match(title2);
            if (title1 == "zombie derby 2" && title2 == "zombie derby")
            {
                Console.WriteLine();
            }
            if (match1.Success && match2.Success)
                return match1.Value != match2.Value;
            else
            {
                return match1.Success != match2.Success;
            }


        }

    }
}
