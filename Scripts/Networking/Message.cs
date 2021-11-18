using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CableWalker.Simulator
{
    public class Message
    {

        public Dictionary<string, dynamic> message = new Dictionary<string, dynamic>();
        public Dictionary<string, dynamic> messageValue = new Dictionary<string, dynamic>();
        public string messageType;

        public Message(string type, Dictionary<string, dynamic> messageValue)
        {
            messageType = type;
            this.messageValue = messageValue;
            message["type"] = messageType;
            if (messageValue == null)
                message["msg"] = "";
            else
                message["msg"] = messageValue;
        }

        public Message(string jsonString)
        {
            message = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);
            messageType = message["type"];
            messageValue = message["msg"].ToObject<Dictionary<string, object>>();
        }

        public string getJsonString()
        {
            return JsonConvert.SerializeObject(message);
        }

    }
}
