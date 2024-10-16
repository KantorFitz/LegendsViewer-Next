using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class SiteAbandoned : WorldEvent
{
    public Entity? Civ { get; set; }
    public Entity? SiteEntity { get; set; }
    public Site? Site { get; set; }

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

        if (Site != null)
        {
            Site.OwnerHistory.Last().EndYear = Year;
            Site.OwnerHistory.Last().EndCause = "abandoned";
            world.AddPlayerRelatedDwarfObjects(Site);
        }
        if (SiteEntity != null)
        {
            SiteEntity.SiteHistory.Last(s => s.Site == Site).EndYear = Year;
            SiteEntity.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";
            world.AddPlayerRelatedDwarfObjects(SiteEntity);
        }
        if (Civ != null)
        {
            Civ.SiteHistory.Last(s => s.Site == Site).EndYear = Year;
            Civ.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";
        }

        Civ.AddEvent(this);
        SiteEntity.AddEvent(this);
        Site.AddEvent(this);
    }
    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        if (SiteEntity != null && SiteEntity != Civ)
        {
            eventString += SiteEntity.ToLink(link, pov, this) + " of ";
        }

        eventString += Civ?.ToLink(link, pov, this) + " abandoned the settlement at " + Site?.ToLink(link, pov, this);
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}