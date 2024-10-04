﻿using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Utilities;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class WrittenContent : WorldObject
{
    public static readonly string Icon = HtmlStyleUtil.GetIconString("bookshelf");

    public int PageStart { get; set; } // legends_plus.xml
    public int PageEnd { get; set; } // legends_plus.xml
    public WrittenContentType WrittenContentType { get; set; } // legends_plus.xml

    [JsonIgnore]
    public HistoricalFigure? Author { get; set; } // legends_plus.xml
    public int AuthorId { get; set; } // legends_plus.xml
    public List<string> Styles { get; set; } = [];
    public List<Reference> References { get; set; } = [];
    public string TypeAsString => WrittenContentType.GetDescription();
    public int PageCount => PageEnd - PageStart + 1;
    public int AuthorRoll { get; set; }
    public int FormId { get; set; }

    public WrittenContent(List<Property> properties, World world)
        : base(properties, world)
    {
        FormId = -1;
        AuthorId = -1;

        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "title": Name = Formatting.InitCaps(property.Value); break;
                case "page_start": PageStart = Convert.ToInt32(property.Value); break;
                case "page_end": PageEnd = Convert.ToInt32(property.Value); break;
                case "reference":
                    property.Known = true;
                    if (property.SubProperties != null)
                    {
                        References.Add(new Reference(property.SubProperties, world));
                    }

                    break;
                case "form":
                case "type":
                    switch (property.Value)
                    {
                        case "Autobiography":
                        case "autobiography":
                            WrittenContentType = WrittenContentType.Autobiography; break;
                        case "Biography":
                        case "biography":
                            WrittenContentType = WrittenContentType.Biography; break;
                        case "Chronicle":
                        case "chronicle":
                            WrittenContentType = WrittenContentType.Chronicle; break;
                        case "Dialog":
                        case "dialog":
                            WrittenContentType = WrittenContentType.Dialog; break;
                        case "Essay":
                        case "essay":
                            WrittenContentType = WrittenContentType.Essay; break;
                        case "Guide":
                        case "guide":
                            WrittenContentType = WrittenContentType.Guide; break;
                        case "Letter":
                        case "letter":
                            WrittenContentType = WrittenContentType.Letter; break;
                        case "Manual":
                        case "manual":
                            WrittenContentType = WrittenContentType.Manual; break;
                        case "Novel":
                        case "novel":
                            WrittenContentType = WrittenContentType.Novel; break;
                        case "Play":
                        case "play":
                            WrittenContentType = WrittenContentType.Play; break;
                        case "Poem":
                        case "poem":
                            WrittenContentType = WrittenContentType.Poem; break;
                        case "ShortStory":
                        case "short story":
                            WrittenContentType = WrittenContentType.ShortStory; break;
                        case "MusicalComposition":
                        case "musical composition":
                            WrittenContentType = WrittenContentType.MusicalComposition; break;
                        case "Choreography":
                        case "choreography":
                            WrittenContentType = WrittenContentType.Choreography; break;
                        case "CulturalHistory":
                        case "cultural history":
                            WrittenContentType = WrittenContentType.CulturalHistory; break;
                        case "StarChart":
                        case "star chart":
                            WrittenContentType = WrittenContentType.StarChart; break;
                        case "ComparativeBiography":
                        case "comparative biography":
                            WrittenContentType = WrittenContentType.ComparativeBiography; break;
                        case "CulturalComparison":
                        case "cultural comparison":
                            WrittenContentType = WrittenContentType.CulturalComparison; break;
                        case "Atlas":
                        case "atlas":
                            WrittenContentType = WrittenContentType.Atlas; break;
                        case "TreatiseOnTechnologicalEvolution":
                        case "treatise on technological evolution":
                            WrittenContentType = WrittenContentType.TreatiseOnTechnologicalEvolution; break;
                        case "AlternateHistory":
                        case "alternate history":
                            WrittenContentType = WrittenContentType.AlternateHistory; break;
                        case "StarCatalogue":
                        case "star catalogue":
                            WrittenContentType = WrittenContentType.StarCatalogue; break;
                        case "Dictionary":
                        case "dictionary":
                            WrittenContentType = WrittenContentType.Dictionary; break;
                        case "Genealogy":
                        case "genealogy":
                            WrittenContentType = WrittenContentType.Genealogy; break;
                        case "Encyclopedia":
                        case "encyclopedia":
                            WrittenContentType = WrittenContentType.Encyclopedia; break;
                        case "BiographicalDictionary":
                        case "biographical dictionary":
                            WrittenContentType = WrittenContentType.BiographicalDictionary; break;
                        default:
                            WrittenContentType = WrittenContentType.Unknown;
                            if (!int.TryParse(property.Value.Replace("unknown ", ""), out _))
                            {
                                property.Known = false;
                            }
                            break;
                    }
                    break;
                case "author":
                case "author_hfid":
                    AuthorId = Convert.ToInt32(property.Value);
                    Author = world.GetHistoricalFigure(AuthorId);
                    break;
                case "style":
                    var style = property.Value.Contains(":")
                        ? string.Intern(property.Value.Substring(0, property.Value.IndexOf(":", StringComparison.Ordinal))).ToLower()
                        : string.Intern(property.Value).ToLower();
                    if (Styles.Contains(style.Replace(" ", "")))
                    {
                        Styles.Remove(style.Replace(" ", ""));
                        Styles.Add(style);
                    }
                    else if (Styles.All(s => s.Replace(" ", "") != style))
                    {
                        Styles.Add(style);
                    }
                    break;
                case "author_roll":
                    AuthorRoll = Convert.ToInt32(property.Value);
                    break;
                case "form_id":
                    FormId = Convert.ToInt32(property.Value);
                    break;
            }
        }
        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = "An untitled " + WrittenContentType.GetDescription().ToLower();
        }
        Type = WrittenContentType.GetDescription();
    }

    public override string ToString()
    {
        return Name;
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string? type = null;
            if (WrittenContentType != WrittenContentType.Unknown)
            {
                type = WrittenContentType.GetDescription();
            }
            string title = "Written Content";
            title += string.IsNullOrWhiteSpace(type) ? "" : ", " + type;
            title += "&#13";
            title += "Events: " + Events.Count;

            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "writtencontent", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
