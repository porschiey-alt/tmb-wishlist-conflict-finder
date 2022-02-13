using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeathRollingAutoPrio.Models.TMB
{
    [DebuggerDisplay("{Name}")]
    public class Item
    {
        public int Id { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("parent_id")]
        public int? ParentId { get; set; }

        [JsonProperty("parent_item_id")]
        public int? ParentItemId { get; set; }

        public string Name { get; set; }

        public double? Weight { get; set; }

        [JsonProperty("quality")]
        public int QualityInt { get; set; }

        [JsonIgnore]
        public ItemQuality Quality => (ItemQuality)this.QualityInt;

        [JsonProperty("added_by_username")]
        public string AddedByUsername { get; set; }

        public ItemPivot Pivot { get; set; }

        [JsonIgnore]
        public SortedDictionary<string, int> WishedFor { get; set; }

        [JsonIgnore]
        public SortedDictionary<string, int> Priority { get; set; }
    }

    public class ListedItem : Item
    {

        /// <summary>
        /// Either the prio number or wishlist prio number.
        /// </summary>
        [JsonProperty("list_number")]
        public int ListNumber { get; set; }
    }

    public class ItemPivot
    {
        [JsonProperty("character_id")]
        public int CharacterId { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        public int Id { get; set; }

        [JsonProperty("added_by")]
        public int AddedByMemberId { get; set; }

        public string Type { get; set; }

        public int Order { get; set; }

        public string Note { get; set; }

        public DateTime ReceivedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public enum ItemQuality
    {
        Trash,
        Common, 
        UnCommon,
        Rare,
        Epic,
        Legendary
    }
}
