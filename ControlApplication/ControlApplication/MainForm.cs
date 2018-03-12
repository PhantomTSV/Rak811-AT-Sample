using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;



namespace WindowsFormsApplication1
{
    public partial class MainForm : Form
    {

        public class MqttMsg
        {
            public int applicationID { get; set; }
            public string applicationName { get; set; }
            public string deviceName { get; set; }
            public string devEUI { get; set; }
            public MqttMsgTxInfo txInfo { get; set; }
            public int fCnt { get; set; }
            public int fPort { get; set; }
            public string data { get; set; }
            [JsonProperty("object")]
            public MqttMsgObject Object { get; set; }
        }

        public class MqttMsgTxInfo
        {
            public string frequency { get; set; }
            public MqttMsgDataRate dataRate { get; set; }
            public string adr { get; set; }
            public string codeRate { get; set; }
        }

        public class MqttMsgDataRate
        {
            public string modulation { get; set; }
            public int bandwidth { get; set; }
            public int spreadFactor { get; set; }
        }

        public class MqttMsgObject
        {
            public int Humidity { get; set; }
            public int Temperature { get; set; }
        }

        public class MqttSndMsg
        {
            public string reference { get; set; }
            public string devEUI { get; set; }
            public bool confirmed { get; set; }
            public int fPort { get; set; }
            [JsonProperty("object")]
            public MqttSndMsgObject Object { get; set; }
        }

        public class MqttSndMsgObject
        {
            public int led1 { get; set; }
            public int led2 { get; set; }
        }


        string MQTT_BROKER_ADDRESS = "127.0.0.1";
        MqttClient client;
        bool led1 = false;
        bool led2 = false;

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received 

            int a = 67, b = 22, c;


            String str = System.Text.UTF8Encoding.UTF8.GetString(e.Message);

            var info = JsonConvert.DeserializeObject<MqttMsg>(str);

            c = a + b;

          //  return;

            //labelTemperature.Text = "Температура: " + info.Object.Temperature.ToString();
            //labelHumidity.Text = "Температура: " + info.Object.Humidity.ToString();
            if (info.Object != null)
            {
              //  MqttClient client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

              ////  string clientId = Guid.NewGuid().ToString();
              //  client.Connect(clientId);

             //   client.MqttMsgPublished += client_MqttMsgPublished;

                MqttSndMsg msg = new MqttSndMsg();

                msg.reference = "";
                msg.confirmed = false;
                msg.fPort = 2;
                msg.Object = new MqttSndMsgObject();
                msg.Object.led1 = (led1 ? 1 : 0);
                msg.Object.led2 = (led2 ? 1 : 0);


                string str1 = JsonConvert.SerializeObject(msg);  //"{\"reference\": \"\",\"confirmed\": false,\"fPort\": 2,\"object\":{\"Temperature\":5,\"Humidity\":255}}";


               /* ushort msgId = client.Publish("application/1/node/3037343644357422/tx", // topic
                                  Encoding.UTF8.GetBytes(str1), // message body
                                  MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
                                  false); // retained*/
                
                SetText(info.Object);

            }

        }

        delegate void SetTextCallback(MqttMsgObject obj);

        private void SetText(MqttMsgObject obj)
        {
            if (this.labelHumidity.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { obj });
            }
            else
            {
                labelTemperature.Text = "Температура: " + obj.Temperature.ToString();
                labelHumidity.Text = "Влажность: " + obj.Humidity.ToString();
            }
        }

        void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Debug.WriteLine("MessageId = " + e.MessageId + " Published = " + e.IsPublished);
        }

        public MainForm()
        {
            InitializeComponent();

            // create client instance 
            client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { "application/1/node/3037343644357422/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

         /*   client.MqttMsgPublished += client_MqttMsgPublished;


            string str1 = "{\"reference\": \"\",\"confirmed\": false,\"fPort\": 2,\"data\": \"/68=\"}";


            ushort msgId = client.Publish("application/1/node/3531323975377613/tx", // topic
                              Encoding.UTF8.GetBytes(str1), // message body
                              MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
                              false); // retained

            */
            // labelHumidity



        }

        private void sendLeds()
        {
            MqttSndMsg msg = new MqttSndMsg();

            msg.reference = "123";
            msg.confirmed = false;
            msg.devEUI = "3531323975377613";
            msg.fPort = 2;
            msg.Object = new MqttSndMsgObject();
            msg.Object.led1 = (led1 ? 1 : 0);
            msg.Object.led2 = (led2 ? 1 : 0);


            string str1 = JsonConvert.SerializeObject(msg);  //"{\"reference\": \"123\",\"confirmed\": false,\"fPort\": 2,\"data\":\"Qzc=\"}";


            ushort msgId = client.Publish("application/1/node/3531323975377613/tx", // topic
                              Encoding.UTF8.GetBytes(str1), // message body
                              MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
                              false); // retained
        }

        private void button1_Click(object sender, EventArgs e)
        {
            led1 = !led1;

            if (led1)
            {
                button1.BackColor = Color.Lime;
            }
            else
            {
                button1.BackColor = Color.Gainsboro;
            }

            sendLeds();      
        }

        private void button3_Click(object sender, EventArgs e)
        {
            led2 = !led2;

            if (led2)
            {
                button3.BackColor = Color.Lime;
            }
            else
            {
                button3.BackColor = Color.Gainsboro;
            }

            sendLeds();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Disconnect();
        }
    }
}

