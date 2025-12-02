using System;

using System.Collections.Generic;

using System.Reflection;



namespace ReactCRM.Models

{

    public static class ClientFieldMappings

    {

        public static readonly Dictionary<string, string> FieldDisplayNames = new Dictionary<string, string>

        {

            { "Id", "Client ID" },

            { "Name", "Client Name" },

            { "Email", "Email Address" },

            { "Phone", "Phone Number" },

            { "Address", "Address" },

            { "City", "City" },

            { "State", "State" },

            { "ZipCode", "Zip Code" },

            { "Country", "Country" },

            { "Company", "Company Name" },

            { "Notes", "Notes" },

            { "Status", "Status" },

            { "CreatedAt", "Created Date" },

            { "UpdatedAt", "Updated Date" }

        };
        public static readonly Dictionary<string, string> StandardFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)

        {

            { "name", "Name" },

            { "clientname", "Name" },

            { "fullname", "Name" },

            { "ssn", "SSN" },

            { "socialsecurity", "SSN" },

            { "socialsecuritynumber", "SSN" },

            { "dob", "DOB" },

            { "dateofbirth", "DOB" },

            { "birthdate", "DOB" },

            { "phone", "Phone" },

            { "phonenumber", "Phone" },

            { "telephone", "Phone" },

            { "email", "Email" },

            { "emailaddress", "Email" },

            { "currentdate", "CurrentDate" },

            { "date", "CurrentDate" },

            { "todaydate", "CurrentDate" }

        };




        public static List<string> GetClientFieldNames()

        {

            return new List<string>(FieldDisplayNames.Keys);

        }

        public static List<string> GetAvailableFields()

        {

            var fields = new List<string>(FieldDisplayNames.Keys);

            fields.AddRange(new[] { "SSN", "DOB", "CurrentDate" });

            return fields;

        }



        public static string GetDisplayName(string fieldName)

        {

            return FieldDisplayNames.TryGetValue(fieldName, out var displayName)

                ? displayName

                : fieldName;

        }



        public static object? GetFieldValue(Client client, string fieldName)

        {

            if (client == null) return null;



            var property = typeof(Client).GetProperty(fieldName,

                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);



            return property?.GetValue(client);

        }



        public static Dictionary<string, string> GetClientFieldValues(Client client)

        {

            var values = new Dictionary<string, string>();



            if (client == null) return values;



            foreach (var fieldName in FieldDisplayNames.Keys)

            {

                var value = GetFieldValue(client, fieldName);

                values[fieldName] = value?.ToString() ?? string.Empty;

            }



            return values;

        }

    }

}