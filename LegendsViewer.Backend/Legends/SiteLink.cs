using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends;

public class SiteLink
{
    [JsonIgnore]
    public Site Site { get; set; }
    public string? SiteToLink => Site?.ToLink(true);

    public SiteLinkType Type { get; set; }
    public int SubId { get; set; }
    public int OccupationId { get; set; }

    [JsonIgnore]
    public Entity Entity { get; set; }
    public string? EntityToLink => Entity?.ToLink(true);

    public SiteLink(List<Property> properties, World world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "link_type":
                    switch (property.Value)
                    {
                        case "lair": Type = SiteLinkType.Lair; break;
                        case "hangout": Type = SiteLinkType.Hangout; break;
                        case "home_site_building": Type = SiteLinkType.HomeSiteBuilding; break;
                        case "home_site_underground": Type = SiteLinkType.HomeSiteUnderground; break;
                        case "home_structure": Type = SiteLinkType.HomeStructure; break;
                        case "seat_of_power": Type = SiteLinkType.SeatOfPower; break;
                        case "occupation": Type = SiteLinkType.Occupation; break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "site_id":
                    Site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "sub_id":
                    SubId = Convert.ToInt32(property.Value);
                    break;
                case "entity_id":
                    Entity = world.GetEntity(Convert.ToInt32(property.Value));
                    break;
                case "occupation_id":
                    OccupationId = Convert.ToInt32(property.Value);
                    break;
            }
        }
    }
}
