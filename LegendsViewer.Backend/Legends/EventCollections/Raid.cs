﻿using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.EventCollections;

public class Raid : EventCollection
{
    public int Ordinal { get; set; } = -1;
    public Location? Coordinates { get; set; }
    [JsonIgnore]
    public Entity? Attacker { get; set; }
    [JsonIgnore]
    public Entity? Defender { get; set; }
    public int ItemsStolenCount => GetSubEvents().OfType<ItemStolen>().Count();
    [JsonIgnore]
    public List<HistoricalFigure> Deaths => GetSubEvents().OfType<HfDied>().Select(death => death.HistoricalFigure).ToList();
    public int DeathCount => Deaths.Count;
    [JsonIgnore]
    public EventCollection? ParentEventCol { get; set; }

    public Raid(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "ordinal": Ordinal = Convert.ToInt32(property.Value); break;
                case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                case "parent_eventcol": ParentEventCol = world.GetEventCollection(Convert.ToInt32(property.Value)); break;
                case "attacking_enid": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "defending_enid": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
            }
        }
        Attacker?.AddEventCollection(this);
        Defender?.AddEventCollection(this);

        Name = $"{Formatting.AddOrdinal(Ordinal)} raid";

        Icon = HtmlStyleUtil.GetIconString("lightning-bolt");
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = GetTitle();
            string linkedString = "the ";
            linkedString += pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "raid", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));

            if (Site != null && pov != Site)
            {
                linkedString += $" in {Site.ToLink(true, this)}";
            }
            return linkedString;
        }
        return ToString();
    }

    private string GetTitle()
    {
        string title = Type;
        title += "&#13";
        title += "Items Stolen: " + ItemsStolenCount;
        title += "&#13";
        title += "Site: ";
        title += Site != null ? Site.ToLink(false) : "UNKNOWN";
        return title;
    }

    public override string ToString()
    {
        return $"the {Name} in {Site}";
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
