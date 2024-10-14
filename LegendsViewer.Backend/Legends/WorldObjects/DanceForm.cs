﻿using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.WorldObjects;

public class DanceForm : ArtForm
{
    public DanceForm(List<Property> properties, World world)
        : base(properties, world)
    {
        Icon = HtmlStyleUtil.GetIconString("dance-ballroom");
        FormType = FormType.Dance;
        Type = FormType.GetDescription();
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = "Dance Form";
            title += "&#13";
            title += "Events: " + Events.Count;
            return pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "danceform", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
        }
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
