using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class ArtifactCreated : WorldEvent
{
    public Artifact? Artifact { get; set; }
    public bool ReceivedName { get; set; }
    public HistoricalFigure? HistoricalFigure { get; set; }
    public Site? Site { get; set; }
    public HistoricalFigure? SanctifyFigure { get; set; }
    public ArtifactReason Reason { get; set; }
    public Circumstance Circumstance { get; set; }
    public HistoricalFigure? DefeatedFigure { get; set; }
    public Entity? Entity { get; set; }

    public ArtifactCreated(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); break;
                case "hist_figure_id":
                case "creator_hfid":
                    HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "entity_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "name_only": ReceivedName = true; property.Known = true; break;
                case "hfid": if (HistoricalFigure == null) { HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "site": if (Site == null) { Site = world.GetSite(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "unit_id":
                case "creator_unit_id":
                case "anon_3":
                    if (property.Value != "-1")
                    {
                        property.Known = false;
                    }
                    break;
                case "anon_4":
                case "sanctify_hf":
                    SanctifyFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "reason":
                    switch (property.Value)
                    {
                        case "sanctify_hf":
                            Reason = ArtifactReason.SanctifyHistoricalFigure;
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    if (!property.Known && property.Value.Contains("unknown"))
                    {
                        property.Known = true;
                    }
                    break;
                case "circumstance":
                    property.Known = true;
                    if (property.SubProperties == null)
                    {
                        continue;
                    }
                    foreach (var subProperty in property.SubProperties)
                    {
                        switch (subProperty.Name)
                        {
                            case "type":
                                switch (subProperty.Value)
                                {
                                    case "defeated":
                                        Circumstance = Circumstance.DefeatedHf;
                                        break;
                                    case "conflict":
                                        Circumstance = Circumstance.Conflict;
                                        break;
                                    case "trauma":
                                        Circumstance = Circumstance.Trauma;
                                        break;
                                    default:
                                        property.Known = false;
                                        break;
                                }
                                break;
                            case "defeated":
                                DefeatedFigure = world.GetHistoricalFigure(Convert.ToInt32(subProperty.Value));
                                break;
                        }
                    }
                    break;
            }
        }

        if (Artifact != null && HistoricalFigure != null)
        {
            Artifact.Creator = HistoricalFigure;
        }
        Artifact?.AddEvent(this);
        HistoricalFigure?.AddEvent(this);
        Site?.AddEvent(this);
        SanctifyFigure?.AddEvent(this);
        DefeatedFigure?.AddEvent(this);
        Entity?.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime() + Artifact?.ToLink(link, pov, this);
        if (ReceivedName)
        {
            eventString += " received its name";
        }
        else
        {
            eventString += " was created";
        }

        if (Site != null)
        {
            eventString += " in " + Site.ToLink(link, pov, this);
        }

        if (ReceivedName)
        {
            eventString += " from ";
        }
        else
        {
            eventString += " by ";
        }

        eventString += HistoricalFigure != null ? HistoricalFigure.ToLink(link, pov, this) : "UNKNOWN HISTORICAL FIGURE";
        if (SanctifyFigure != null)
        {
            eventString += " in order to sanctify ";
            eventString += SanctifyFigure.ToLink(link, pov, this);
            eventString += " by preserving a part of the body";
        }

        if (DefeatedFigure != null)
        {
            eventString += " after defeating ";
            eventString += DefeatedFigure.ToLink(link, pov, this);
        }
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}