﻿using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.EventCollections;

public class Purge : EventCollection
{
    public int Ordinal { get; set; } = -1;
    public string? Adjective { get; set; }
    public Site? Site;

    public List<HistoricalFigure> Deaths => GetSubEvents().OfType<HfDied>().Select(death => death.HistoricalFigure).ToList();
    public int DeathCount => Deaths.Count;

    public Purge(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "ordinal": Ordinal = Convert.ToInt32(property.Value); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "adjective": Adjective = property.Value; break;
            }
        }
        Site?.AddEventCollection(this);

        Site?.Warfare.Add(this);

        Name = $"{Formatting.AddOrdinal(Ordinal)} {(!string.IsNullOrWhiteSpace(Adjective) ? $"{Adjective.ToLower()} " : "")}purge";

        Icon = HtmlStyleUtil.GetIconString("skull-crossbones-outline");
    }


    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = GetTitle();
            string linkedString = "the ";
            linkedString += pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "purge", Id, title, Name)
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
        title += "Site: ";
        title += Site != null ? Site.ToLink(false) : "UNKNOWN";
        return title;
    }

    public override string ToString()
    {
        return $"the {Name} in {Site}";
    }
}
