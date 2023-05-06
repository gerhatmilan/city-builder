using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simcityModel.Model
{
    public class BuildingConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Building));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo["Type"]!.Value<string>() == "Home" || jo["Type"]!.Value<string>() == "Industry" || jo["Type"]!.Value<string>() == "OfficeBuilding")
                return jo.ToObject<PeopleBuilding>(serializer);

            if (jo["Type"]!.Value<string>() == "Stadium" || jo["Type"]!.Value<string>() == "PoliceStation" || jo["Type"]!.Value<string>() == "FireStation")
            {
                return jo.ToObject<ServiceBuilding>(serializer);
            }

            if (jo["Type"]!.Value<string>() == "Road") {
                return jo.ToObject<Road>(serializer); }

            return null;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
