﻿using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends;

public enum EntitySiteLinkType // legends_plus.xml
{
    Unknown
}

public class EntitySiteLink : DwarfObject // legends_plus.xml
{
    public EntitySiteLink(List<Property> properties, World world)
    {
        Type = EntitySiteLinkType.Unknown;
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "type":
                    //All unknown for the time being.
                    break;
                case "site":
                    Site = world.GetSite(property.ValueAsInt());
                    break;
                case "strength":
                    Strength = property.ValueAsInt();
                    break;
            }
        }
    }

    public EntitySiteLinkType Type { get; set; }
    public Site Site { get; set; }
    public int Strength { get; set; }
}