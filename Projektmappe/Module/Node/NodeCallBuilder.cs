using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GVRP.Module.Node
{
    public class NodeCallBuilder
    {
        public string Name { get; }
        public List<NodeArg> ArgList { get; }

        public NodeCallBuilder(string functionName)
        {
            ArgList = new List<NodeArg>();
            Name = functionName;
        }

        public NodeCallBuilder AddInt(int value)
        {
            AddType("", value);
            return this;
        }

        public NodeCallBuilder AddFloat(float value)
        {
            AddType("", value);
            return this;
        }

        public NodeCallBuilder AddString(string value)
        {
            AddType("", value);
            return this;
        }

        public NodeCallBuilder AddBool(bool value)
        {
            AddType("", value);
            return this;
        }

        public NodeCallBuilder AddPlayer(Client client)
        {
            AddType("player", client.Handle.Value);
            return this;
        }

        public NodeCallBuilder AddVehicle(Vehicle vehicle)
        {
            AddType("vehicle", vehicle.Handle.Value);
            return this;
        }

        public NodeCall Build()
        {
            return new NodeCall(Name, JsonConvert.SerializeObject(ArgList));
        }

        private void AddType(string type, object value)
        {
            ArgList.Add(new NodeArg(type, value));
        }
    }

    public class NodeCall
    {
        public string Name { get; }
        public string Args { get; }

        public NodeCall(string name, string args)
        {
            Name = name;
            Args = args;
        }
    }

    public class NodeArg
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; }
        [JsonProperty(PropertyName = "value")]
        public object Value { get; }

        public NodeArg(string type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
