using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class NewSiteLeader : WorldEvent
{
    public Entity Attacker { get; set; }
    public Entity Defender { get; set; }
    public Entity SiteEntity { get; set; }
    public Entity NewSiteEntity { get; set; }
    public Site Site { get; set; }
    public HistoricalFigure NewLeader { get; set; }

    public NewSiteLeader(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "new_site_civ_id": NewSiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "new_leader_hfid": NewLeader = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
            }
        }

        if (Site != null)
        {
            if (Site.OwnerHistory.Count == 0)
            {
                if (SiteEntity != null)
                {
                    SiteEntity.SetParent(Defender);
                    Site.OwnerHistory.Add(new OwnerPeriod(Site, SiteEntity, -1, "founded"));
                }
            }

            Site.OwnerHistory.Last().EndCause = "taken over";
            Site.OwnerHistory.Last().EndYear = Year;
            Site.OwnerHistory.Last().Ender = Attacker;
            if (NewSiteEntity != null)
            {
                NewSiteEntity.SetParent(Attacker);
                Site.OwnerHistory.Add(new OwnerPeriod(Site, NewSiteEntity, Year, "took over"));
            }
        }

        Attacker.AddEvent(this);
        Defender.AddEvent(this);
        if (SiteEntity != Defender)
        {
            SiteEntity.AddEvent(this);
        }

        Site.AddEvent(this);
        NewSiteEntity.AddEvent(this);
        NewLeader.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject pov = null)
    {
        string eventString = GetYearTime();
        eventString += Attacker?.ToLink(link, pov, this) ?? "an unknown entity";
        eventString += " defeated ";
        if (SiteEntity != null && SiteEntity != Defender)
        {
            eventString += SiteEntity.ToLink(link, pov, this);
            eventString += " of ";
        }

        eventString += Defender?.ToLink(link, pov, this) ?? "an unknown entity";
        eventString += " and placed ";
        eventString += NewLeader?.ToLink(link, pov, this) ?? "an unknown creature";
        eventString += " in charge of ";
        eventString += Site?.ToLink(link, pov, this) ?? "an unknown site";
        if (NewSiteEntity != null)
        {
            eventString += ".";
            eventString += " The new government was called ";
            eventString += NewSiteEntity.ToLink(link, pov, this);
        }
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}