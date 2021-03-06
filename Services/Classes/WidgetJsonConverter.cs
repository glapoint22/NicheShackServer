﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.Classes
{
    public class WidgetJsonConverter : JsonConverter<Widget>
    {
        public override Widget Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Widget widget = null;
            int startDepth = reader.CurrentDepth;


            while (reader.Read())
            {
                // Return the widget when we are at the end of the object
                if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
                {
                    return widget;
                }



                // If we are reading a property name
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string property = reader.GetString();

                    // If the property name is widget type
                    if (property == "widgetType")
                    {
                        reader.Read();
                        WidgetType widgetType = (WidgetType)reader.GetInt32();

                        // Get the widget
                        switch (widgetType)
                        {
                            case WidgetType.Button:
                                widget = new ButtonWidget();
                                break;
                            case WidgetType.Text:
                                widget = new TextWidget();
                                break;
                            case WidgetType.Image:
                                widget = new ImageWidget();
                                break;
                            case WidgetType.Container:
                                widget = new ContainerWidget();
                                break;
                            case WidgetType.Line:
                                widget = new LineWidget();
                                break;
                            case WidgetType.Video:
                                widget = new VideoWidget();
                                break;
                            case WidgetType.ProductGroup:
                                widget = new ProductGroupWidget();
                                break;
                            case WidgetType.Shop:
                                widget = new ShopWidget();
                                break;
                            case WidgetType.Carousel:
                                widget = new CarouselWidget();
                                break;
                            case WidgetType.Grid:
                                widget = new GridWidget();
                                break;
                            case WidgetType.Section:
                                widget = new SectionWidget();
                                break;
                            case WidgetType.Divider:
                                widget = new DividerWidget();
                                break;
                        }

                        widget.WidgetType = widgetType;
                    }
                    else
                    {
                        // Set each property for the widget
                        widget.SetProperty(property, ref reader, options);
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Widget widget, JsonSerializerOptions options)
        {
            switch (widget.WidgetType)
            {
                case WidgetType.Button:
                    JsonSerializer.Serialize(writer, (ButtonWidget)widget, options);
                    break;
                case WidgetType.Text:
                    JsonSerializer.Serialize(writer, (TextWidget)widget, options);
                    break;
                case WidgetType.Image:
                    JsonSerializer.Serialize(writer, (ImageWidget)widget, options);
                    break;
                case WidgetType.Container:
                    JsonSerializer.Serialize(writer, (ContainerWidget)widget, options);
                    break;
                case WidgetType.Line:
                    JsonSerializer.Serialize(writer, (LineWidget)widget, options);
                    break;
                case WidgetType.Video:
                    JsonSerializer.Serialize(writer, (VideoWidget)widget, options);
                    break;
                case WidgetType.ProductGroup:
                    JsonSerializer.Serialize(writer, (ProductGroupWidget)widget, options);
                    break;
                case WidgetType.Shop:
                    JsonSerializer.Serialize(writer, (ShopWidget)widget, options);
                    break;
                case WidgetType.Carousel:
                    JsonSerializer.Serialize(writer, (CarouselWidget)widget, options);
                    break;
                case WidgetType.Grid:
                    JsonSerializer.Serialize(writer, (GridWidget)widget, options);
                    break;
                case WidgetType.Section:
                    JsonSerializer.Serialize(writer, (SectionWidget)widget, options);
                    break;
                case WidgetType.Divider:
                    JsonSerializer.Serialize(writer, (DividerWidget)widget, options);
                    break;
            }

            
        }
    }
}
