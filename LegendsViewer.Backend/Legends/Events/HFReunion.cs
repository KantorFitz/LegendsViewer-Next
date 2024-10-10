using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class HfReunion : WorldEvent
{
    public HistoricalFigure HistoricalFigure1, HistoricalFigure2;
    public Site Site;
    public WorldRegion Region;
    public UndergroundRegion UndergroundRegion;
    public HfReunion(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "group_1_hfid": HistoricalFigure1 = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "group_2_hfid": HistoricalFigure2 = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
            }
        }

        HistoricalFigure1.AddEvent(this);
        HistoricalFigure2.AddEvent(this);
        Site.AddEvent(this);
        Region.AddEvent(this);
        UndergroundRegion.AddEvent(this);
    }
    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime() + " " + HistoricalFigure1.ToLink(link, pov, this) + " was reunited with " + HistoricalFigure2.ToLink(link, pov, this);
        if (Site != null)
        {
            eventString += " in " + Site.ToLink(link, pov, this);
        }
        else if (Region != null)
        {
            eventString += " in " + Region.ToLink(link, pov, this);
        }

        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}