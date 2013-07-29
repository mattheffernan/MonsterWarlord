namespace Data.Models
{
    public class Monster
    {
        public string Name { get; set; }
        public UniqueType Uniqueness { get; set; }
        public decimal Price { get; set; }
        public decimal Upkeep { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
    }

    public enum UniqueType
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legend,
        Ancient,
        Chaos,
        Ultimate,
        God,
        SGod,
        SSGod,
        UltraGod,
        Supreme,
        Elder
    }

    public class UniquePresidence
    {
        public UniqueType Uniqueness { get; set; }
        public int Tier { get; set; }
    }
}