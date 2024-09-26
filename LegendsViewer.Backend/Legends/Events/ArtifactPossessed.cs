using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class ArtifactPossessed : WorldEvent
{
    public Artifact Artifact { get; set; }
    public int UnitId { get; set; }
    public HistoricalFigure HistoricalFigure { get; set; }
    public Site Site { get; set; }
    public WorldRegion Region { get; set; }
    public UndergroundRegion UndergroundRegion { get; set; }
    public ArtifactReason ArtifactReason { get; set; }
    public int ReasonId { get; set; }
    public Circumstance Circumstance { get; set; }
    public int CircumstanceId { get; set; }

    public HistoricalFigure FamilyFigure { get; set; }
    public HistoricalFigure FormerHolder { get; set; }
    public Entity SymbolEntity { get; set; }

    public ArtifactPossessed(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); break;
                case "unit_id": UnitId = Convert.ToInt32(property.Value); break;
                case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                case "reason":
                    switch (property.Value)
                    {
                        case "artifact is heirloom of family hfid":
                            ArtifactReason = ArtifactReason.ArtifactIsHeirloomOfFamilyHfid;
                            break;
                        case "artifact is symbol of entity position":
                            ArtifactReason = ArtifactReason.ArtifactIsSymbolOfEntityPosition;
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "reason_id":
                    ReasonId = Convert.ToInt32(property.Value);
                    break;
                case "circumstance":
                    switch (property.Value)
                    {
                        case "hf is dead":
                            Circumstance = Circumstance.HfIsDead;
                            break;
                        default:
                            Circumstance = Circumstance.Unknown;
                            property.Known = false;
                            break;
                    }
                    break;
                case "circumstance_id":
                    CircumstanceId = Convert.ToInt32(property.Value);
                    break;
            }
        }
        switch (ArtifactReason)
        {
            case ArtifactReason.ArtifactIsHeirloomOfFamilyHfid:
                FamilyFigure = world.GetHistoricalFigure(ReasonId);
                FamilyFigure.AddEvent(this);
                break;
            case ArtifactReason.ArtifactIsSymbolOfEntityPosition:
                SymbolEntity = world.GetEntity(ReasonId);
                SymbolEntity.AddEvent(this);
                break;
        }
        switch (Circumstance)
        {
            case Circumstance.HfIsDead:
                FormerHolder = world.GetHistoricalFigure(CircumstanceId);
                if (FormerHolder != FamilyFigure)
                {
                    FormerHolder.AddEvent(this);
                }
                break;
        }
        Artifact.AddEvent(this);
        if (HistoricalFigure != HistoricalFigure.Unknown)
        {
            HistoricalFigure.AddEvent(this);
        }
        Site.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject pov = null)
    {
        string eventString = GetYearTime() + Artifact.ToLink(link, pov, this);
        switch (ArtifactReason)
        {
            case ArtifactReason.ArtifactIsHeirloomOfFamilyHfid:
            case ArtifactReason.ArtifactIsSymbolOfEntityPosition:
                eventString += " was acquired";
                break;
            default:
                eventString += " was claimed";
                break;
        }
        if (Site != null)
        {
            eventString += " in ";
            eventString += Site.ToLink(link, pov, this);
        }
        else if (Region != null)
        {
            eventString += " in ";
            eventString += Region.ToLink(link, pov, this);
        }
        else if (UndergroundRegion != null)
        {
            eventString += " in ";
            eventString += UndergroundRegion.ToLink(link, pov, this);
        }

        eventString += " by " + HistoricalFigure.ToLink(link, pov, this);
        switch (ArtifactReason)
        {
            case ArtifactReason.ArtifactIsHeirloomOfFamilyHfid:
                eventString += " as an heirloom of the ";
                eventString += FamilyFigure?.ToLink(link, pov, this);
                eventString += " family";
                break;
            case ArtifactReason.ArtifactIsSymbolOfEntityPosition:
                eventString += " as a symbol of authority within ";
                eventString += SymbolEntity?.ToLink(link, pov, this);
                break;
        }
        switch (Circumstance)
        {
            case Circumstance.HfIsDead:
                eventString += " after the death of ";
                eventString += FormerHolder?.ToLink(link, pov, this);
                break;
        }
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}