using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ReactCRM.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string SSN { get; set; }
        public string Name { get; set; }
        public DateTime? DOB { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public int? TabId { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }

        // Upload Link properties
        public string UploadToken { get; set; }
        public DateTime? UploadLinkExpires { get; set; }

        public Client()
        {
            ExtraData = new Dictionary<string, object>();
        }

        public string GetExtraDataValue(string fieldName)
        {
            if (ExtraData != null && ExtraData.ContainsKey(fieldName))
            {
                var value = ExtraData[fieldName];

                // Handle different types that might come from JSON deserialization
                if (value == null)
                    return null;

                // If it's a JToken/JValue (from Newtonsoft.Json), get the actual value
                if (value is JValue jValue)
                {
                    return jValue.Value?.ToString();
                }
                else if (value is JToken jToken)
                {
                    return jToken.ToString();
                }

                return value.ToString();
            }
            return null;
        }

        public void SetExtraDataValue(string fieldName, object value)
        {
            if (ExtraData == null)
            {
                ExtraData = new Dictionary<string, object>();
            }

            ExtraData[fieldName] = value;
        }

                    /// <summary>

                    /// Gets the last name from ExtraData using a dictionary of possible field names.

                    /// Checks for: last name, Apellido, lastname, LAST NAME, Last Name, apellido, LastName

                    /// </summary>

        public string GetLastName()

        {

            if (ExtraData == null || ExtraData.Count == 0)

                return null;



            // Dictionary of possible last name field variations

            var lastNameFields = new[]

            {

                "last name",

                "Last Name",

                "LAST NAME",

                "lastname",

                "LastName",

                "Apellido",

                "apellido",

                "APELLIDO"

            };



            foreach (var fieldName in lastNameFields)

            {

                var lastName = GetExtraDataValue(fieldName);

                if (!string.IsNullOrWhiteSpace(lastName))

                {

                    return lastName.Trim();

                }

            }



            return null;

        }



        /// <summary>

        /// Gets the full name (first name + last name if available)

        /// </summary>

        public string GetFullName()

        {

            var lastName = GetLastName();

            if (!string.IsNullOrWhiteSpace(lastName))

            {

                return $"{Name} {lastName}";

            }

            return Name;

        }
    }
    }