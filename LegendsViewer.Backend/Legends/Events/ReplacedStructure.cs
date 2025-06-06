using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class ReplacedStructure : WorldEvent
{
    public int OldStructureId { get; set; }
    public int NewStructureId { get; set; }
    public Entity? Civ { get; set; }
    public Entity? SiteEntity { get; set; }
    public Site? Site { get; set; }
    public Structure? OldStructure { get; set; }
    public Structure? NewStructure { get; set; }

    public ReplacedStructure(List<Property> properties, World world) : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "old_ab_id": OldStructureId = Convert.ToInt32(property.Value); break;
                case "new_ab_id": NewStructureId = Convert.ToInt32(property.Value); break;
                case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "site": if (Site == null) { Site = world.GetSite(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "civ": if (Civ == null) { Civ = world.GetEntity(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "site_civ": if (SiteEntity == null) { SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "old_structure": OldStructureId = Convert.ToInt32(property.Value); break;
                case "new_structure": NewStructureId = Convert.ToInt32(property.Value); break;
            }
        }
        if (Site != null)
        {
            OldStructure = Site.Structures.Find(structure => structure.LocalId == OldStructureId);
            NewStructure = Site.Structures.Find(structure => structure.LocalId == NewStructureId);
        }
        Civ.AddEvent(this);
        SiteEntity.AddEvent(this);
        Site.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        eventString += SiteEntity != null ? SiteEntity.ToLink(link, pov, this) : "UNKNOWN ENTITY";
        eventString += " of ";
        eventString += Civ != null ? Civ.ToLink(link, pov, this) : "UNKNOWN CIV";
        eventString += " replaced ";
        eventString += OldStructure != null ? OldStructure.ToLink(link, pov, this) : "UNKNOWN STRUCTURE";
        eventString += " in ";
        eventString += Site != null ? Site.ToLink(link, pov, this) : "UNKNOWN SITE";
        eventString += " with ";
        eventString += NewStructure != null ? NewStructure.ToLink(link, pov, this) : "UNKNOWN STRUCTURE";
        eventString += ".";
        return eventString;
    }
}