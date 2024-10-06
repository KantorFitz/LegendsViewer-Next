﻿using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class WorldConstruction : WorldObject, IHasCoordinates
{
    public WorldConstructionType WorldConstructionType { get; set; } // legends_plus.xml
    public List<Location> Coordinates { get; set; } // legends_plus.xml

    [JsonIgnore]
    public Site? Site1 { get; set; } // legends_plus.xml
    public string? Site1ToLink => Site1?.ToLink(true);

    [JsonIgnore]
    public Site? Site2 { get; set; } // legends_plus.xml
    public string? Site2ToLink => Site2?.ToLink(true);

    [JsonIgnore]
    public List<WorldConstruction> Sections { get; set; } // legends_plus.xml
    public List<string> SectionLinks => Sections.ConvertAll(x => x.ToLink(true, this));

    [JsonIgnore]
    public WorldConstruction? MasterConstruction { get; set; } // legends_plus.xml
    public string? MasterConstructionToLink => MasterConstruction?.ToLink(true);

    public string Icon = HtmlStyleUtil.GetIconString("sign-caution");

    public WorldConstruction(List<Property> properties, World world)
        : base(properties, world)
    {
        Name = "Untitled";
        Coordinates = [];
        Sections = [];
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "type":
                    switch (property.Value)
                    {
                        case "road":
                            WorldConstructionType = WorldConstructionType.Road;
                            Icon = HtmlStyleUtil.GetIconString("road");
                            break;
                        case "bridge":
                            WorldConstructionType = WorldConstructionType.Bridge;
                            Icon = HtmlStyleUtil.GetIconString("bridge");
                            break;
                        case "tunnel":
                            WorldConstructionType = WorldConstructionType.Tunnel;
                            Icon = HtmlStyleUtil.GetIconString("tunnel-outline");
                            break;
                        default:
                            WorldConstructionType = WorldConstructionType.Unknown;
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
        Type = WorldConstructionType.GetDescription();
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = "";
            title += "World Construction";
            title += WorldConstructionType != WorldConstructionType.Unknown ? "" : ", " + WorldConstructionType;
            title += "&#13";
            title += "Events: " + Events.Count;

            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "worldconstruction", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
