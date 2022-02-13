using DeathRollingAutoPrio.Models.TMB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeathRollingAutoPrio
{
    class Program
    {

        static int Main(string[] args)
        {

#if DEBUG
            args = new string[] { @"D:\Downloads\character-json.json" };
#endif

            Console.WriteLine("Death Rolling Loot Tool, by Refuge (Brandon) - v1.1");

            #region File Ingress and Validation
            // validate path
            string path = null;
            if (args.Length < 1)
            {
                // no specified path, attempt to look in operating directory
                var potentialPath = Path.Combine(Environment.CurrentDirectory, ThatsMyBisService.DefaultJsonDumpFileName);
                if (File.Exists(potentialPath))
                {
                    path = potentialPath;
                }
                else
                {
                    Console.WriteLine($"Error: No path was specified to json export from TMB and no expected file was found near this .exe. Place the '{ThatsMyBisService.DefaultJsonDumpFileName}' file in the same directory as this exe or specify the path to the file when running. Ex) DeathRollingAutoPrio.exe \"C:\\path\\to\\file.json\"");
                    return 1;
                }
            }
            else
            {
                path = args[0].Trim();
            }

            // validate file, generally
            if (!File.Exists(path) || Path.GetExtension(path) != ".json")
            {
                Console.WriteLine("Error: The path specified is invalid or the file provided is not a Json file.");
                return 2;
            }

            var tmbService = new ThatsMyBisService();
            List<Character> characters = null;
            try
            {
                characters = tmbService.ParseJsonPayload(path);

                // validate contents of file
                if (!characters.Any() || string.IsNullOrEmpty(characters.First().Slug))
                {
                    Console.WriteLine("Error: The json file provided could not be parsed with plausible accuracy.");
                    return 3;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Unable to parse the json file. An exception occured that could not be ignored. Exception details below: {ex}");
                return 4;
            }

            #endregion

            #region Scan Results Output

            Console.WriteLine($"\n\nSCAN COMPLETE: {characters.Count} characters found and {tmbService.ItemsAccountedFor.Count} wish listed items.");

            Console.WriteLine($"\nATTENDANCE:");

            var records = tmbService.ScoreAttendance().OrderByDescending(r => r.BasicScore);
            Console.WriteLine("SCORE | \tPLAYER");
            Console.WriteLine("======================================");
            foreach (var record in records)
            {
                Console.WriteLine($"{string.Format("{0:00.00}", Math.Round(record.BasicScore, 2))} | \t{record.Name} | ({record.PossibleRaids}r, {Math.Round(record.Attendance * 100, 2)}%)");
            }

            Console.WriteLine($"Note: If you joined the guild in ph2 and attended every possible raid but still see <100% attendance, it is because the first 4 raids only count as 50%. (See Attendance Nuances in Doc for details).");


            var contestedItems = tmbService.FindItemsInConflict();
            Console.WriteLine($"\nCONTESTED ITEMS ({contestedItems.Count}):\n=========================================");
            foreach (var item in contestedItems)
            {
                var competitors = item.WishedFor.Select(kvp => tmbService.GetCharacter(kvp.Key)).Select(c => c.Name).Distinct();
                Console.WriteLine($"{item.Name}, id: {item.ItemId}, https://thatsmybis.com/6419/death-rolling/i/{item.ItemId} \nConflict between {string.Join(", ", competitors)}\n");
            }

            var multiBookedItems = tmbService.FindAlmostContestedItems();
            Console.WriteLine($"\nSHARED ITEMS, UNCONTESTED ({multiBookedItems.Count - contestedItems.Count}):\n=========================================");
            foreach (var item in multiBookedItems)
            {
                if (contestedItems.Any(i => i.ItemId == item.ItemId))
                {
                    continue;
                }

                var competitors = item.WishedFor.Select(kvp => tmbService.GetCharacter(kvp.Key)).Select(c => c.Name).Distinct();
                Console.WriteLine($"{item.Name}, id: {item.ItemId}, https://thatsmybis.com/6419/death-rolling/i/{item.ItemId} \n Shared by these players' wishlists: {string.Join(", ", competitors)}\n");
            }
            #endregion

            return 0;
        }
    }
}
