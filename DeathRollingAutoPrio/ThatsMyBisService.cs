using DeathRollingAutoPrio.Models.TMB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeathRollingAutoPrio
{
    /// <summary>
    /// In memory service that acts as a database after parsing a Json Payload dump from ThatsMyBis.com.
    /// </summary>
    public class ThatsMyBisService
    {

        public static string DefaultJsonDumpFileName = "character-json.json";

        /// <summary>
        /// A queryable set of characters in the guild. <see cref="Character"/>
        /// </summary>
        public List<Character> Characters { get; set; }

        /// <summary>
        /// A queryable set of items that the guild has listed in any way (prio, wishlist, or otherwise).
        /// </summary>
        public List<ListedItem> ItemsAccountedFor { get; set; }

        /// <summary>
        /// A queryable set of items that players have wished for.
        /// </summary>
        public List<ListedItem> WishedForItems => this.ItemsAccountedFor.Where(i => i.WishedFor.Any()).ToList();

        /// <summary>
        /// A queryable set of items that have been prio'd to players.
        /// </summary>
        public List<ListedItem> PrioritizedItems => this.ItemsAccountedFor.Where(i => i.Priority.Any()).ToList();

        /// <summary>
        /// Runs an attendance measure against the in-memory database and spits a list of <see cref="AttendanceRecord"/>s in order of best score to worst.
        /// </summary>
        public List<AttendanceRecord> ScoreAttendance()
        {
            Console.Write("Do you want to factor in last 4 weeks attendance ONLY? (Y/N)?:");
            var input = Console.ReadLine().Trim().ToLowerInvariant();
            var score4w = (input == "y" || input == "yes");
            Console.WriteLine();
            var records = new List<AttendanceRecord>();
            foreach (var ch in this.Characters)
            {
                var record = new AttendanceRecord
                {
                    CharacterId = ch.Id,
                    Name = ch.Name,
                    Attendance = ch.AttendancePercentage,
                    PossibleRaids = ch.RaidCount,
                    LastFourWeeksAttendance = 0
                };

                var percentMod = ch.AttendancePercentage < .5 ? 2 : 1;
                //var percentMod = 1;
                record.BasicScore = ch.RaidCount * (ch.AttendancePercentage / percentMod);

                if (score4w)
                {
                    Console.Write($"What is {ch.Name}'s 4wk attendance rate? (type 'skip' to stop checking): ");
                    Console.WriteLine();
                    var attendanceRateIn = Console.ReadLine().Trim().ToLowerInvariant();
                    if (attendanceRateIn == "skip")
                    {
                        score4w = false;
                        continue;
                    }

                    if (!double.TryParse(attendanceRateIn, out double attPercent))
                    {
                        Console.WriteLine("Bad input, skipping.");
                    }

                    var T = ch.RaidCount;
                    record.AdvancedScore = T - ((T - T * (attPercent)) / 2);
                }
                records.Add(record);
            }
            return records;
        }

        /// <summary>
        /// Primary ingress method for that parses and populates the in-memory database.
        /// </summary>
        /// <returns>Returns the root list of characters parsed.</returns>
        public List<Character> ParseJsonPayload(string pathToJsonFile)
        {

            Console.WriteLine($"SCANNING WISHLISTS, STANDBY...\n");

            var json = File.ReadAllText(pathToJsonFile);
            this.Characters = JsonConvert.DeserializeObject<List<Character>>(json);

            var wishListedItemDictionary = new Dictionary<int, ListedItem>();

            foreach (var c in this.Characters)
            {
                foreach (var item in c.Wishlist)
                {
                    if (!wishListedItemDictionary.ContainsKey(item.ItemId))
                    {
                        item.WishedFor = new SortedDictionary<string, int>();
                        item.Priority = new SortedDictionary<string, int>();
                        wishListedItemDictionary.Add(item.ItemId, item);
                    }

                    var dictionItem = wishListedItemDictionary[item.ItemId];
                    var cKey = $"{c.Id}-0";
                    if (dictionItem.WishedFor.ContainsKey($"{c.Id}-0"))
                    {
                        cKey = $"{c.Id}-1";
                        Console.WriteLine($"Warning: {c.Name} added the item {item.Name} twice. Some items like rings are not unique, so this might be normal. Please double check to see if this is acceptable.");
                    }
                    

                    
                    if (dictionItem.WishedFor.ContainsKey(cKey))
                    {
                        // user added the same item a third time... 
                        Console.WriteLine($"Error: {c.Name} added the item {item.Name} MORE than twice. All additions after 2 will be ignored.");
                        continue;
                    }
                    dictionItem.WishedFor.Add(cKey, item.Pivot.Order);
                }

                foreach (var item in c.Prios)
                {
                    if (!wishListedItemDictionary.ContainsKey(item.ItemId))
                    {
                        item.WishedFor = new SortedDictionary<string, int>();
                        item.Priority = new SortedDictionary<string, int>();
                        wishListedItemDictionary.Add(item.ItemId, item);
                    }
                    var dictionItem = wishListedItemDictionary[item.ItemId];
                    var cKey = dictionItem.WishedFor.ContainsKey($"{c.Id}-0") ? $"{c.Id}-1" : $"{c.Id}-0";
                    dictionItem.Priority.Add(cKey, item.Pivot.Order);
                }
            }

            this.ItemsAccountedFor = wishListedItemDictionary.Values.ToList();
            return this.Characters;
        }

        /// <summary>
        /// Fetches a character by hyphenated id. Typically used for items with dupe entires, like those pesky rings.
        /// </summary>
        public Character GetCharacter(string idHyphenated)
        {
            var idStr = idHyphenated.Replace("-0", string.Empty).Replace("-1", string.Empty);
            var id = Convert.ToInt32(idStr);
            return this.GetCharacter(id);
        }

        /// <summary>
        /// Fetches a character by normal id.
        /// </summary>
        public Character GetCharacter(int id)
        {
            return this.Characters.FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Quick query method that scans the items wished for and finds any that are in conflict for the same wishlist slot.
        /// </summary>
        public List<ListedItem> FindItemsInConflict()
        {
            var itemsThatNeedResolution = new List<ListedItem>();
            foreach (var item in this.WishedForItems)
            {
                var hasConflicts = item.WishedFor.GroupBy(kvp => kvp.Value).Where(group => group.Count() > 1).Any();
                if (hasConflicts)
                {
                    itemsThatNeedResolution.Add(item);
                }
            }

            return itemsThatNeedResolution;
        }

        /// <summary>
        /// Quick query method that scans the items wished for and finds any that are in multiple wishlists.
        /// </summary>
        public List<ListedItem> FindAlmostContestedItems()
        {
            var result = new List<ListedItem>();
            foreach (var item in this.WishedForItems)
            {
                if (item.WishedFor.Count > 1)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        [Obsolete]
        // old formula for attendance.
        private double CalculateCharacterAttendancePoints(Character c)
        {
            var T = c.RaidCount;
            var attPercent = c.AttendancePercentage;

            return T - ((T - T * (attPercent)) / 2);
        }
    }
}
