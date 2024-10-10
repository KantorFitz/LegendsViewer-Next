﻿using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.EventCollections;

public class BeastAttack : EventCollection
{
    public int Ordinal { get; set; } = -1;
    public Location? Coordinates { get; set; }
    public WorldRegion? Region { get; set; }
    public UndergroundRegion? UndergroundRegion { get; set; }
    public Site? Site { get; set; }
    public Entity? Defender { get; set; }

    private HistoricalFigure? _beast;
    public HistoricalFigure? Beast
    {
        get => _beast;
        set
        {
            _beast = value;
            _beast?.AddEventCollection(this);
        }
    }

    public List<HistoricalFigure> Deaths => GetSubEvents().OfType<HfDied>().Select(death => death.HistoricalFigure).ToList();
    public int DeathCount => Deaths.Count;

    // BUG in XML? 
    // ParentCollection was never set prior to DF 0.42.xx and is now often set to an occasion
    // but DF legends mode does not show it.
    // http://www.bay12forums.com/smf/index.php?topic=154617.msg6669851#msg6669851
    public EventCollection? ParentEventCol { get; set; }

    public BeastAttack(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "ordinal": Ordinal = Convert.ToInt32(property.Value); break;
                case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                case "parent_eventcol": ParentEventCol = world.GetEventCollection(Convert.ToInt32(property.Value)); break;
                case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "defending_enid": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
            }
        }

        Site?.BeastAttacks.Add(this);

        //--------Attacking Beast is calculated after parsing event collections in ParseXML()
        //--------So that it can also look at eventsList from duel sub collections to calculate the Beast

        //-------Fill in some missing event details with details from collection
        //-------Filled in after parsing event collections in ParseXML()
        Defender?.AddEventCollection(this);
        Region?.AddEventCollection(this);
        UndergroundRegion?.AddEventCollection(this);
        Site?.AddEventCollection(this);

        Site?.Warfare.Add(this);

        Name = $"{Formatting.AddOrdinal(Ordinal)} rampage";
        Icon = HtmlStyleUtil.GetIconString("chess-knight");
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = GetTitle();
            string linkedString = "the ";
            linkedString += pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "collection", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
            if (Beast != null)
            {
                linkedString += $" of {Beast.ToLink(true, pov)}";
            }

            if (Site != null && pov != Site)
            {
                linkedString += $" in {Site.ToLink(true, pov)}";
            }
            return linkedString;
        }
        return ToString();
    }

    private string GetTitle()
    {
        string title = Type;
        title += "&#13";
        title += "Attacker: ";
        title += Beast != null ? Beast.ToLink(false) : "UNKNOWN";
        title += "&#13";
        title += "Site: ";
        title += Site != null ? Site.ToLink(false) : "UNKNOWN";
        return title;
    }

    public override string ToString()
    {
        return $"the {Name} of {Beast?.Name ?? "an unknown creature"} in {Site}";
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
