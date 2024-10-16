using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class RemoveHfSiteLink : WorldEvent
{
    public int StructureId { get; set; }
    public Structure? Structure { get; set; } // TODO
    public Entity? Civ { get; set; }
    public HistoricalFigure? HistoricalFigure { get; set; }
    public Site? Site { get; set; }
    public SiteLinkType LinkType { get; set; }

    public RemoveHfSiteLink(List<Property> properties, World world) : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "structure": StructureId = Convert.ToInt32(property.Value); break;
                case "civ": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "histfig": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "link_type":
                    switch (property.Value)
                    {
                        case "lair": LinkType = SiteLinkType.Lair; break;
                        case "hangout": LinkType = SiteLinkType.Hangout; break;
                        case "home_site_building": LinkType = SiteLinkType.HomeSiteBuilding; break;
                        case "home_site_underground": LinkType = SiteLinkType.HomeSiteUnderground; break;
                        case "home_structure": LinkType = SiteLinkType.HomeStructure; break;
                        case "seat_of_power": LinkType = SiteLinkType.SeatOfPower; break;
                        case "occupation": LinkType = SiteLinkType.Occupation; break;
                        case "home_site_realization_building": LinkType = SiteLinkType.HomeSiteRealizationBuilding; break;
                        case "home_site_abstract_building": LinkType = SiteLinkType.HomeSiteAbstractBuilding; break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "site": if (Site == null) { Site = world.GetSite(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
            }
        }
        if (Site != null)
        {
            Structure = Site.Structures.Find(structure => structure.LocalId == StructureId);
        }
        HistoricalFigure.AddEvent(this);
        Civ.AddEvent(this);
        Site.AddEvent(this);
        Structure.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        if (HistoricalFigure != null)
        {
            eventString += HistoricalFigure.ToLink(link, pov, this);
        }
        else
        {
            eventString += "UNKNOWN HISTORICAL FIGURE";
        }
        eventString += LinkType switch
        {
            SiteLinkType.HomeSiteAbstractBuilding or SiteLinkType.HomeSiteRealizationBuilding => " moved out of ",
            SiteLinkType.Hangout => " stopped ruling from ",
            SiteLinkType.SeatOfPower => " stopped working from ",
            SiteLinkType.Occupation => " stopped working at ",
            _ => " UNKNOWN LINKTYPE (" + LinkType + ") ",
        };
        if (Structure != null)
        {
            eventString += Structure.ToLink(link, pov, this);
        }
        else
        {
            eventString += "UNKNOWN STRUCTURE";
        }
        if (Civ != null)
        {
            eventString += " of " + Civ.ToLink(link, pov, this);
        }
        if (Site != null)
        {
            eventString += " in " + Site.ToLink(link, pov, this);
        }
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}