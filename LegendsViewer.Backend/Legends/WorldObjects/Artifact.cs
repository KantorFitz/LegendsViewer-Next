﻿using LegendsViewer.Backend.Contracts;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class Artifact : WorldObject, IHasCoordinates
{
    [JsonIgnore]
    public HistoricalFigure? Creator { get; set; }
    public string? CreatorLink => Creator?.ToLink(true, this);

    [JsonIgnore]
    public int HolderId { get; set; } = -1;
    [JsonIgnore]
    public HistoricalFigure? Holder { get; set; }
    public string? HolderLink => Holder?.ToLink(true, this);

    public string? Item { get; set; }
    public string? Description { get; set; } // legends_plus.xml
    public string? Material { get; set; } // legends_plus.xml
    public int PageCount { get; set; } // legends_plus.xml

    [JsonIgnore]
    public int WrittenContentId { get; set; } = -1;

    [JsonIgnore]
    public WrittenContent? WrittenContent { get; set; }
    public string? WrittenContentLink => WrittenContent?.ToLink(true, this);

    public int AbsTileX { get; set; }
    public int AbsTileY { get; set; }
    public int AbsTileZ { get; set; }
    public List<Location> Coordinates { get; set; } = [];

    [JsonIgnore]
    public Structure? Structure { get; set; }
    public string? StructureLink => Structure?.ToLink(true, this);

    [JsonIgnore]
    public Site? Site { get; set; }
    public string? SiteLink => Site?.ToLink(true, this);

    [JsonIgnore]
    public WorldRegion? Region { get; set; }
    public string? RegionLink => Region?.ToLink(true, this);

    public Artifact(List<Property> properties, World world)
        : base(properties, world)
    {
        Icon = HtmlStyleUtil.GetIconString("diamond-stone");
        Name = "Untitled";
        Type = "Unknown";
        Subtype = "";

        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "item":
                    if (property.SubProperties != null)
                    {
                        property.Known = true;
                        foreach (Property subProperty in property.SubProperties)
                        {
                            switch (subProperty.Name)
                            {
                                case "name_string":
                                    Item = Formatting.InitCaps(subProperty.Value);
                                    break;
                                case "page_number":
                                    PageCount = Convert.ToInt32(subProperty.Value);
                                    break;
                                case "page_written_content_id":
                                case "writing_written_content_id":
                                    WrittenContentId = Convert.ToInt32(subProperty.Value);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Item = Formatting.InitCaps(property.Value);
                    }
                    break;
                case "item_type": Type = Formatting.InitCaps(property.Value); break;
                case "item_subtype": Subtype = Formatting.InitCaps(property.Value); break;
                case "item_description": Description = Formatting.InitCaps(property.Value); break;
                case "mat": Material = string.Intern(property.Value); break;
                case "page_count": PageCount = Convert.ToInt32(property.Value); break;
                case "abs_tile_x": AbsTileX = Convert.ToInt32(property.Value); break;
                case "abs_tile_y": AbsTileY = Convert.ToInt32(property.Value); break;
                case "abs_tile_z": AbsTileZ = Convert.ToInt32(property.Value); break;
                case "writing": WrittenContentId = Convert.ToInt32(property.Value); break;
                case "site_id":
                    Site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "subregion_id":
                    Region = world.GetRegion(Convert.ToInt32(property.Value));
                    break;
                case "holder_hfid":
                    HolderId = Convert.ToInt32(property.Value);
                    break;
                case "structure_local_id":
                    Structure = Site?.Structures.Find(structure => structure.LocalId == Convert.ToInt32(property.Value));
                    break;
            }
        }
        if (AbsTileX > 0 && AbsTileY > 0)
        {
            Coordinates.Add(new Location(AbsTileX / 816, AbsTileY / 816));
        }
        else if (Site != null)
        {
            Coordinates.AddRange(Site.Coordinates);
        }
    }

    public void Resolve(World world)
    {
        if (HolderId > -1)
        {
            Holder = world.GetHistoricalFigure(HolderId);
            if (Holder?.HoldingArtifacts.Contains(this) == false)
            {
                Holder.HoldingArtifacts.Add(this);
            }
        }
        if (WrittenContentId > -1)
        {
            WrittenContent = world.GetWrittenContent(WrittenContentId);
            if (WrittenContent != null)
            {
                WrittenContent.Artifact = this;
            }
        }
    }

    public override string ToString() { return Name; }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = "Artifact" + (!string.IsNullOrEmpty(Type) ? ", " + Type : "");
            title += "&#13";
            title += "Events: " + Events.Count;
            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "artifact", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
