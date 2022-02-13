using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeathRollingAutoPrio.Models.TMB
{
    [DebuggerDisplay("{Name} - {Spec}")]
    public class Character
    {
        // note: Json property attributes exist on members that otherwise wouldn't translate do to underscore spacing.

        public int Id { get; set; }

        [JsonProperty("member_id")]
        public int? MemberId { get; set; }

        [JsonProperty("guild_id")]
        public int? GuildId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public int Level { get; set; }

        public string Race { get; set; }

        public string Class { get; set; }

        public string Archetype { get; set; }

        public string Spec { get; set; }

        [JsonProperty("raid_group_id")]
        public int? RaidGroupId { get; set; }

        public string Username { get; set; }

        public string DiscordUsername { get; set; }

        public string DiscordId { get; set; }

        [JsonProperty("raid_count")]
        public int RaidCount { get; set; }

        [JsonProperty("attendance_percentage")]
        public double AttendancePercentage { get; set; }

        public List<Item> Received { get; set; }

        public List<ListedItem> Prios { get; set; }

        public List<ListedItem> Wishlist { get; set; }

    }
}
