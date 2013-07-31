using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using SharpRepository.Repository;

namespace Domain.Services
{
    public class MonsterService : IMonsterService
    {
        private readonly IRepository<Monster> _monsterRepository;

        public MonsterService(IRepository<Monster> monsterRepository)
        {
            _monsterRepository = monsterRepository;
        }

        public SaveResult SaveMonster(Monster dirtyMonster)
        {
            var monster = UpdateMonster(dirtyMonster);

            var result = new SaveResult();
            try
            {
                if (monster.MonsterId == 0)
                    _monsterRepository.Add(monster);
                else
                    _monsterRepository.Update(monster);

                result.Successful = true;
            }
            catch (Exception ex)
            {
                result.Error = ex;
                result.Successful = false;
            }

            return result;
        }

        private Monster UpdateMonster(Monster dirtyMonster)
        {
            var monster = _monsterRepository.Find(p => p.MonsterId == dirtyMonster.MonsterId) ?? new Monster {MonsterId = 0};
            monster.Attack = dirtyMonster.Attack;
            monster.Defence = dirtyMonster.Defence;
            monster.Name = dirtyMonster.Name;
            monster.Price = dirtyMonster.Price;
            monster.Upkeep = dirtyMonster.Upkeep;
            monster.Element = dirtyMonster.Element;
            monster.MonsterLevel = dirtyMonster.MonsterLevel;
            return monster;
        }

        public SaveResult SaveMonsters(IEnumerable<Monster> dirtyMonsters)
        {
            var result = new SaveResult();
            try
            {
                using (var batch = _monsterRepository.BeginBatch())
                {
                    foreach (var dirtyMonster in dirtyMonsters.Select(UpdateMonster))
                    {
                        if (dirtyMonster.MonsterId == 0)
                            batch.Add(dirtyMonster);
                        else
                            batch.Update(dirtyMonster);
                    }
                    batch.Commit();
                    result.Successful = true;
                }
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Error = ex;
            }

            return result;
        }

        public IEnumerable<Monster> GetMonsters()
        {
            return _monsterRepository.GetAll();
        }

        public IEnumerable<Monster> GetMonsters(int tier)
        {
            return _monsterRepository.FindAll(p => p.MonsterLevel.Tier == tier);
        }

        public IEnumerable<Monster> GetMonsters(Element element)
        {
            return _monsterRepository.FindAll(p => p.Element == element);
        }
    }
}