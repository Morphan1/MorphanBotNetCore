using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
                            List = attribute.List
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

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                List<string> files = token.ToObject<List<string>>();
                IList list = (IList)Activator.CreateInstance(objectType);
                foreach (string file in files)
                {
                    list.Add(ReadFile(serializer, file, objectType.GenericTypeArguments[0]));
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
                foreach (object obj in list)
                {
                    string filePath = ((IFileReferenceable)obj).GetFilePath();
                    WriteFile(serializer, filePath, obj);
                    files.Add(filePath);
                }
                serializer.Serialize(writer, files);
                return;
            }
            string path = ((IFileReferenceable)value).GetFilePath();
            WriteFile(serializer, path, value);
            serializer.Serialize(writer, path);
        }

        private static object ReadFile(JsonSerializer serializer, string file, Type objectType)
        {
            using (FileStream stream = File.OpenRead($"{file}.json"))
            using (StreamReader sr = new StreamReader(stream, Utilities.DefaultEncoding))
            using (JsonReader fileReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(fileReader, objectType);
            }
        }

        private static void WriteFile(JsonSerializer serializer, string file, object value)
        {
            using (FileStream stream = File.OpenWrite($"{file}.json"))
            using (StreamWriter sw = new StreamWriter(stream, Utilities.DefaultEncoding))
            using (JsonWriter fileWriter = new JsonTextWriter(sw))
            {
                serializer.Serialize(fileWriter, value);
            }
        }
    }
}
