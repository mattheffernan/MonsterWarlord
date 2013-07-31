using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain.Entities;
using LINQtoCSV;

namespace Domain.Services
{
    public interface IMonsterImportService
    {
        void Import(string file);
    }

    public class MonsterCsvImportService : IMonsterImportService
    {
        private readonly IMonsterLevelService _monsterLevelService;
        private readonly IMonsterService _monsterService;

        public MonsterCsvImportService(IMonsterLevelService monsterLevelService, IMonsterService monsterService)
        {
            _monsterLevelService = monsterLevelService;
            _monsterService = monsterService;
        }

        public void Import(string stream)
        {
            var inputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true
            };
            var cc = new CsvContext();
            var monsterImports = cc.Read<MonsterImport>(stream, inputFileDescription);

            var monsterLevels = _monsterLevelService.GetMonsterLevel().ToDictionary(k => k.Name, v => v);
            var monsters = _monsterService.GetMonsters().ToDictionary(k => k.Name, v => v);

            var monsterList = new List<Monster>();
            foreach (var monsterImport in monsterImports)
            {
                Monster monster = null;
                var name = monsterImport.Unit;
                if (monsters.ContainsKey(name))
                {
                    monster = monsters[name];
                }
                else
                {
                    monster = new Monster();
                    monster.Name = name;
                }

                monster.Attack = monsterImport.Attack;
                monster.Defence = monsterImport.Defense;
                monster.Price = monsterImport.Price;
                monster.Upkeep = monsterImport.Upkeep;
                monster.Element = EnumUtil.ParseEnum<Element>(monsterImport.Element);
                MonsterLevel monsterLevel;
                if (monsterLevels.ContainsKey(monsterImport.Uniqueness))
                {
                    monsterLevel = monsterLevels[monsterImport.Uniqueness];
                }
                else
                {
                    monsterLevel = new MonsterLevel
                        {
                            Active = true,
                            Name = monsterImport.Uniqueness,
                            Tier = monsterLevels.Count() + 1
                        };
                    monsterLevels.Add(monsterImport.Uniqueness, monsterLevel);
                }
                monster.MonsterLevel = monsterLevel;
                monsterList.Add(monster);
            }
            _monsterService.SaveMonsters(monsterList);
        }


        private class MonsterImport
        {
            [CsvColumn(FieldIndex = 1)]
            public string Unit { get; set; }
            [CsvColumn(FieldIndex = 2)]
            public string Element { get; set; }
            [CsvColumn(FieldIndex = 3)]
            public string Uniqueness { get; set; }
            [CsvColumn(FieldIndex = 4)]
            public int Price { get; set; }
            [CsvColumn(FieldIndex = 5)]
            public int Upkeep { get; set; }
            [CsvColumn(FieldIndex = 6)]
            public int Attack { get; set; }
            [CsvColumn(FieldIndex = 7)]
            public int Defense { get; set; }
        }
    }
}