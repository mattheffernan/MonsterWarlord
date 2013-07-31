using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Services
{
    public interface IMonsterService
    {
        SaveResult SaveMonster(Monster dirtyMonster);
        SaveResult SaveMonsters(IEnumerable<Monster> dirtyMonsters);
        IEnumerable<Monster> GetMonsters();
        IEnumerable<Monster> GetMonsters(int tier);
        IEnumerable<Monster> GetMonsters(Element element);
    }
}