using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public enum Element
    {
        Fire,
        Water,
        Air,
        Earth,
        Dark,
        Holy
    }

    public class Monster : Entity
    {
        [Key]
        public virtual int MonsterId { get; set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual Element Element { get; set; }
        [Required]
        public virtual MonsterLevel MonsterLevel { get; set; }
        public virtual int Price { get; set; }
        public virtual int? Upkeep { get; set; }
        [Required]
        public virtual int Attack { get; set; }
        [Required]
        public virtual int Defence { get; set; }
    }

    public class MonsterLevel : Entity
    {
        [Key]
        public virtual int MonsterLevelId { get; set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual int Tier { get; set; }
        public virtual bool Active { get; set; }
        public virtual ICollection<Monster> Monsters { get; set; }  
    }
}