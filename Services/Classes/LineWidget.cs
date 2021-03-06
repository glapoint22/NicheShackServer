﻿using DataAccess.Models;
using HtmlAgilityPack;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Classes
{
    public class LineWidget : Widget
    {
        public Border Border { get; set; }
        public Shadow Shadow { get; set; }


        public async override Task<HtmlNode> Create(HtmlNode column, NicheShackContext context)
        {
            // Call the base
            HtmlNode widget = await base.Create(column, context);


            // Td
            HtmlNode td = widget.SelectSingleNode("tr/td");


            td.SetAttributeValue("style", "border-bottom: " + Border.Width + "px " + Border.Style + " " + Border.Color + ";");
            if (Shadow != null) Shadow.SetStyle(td);


            HtmlNode blankRow = widget.InsertBefore(HtmlNode.CreateNode("<tr>"), widget.SelectSingleNode("tr"));
            HtmlNode blankColumn = blankRow.AppendChild(HtmlNode.CreateNode("<td>"));
            blankColumn.SetAttributeValue("height", "10");


            blankRow = widget.AppendChild(HtmlNode.CreateNode("<tr>"));
            blankColumn = blankRow.AppendChild(HtmlNode.CreateNode("<td>"));
            blankColumn.SetAttributeValue("height", "10");


            return widget;
        }

        

        public override void SetProperty(string property, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            base.SetProperty(property, ref reader, options);

            switch (property)
            {
                case "border":
                    Border = (Border)JsonSerializer.Deserialize(ref reader, typeof(Border), options);
                    break;

                case "shadow":
                    Shadow = (Shadow)JsonSerializer.Deserialize(ref reader, typeof(Shadow), options);
                    break;
            }
        }





        public override Task SetData(NicheShackContext context, QueryParams queryParams)
        {
            return Task.FromResult(false);
        }
    }
}
