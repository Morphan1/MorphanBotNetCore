using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MorphanBotNetCore.Storage
{
    public class JsonFileReferenceResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            foreach (JsonProperty property in properties)
            {
                PropertyInfo info = type.GetProperty(property.UnderlyingName);
                if (info != null)
                {
                    FileReferenceAttribute attribute = info.GetCustomAttribute<FileReferenceAttribute>();
                    if (attribute != null)
                    {
                        property.Converter = new JsonFileReferenceConverter()
                        {
                            List = attribute.List,
                            Folder = attribute.Folder
                        };
                    }
                }
            }
            return properties;
        }
    }

    public class JsonFileReferenceConverter : JsonConverter
    {
        public bool List;

        public string Folder;

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                Type type = objectType.GenericTypeArguments[0];
                List<string> files = token.ToObject<List<string>>();
                IList list = (IList)Activator.CreateInstance(objectType);
                foreach (string file in files)
                {
                    list.Add(ReadFile(serializer, file, type));
                }
                return list;
            }
            return ReadFile(serializer, token.ToString(), objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (List)
            {
                IList list = (IList)value;
                List<string> files = new List<string>();
                PropertyInfo nameProperty = value.GetType().GenericTypeArguments[0].GetProperties()
                    .Where((prop) => prop.GetCustomAttribute<FileNameAttribute>() != null)
                    .First();
                foreach (object obj in list)
                {
                    string filePath = Folder + ((string)nameProperty.GetValue(obj)).ToLowerInvariant().StripNonAlphaNumeric();
                    WriteFile(serializer, filePath, obj);
                    files.Add(filePath);
                }
                serializer.Serialize(writer, files);
                return;
            }
            PropertyInfo nameProp = value.GetType().GetProperties()
                .Where((prop) => prop.GetCustomAttribute<FileNameAttribute>() != null)
                .First();
            string path = Folder + ((string)nameProp.GetValue(value)).ToLowerInvariant().StripNonAlphaNumeric();
            WriteFile(serializer, path, value);
            serializer.Serialize(writer, path);
        }

        private static object ReadFile(JsonSerializer serializer, string file, Type objectType)
        {
            file += ".json";
            if (!File.Exists(file))
            {
                return default;
            }
            using (FileStream stream = File.OpenRead(file))
            using (StreamReader sr = new StreamReader(stream, Utilities.DefaultEncoding))
            using (JsonReader fileReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(fileReader, objectType);
            }
        }

        private static void WriteFile(JsonSerializer serializer, string file, object value)
        {
            file += ".json";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            using (FileStream stream = File.OpenWrite(file))
            using (StreamWriter sw = new StreamWriter(stream, Utilities.DefaultEncoding))
            using (JsonWriter fileWriter = new JsonTextWriter(sw))
            {
                serializer.Serialize(fileWriter, value);
            }
        }
    }
}
