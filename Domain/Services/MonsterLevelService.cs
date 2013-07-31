using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using SharpRepository.Repository;

namespace Domain.Services
{
    public class MonsterLevelService : IMonsterLevelService
    {
        private readonly IRepository<MonsterLevel> _uniqueTypeRepository;

        public MonsterLevelService(IRepository<MonsterLevel> uniqueTypeRepository)
        {
            _uniqueTypeRepository = uniqueTypeRepository;
        }

        public SaveResult SaveUniqueType(MonsterLevel dirtyMonsterLevel)
        {
            var monster = UpdateUniqueType(dirtyMonsterLevel);

            var result = new SaveResult();
            try
            {
                if (monster.MonsterLevelId == 0)
                    _uniqueTypeRepository.Add(monster);
                else
                    _uniqueTypeRepository.Update(monster);

                result.Successful = true;
            }
            catch (Exception ex)
            {
                result.Error = ex;
                result.Successful = false;
            }

            return result;
        }

        private MonsterLevel UpdateUniqueType(MonsterLevel dirtyMonster)
        {
            var uniqueType = _uniqueTypeRepository.Find(p => p.MonsterLevelId == dirtyMonster.MonsterLevelId) ?? new MonsterLevel();
            uniqueType.Tier = dirtyMonster.Tier;
            uniqueType.Name = dirtyMonster.Name;
            return uniqueType;
        }

        public SaveResult SaveUniqueTypes(IEnumerable<MonsterLevel> dirtyUniqueTypes)
        {
            var result = new SaveResult();
            try
            {
                using (var batch = _uniqueTypeRepository.BeginBatch())
                {
                    foreach (var uniqueType in dirtyUniqueTypes.Select(UpdateUniqueType))
                    {
                        if (uniqueType.MonsterLevelId == 0)
                            batch.Add(uniqueType);
                        else
                            batch.Update(uniqueType);
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

        public MonsterLevel GetMonsterLevel(int tier)
        {
            return _uniqueTypeRepository.Find(p => p.Tier == tier);
        }

        public MonsterLevel GetMonsterLevel(string name)
        {
            return _uniqueTypeRepository.Find(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<MonsterLevel> GetMonsterLevel()
        {
            return _uniqueTypeRepository.GetAll();
        }

    }
}