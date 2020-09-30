﻿using HtmlAgilityPack;

namespace Services.Classes
{
    public class Background
    {
        public string Color { get; set; }
        public BackgroundImage Image { get; set; }


        public void SetStyle(HtmlNode node)
        {
            string styles = node.GetAttributeValue("style", "");

            // Color
            if (Color != null)
            {
                node.SetAttributeValue("bgcolor", Color);
                styles += "background-color: " + Color + ";";
            }

            // Image
            if (Image != null)
            {
                node.SetAttributeValue("background", "{host}/images/" + Image.Url);
                styles += "background-image: url({host}/images/" + Image.Url + ");";
                Image.SetStyle(ref styles);
            }


            node.SetAttributeValue("style", styles);
        }
    }
}
