﻿using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class UndergroundRegion : WorldObject, IRegion
{
    public static readonly string Icon = HtmlStyleUtil.GetIconString("tunnel");

    public int? Depth { get; set; }
    public RegionType RegionType { get; set; }

    [JsonIgnore]
    public List<Battle> Battles { get; set; } = [];
    public List<string> BattleLinks => Battles.ConvertAll(x => x.ToLink(true, this));

    public List<Location> Coordinates { get; set; } = []; // legends_plus.xml
    public int SquareTiles => Coordinates.Count;

    public UndergroundRegion(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "depth": Depth = Convert.ToInt32(property.Value); break;
                case "type":
                    switch (property.Value)
                    {
                        case "cavern":
                            RegionType = RegionType.Cavern;
                            break;
                        case "underworld":
                            RegionType = RegionType.Underworld;
                            break;
                        case "magma":
                            RegionType = RegionType.Magma;
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "coords":
                    string[] coordinateStrings = property.Value.Split(new[] { '|' },
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (var coordinateString in coordinateStrings)
                    {
                        string[] xYCoordinates = coordinateString.Split(',');
                        int x = Convert.ToInt32(xYCoordinates[0]);
                        int y = Convert.ToInt32(xYCoordinates[1]);
                        Coordinates.Add(new Location(x, y));
                    }
                    break;
            }
        }
        string name;
        switch (RegionType)
        {
            case RegionType.Cavern:
                name = "the depths of the world";
                break;
            case RegionType.Underworld:
                name = "the underworld";
                break;
            case RegionType.Magma:
                name = "the magma seas";
                break;
            default:
                name = $"an underground region ({RegionType})";
                break;
        }
        Name = name;
        Type = RegionType.GetDescription();
        Subtype = Depth?.ToString() ?? string.Empty;
    }

    public override string ToString() { return RegionType.GetDescription(); }
    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {

        if (link)
        {
            string title = RegionType.GetDescription();
            title += "&#13";
            title += "Events: " + Events.Count;
            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "uregion", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
