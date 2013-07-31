using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Services
{
    public interface IMonsterLevelService
    {
        SaveResult SaveUniqueType(MonsterLevel dirtyMonsterLevel);
        SaveResult SaveUniqueTypes(IEnumerable<MonsterLevel> dirtyUniqueTypes);
        MonsterLevel GetMonsterLevel(int tier);
        MonsterLevel GetMonsterLevel(string name);
        IEnumerable<MonsterLevel> GetMonsterLevel();
    }
}