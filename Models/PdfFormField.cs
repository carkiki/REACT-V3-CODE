using System;

using System.Collections.Generic;



namespace ReactCRM.Models

{

    public class PdfFormField

    {

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? MappedClientField { get; set; }

        public string? DefaultValue { get; set; }

        public List<string>? Options { get; set; }

        public string Value { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsReadOnly { get; set; }

        public string Tooltip { get; set; } = string.Empty;

        public int PageIndex { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }



        public PdfFormField()

        {

        }



        public PdfFormField(string name, string type)

        {

            Name = name;

            Type = type;

        }

    }

}