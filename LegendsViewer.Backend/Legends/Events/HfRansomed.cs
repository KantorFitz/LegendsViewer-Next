﻿using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.Events;

public class HfRansomed : WorldEvent
{
    public HistoricalFigure RansomedHf { get; set; }
    public HistoricalFigure RansomerHf { get; set; }
    public HistoricalFigure PayerHf { get; set; }
    public Entity PayerEntity { get; set; }
    public Site MovedToSite { get; set; }

    public HfRansomed(List<Property> properties, World world) : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "ransomed_hfid": RansomedHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "ransomer_hfid": RansomerHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "payer_hfid": PayerHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                case "payer_entity_id": PayerEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "moved_to_site_id": MovedToSite = world.GetSite(Convert.ToInt32(property.Value)); break;
            }
        }

        PayerHf.AddEvent(this);
        PayerEntity.AddEvent(this);
        RansomedHf.AddEvent(this);
        RansomerHf.AddEvent(this);
        MovedToSite.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        eventString += RansomerHf.ToLink(link, pov, this);
        eventString += " ransomed ";
        eventString += RansomedHf.ToLink(link, pov, this);
        if (PayerHf != null)
        {
            eventString += " to ";
            eventString += PayerHf.ToLink(link, pov, this);
            if (PayerEntity != null)
            {
                eventString += " of ";
                eventString += PayerEntity.ToLink(link, pov, this);
            }
        }
        else if (PayerEntity != null)
        {
            eventString += " to ";
            eventString += PayerEntity.ToLink(link, pov, this);
        }
        eventString += PrintParentCollection(link, pov);
        eventString += ". ";
        if (MovedToSite != null)
        {
            eventString += RansomedHf.ToLink(link, pov, this).ToUpperFirstLetter();
            eventString += " was sent to ";
            eventString += MovedToSite.ToLink(link, pov, this);
        }
        return eventString;
    }
}