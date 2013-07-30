using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class Monster : Entity
    {
        [Key]
        public int MonsterId { get; set; }
        [Required]
        public string Name { get; set; }
        public UniqueType Uniqueness { get; set; }
        public decimal Price { get; set; }
        public decimal? Upkeep { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
    }

    public class UniqueType : Entity
    {
        [Key]
        public int UniqueTypeId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Tier { get; set; }
    }
}