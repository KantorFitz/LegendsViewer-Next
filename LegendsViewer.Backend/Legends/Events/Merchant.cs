using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class Merchant : WorldEvent
{
    public Entity? Source { get; set; }
    public Entity? Destination { get; set; }
    public Site? Site { get; set; }
    public bool Seizure { get; set; }
    public bool LostValue { get; set; }
    public bool HardShip { get; set; }
    public bool AllDead { get; set; }

    public Merchant(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "source":
                case "trader_entity_id":
                    Source = world.GetEntity(Convert.ToInt32(property.Value));
                    break;
                case "destination":
                case "depot_entity_id":
                    Destination = world.GetEntity(Convert.ToInt32(property.Value));
                    break;
                case "site":
                    Site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "site_id":
                    // points to wrong site
                    property.Known = true;
                    break;
                case "seizure": Seizure = true; property.Known = true; break;
                case "lost_value": LostValue = true; property.Known = true; break;
                case "hardship": HardShip = true; property.Known = true; break;
                case "all_dead": AllDead = true; property.Known = true; break;
            }
        }
        Source.AddEvent(this);
        Destination.AddEvent(this);
        Site.AddEvent(this);
    }
    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        eventString += "merchants from ";
        eventString += Source != null ? Source.ToLink(link, pov, this) : "UNKNOWN CIV";
        eventString += " visited ";
        eventString += Destination != null ? Destination.ToLink(link, pov, this) : "UNKNOWN ENTITY";
        eventString += " at ";
        eventString += Site != null ? Site.ToLink(link, pov, this) : "UNKNOWN SITE";
        if (HardShip)
        {
            eventString += " and suffered great hardships";
        }
        eventString += ".";
        if (AllDead)
        {
            eventString += " They never returned.";
        }
        if (Seizure)
        {
            eventString += " They reported a seizure of goods.";
        }
        if (LostValue)
        {
            eventString += " They reported irregularities with their goods.";
        }
        return eventString;
    }
}