﻿namespace LegendsViewer.Backend.Legends.Various;

public class Population
{
    private readonly World _world;

    public bool IsMainRace => _world.MainRaces.ContainsKey(Race);

    public bool IsOutcasts => Race.NamePlural.Contains("Outcasts");

    public bool IsPrisoners => Race.NamePlural.Contains("Prisoners");

    public bool IsSlaves => Race.NamePlural.Contains("Slaves");

    public bool IsVisitors => Race.NamePlural.Contains("Visitors");

    public bool IsAnimalPeople => Race.NamePlural.Contains(" Men") && !IsSlaves && !IsPrisoners && !IsOutcasts && !IsVisitors;

    public CreatureInfo Race { get; set; }
    public int Count { get; set; }

    public Population(World world, CreatureInfo type, int count)
    {
        _world = world;
        Race = type;
        Count = count;
    }
}
