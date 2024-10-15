﻿using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Parser;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends;

public abstract class WorldObject : DwarfObject
{
    public int Id { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public virtual string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;

    public int PreviousId { get; set; }
    public int NextId { get; set; }

    [JsonIgnore]
    public List<WorldEvent> Events { get; set; } = [];

    [JsonIgnore]
    public List<EventCollection> EventCollections { get; set; } = [];

    [JsonIgnore]
    public World? World { get; }

    public int EventCount => Events.Count;
    public int EventCollectionCount => EventCollections.Count;

    protected WorldObject()
    {
        Id = -1;
    }

    protected WorldObject(World world) : this()
    {
        World = world;
    }

    protected WorldObject(List<Property> properties, World world) : this(world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "id": Id = Convert.ToInt32(property.Value); break;
            }
        }
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        return "";
    }
}
