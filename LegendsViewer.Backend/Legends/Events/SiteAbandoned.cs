using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class SiteAbandoned : WorldEvent
{
    public Entity Civ, SiteEntity;
    public Site Site;
    public SiteAbandoned(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
            }
        }

        Site.OwnerHistory.Last().EndYear = Year;
        Site.OwnerHistory.Last().EndCause = "abandoned";
        if (SiteEntity != null)
        {
            SiteEntity.SiteHistory.Last(s => s.Site == Site).EndYear = Year;
            SiteEntity.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";
        }
        Civ.SiteHistory.Last(s => s.Site == Site).EndYear = Year;
        Civ.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";

        Civ.AddEvent(this);
        SiteEntity.AddEvent(this);
        Site.AddEvent(this);

        world.AddPlayerRelatedDwarfObjects(SiteEntity);
        world.AddPlayerRelatedDwarfObjects(Site);
    }
    public override string Print(bool link = true, DwarfObject pov = null)
    {
        string eventString = GetYearTime();
        if (SiteEntity != null && SiteEntity != Civ)
        {
            eventString += SiteEntity.ToLink(link, pov, this) + " of ";
        }

        eventString += Civ.ToLink(link, pov, this) + " abandoned the settlement at " + Site.ToLink(link, pov, this);
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}