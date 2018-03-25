using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureSensorService
{
    public class MqttMessage
    {
        public int applicationID { get; set; }
        public string applicationName { get; set; }
        public string deviceName { get; set; }
        public string devEUI { get; set; }
        public MqttMessageTxInfo txInfo { get; set; }
        public int fCnt { get; set; }
        public int fPort { get; set; }
        public string data { get; set; }
        [JsonProperty("object")]
        public MqttMessageObject Object { get; set; }
    }

    public class MqttMessageTxInfo
    {
        public string frequency { get; set; }
        public MqttMessageDataRate dataRate { get; set; }
        public string adr { get; set; }
        public string codeRate { get; set; }
    }

    public class MqttMessageDataRate
    {
        public string modulation { get; set; }
        public int bandwidth { get; set; }
        public int spreadFactor { get; set; }
    }

    public class MqttMessageObject
    {
        public int Humidity { get; set; }
        public int Temperature { get; set; }
    }
}
