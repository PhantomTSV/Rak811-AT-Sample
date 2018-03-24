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
using System.Threading;
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
            public int output { get; set; }            
            public int pwm { get; set; }
        }


        string MQTT_BROKER_ADDRESS = "127.0.0.1";
        MqttClient ledControlClient;
        MqttClient termometrClient;
        bool led1 = false;
        bool led2 = false;
        int pwm = 0;

        private void ledControlClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received 

            String str = System.Text.UTF8Encoding.UTF8.GetString(e.Message);
            var info = JsonConvert.DeserializeObject<MqttMsg>(str);   

         
            if (info.Object != null)
            {
                sendLeds();
                Thread.Sleep(1000);
                sendPwm();       

            }
        }

        private void temometrClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received 

            String str = System.Text.UTF8Encoding.UTF8.GetString(e.Message);
            var info = JsonConvert.DeserializeObject<MqttMsg>(str);
            
            if (info.Object != null)
            {           
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

                chart1.Series[0].Points.AddXY( DateTime.Now.Minute,  obj.Temperature);

            }
        }

        void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Debug.WriteLine("MessageId = " + e.MessageId + " Published = " + e.IsPublished);
        }

        public MainForm()
        {
            InitializeComponent();
            string clientId;

            ledControlClient = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));
            ledControlClient.MqttMsgPublishReceived += ledControlClient_MqttMsgPublishReceived;
            ledControlClient.ConnectionClosed += ledControlClient_ConnectionClosed;


            try
            {
                clientId = Guid.NewGuid().ToString();
                ledControlClient.Connect(clientId);
                ledControlClient.Subscribe(new string[] { "application/2/node/60c5a8fffe000001/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });     
            }
            catch
            {

            }
           
            
            termometrClient = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));
            termometrClient.MqttMsgPublishReceived += temometrClient_MqttMsgPublishReceived;
            termometrClient.ConnectionClosed += termometrClient_ConnectionClosed;
                     


            try
            {
                clientId = Guid.NewGuid().ToString();
                termometrClient.Connect(clientId);
                termometrClient.Subscribe(new string[] { "application/1/node/3531323975377613/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            catch
            {
                termometrClientReconnectTimer.Start();
            }

            

            pwmDelayTimer.Stop();
        }           


        private void sendLeds()
        {
            MqttSndMsg msg = new MqttSndMsg();

            msg.reference = "123";
            msg.confirmed = false;
            msg.devEUI = "60c5a8fffe000001";
            msg.fPort = 2;
            msg.Object = new MqttSndMsgObject();           
            msg.Object.output = ((led2 ? 1 : 0) << 1) | ((led1 ? 1 : 0) << 0);
            


            string str1 = JsonConvert.SerializeObject(msg); 


            ushort msgId = ledControlClient.Publish("application/2/node/60c5a8fffe000001/tx", // topic
                              Encoding.UTF8.GetBytes(str1), // message body
                              MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
                              false); // retained
        }

        private void sendPwm()
        {
            MqttSndMsg msg = new MqttSndMsg();

            msg.reference = "123";
            msg.confirmed = false;
            msg.devEUI = "60c5a8fffe000001";
            msg.fPort = 3;
            msg.Object = new MqttSndMsgObject();       
            msg.Object.pwm = pwm;

            string str1 = JsonConvert.SerializeObject(msg);


            ushort msgId = ledControlClient.Publish("application/2/node/60c5a8fffe000001/tx", // topic
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
            ledControlClient.ConnectionClosed -= ledControlClient_ConnectionClosed;
            if (ledControlClient.IsConnected)
                ledControlClient.Disconnect();

           termometrClient.ConnectionClosed -= termometrClient_ConnectionClosed;
            if (termometrClient.IsConnected)
                termometrClient.Disconnect();
        }

        private void pwm1TrackBar_ValueChanged(object sender, EventArgs e)
        {
            pwm1Label.Text = (pwm1TrackBar.Value / 255.0 * 100.0).ToString("N2") + " %";
            pwm = pwm1TrackBar.Value;

            if (pwmDelayTimer.Enabled)
                pwmDelayTimer.Stop();

            pwmDelayTimer.Start();
        }

        private void pwmDelayTimer_Tick(object sender, EventArgs e)
        {
            pwmDelayTimer.Stop();
            sendPwm();
        }


        delegate void StartTimerCallback();


        void startTermometrClientReconnectTimer()
        {
            termometrClientReconnectTimer.Start();
        }

        void termometrClient_ConnectionClosed(object sender, EventArgs e)
        {
            StartTimerCallback d = new StartTimerCallback(startTermometrClientReconnectTimer);
            this.Invoke(d, new object[] { });
        }

        void startLedControllClientReconnectTimer()
        {
            ledControlClientReconnectTimer.Start();
        }


        void ledControlClient_ConnectionClosed(object sender, EventArgs e)
        {
            StartTimerCallback d = new StartTimerCallback(startLedControllClientReconnectTimer);
            this.Invoke(d, new object[] { });
        }


        private void termometrClientReconnectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string clientId = termometrClient.ClientId == null ? Guid.NewGuid().ToString() : termometrClient.ClientId;
                termometrClient.Connect(clientId);
                if (termometrClient.IsConnected)
                {
                    termometrClient.Subscribe(new string[] { "application/1/node/3531323975377613/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                    termometrClientReconnectTimer.Stop();
                }
            }
            catch
            {

            }
        }

        private void ledControlClientReconnectTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string clientId = ledControlClient.ClientId == null ? Guid.NewGuid().ToString() : ledControlClient.ClientId;
                ledControlClient.Connect(clientId);
                if (ledControlClient.IsConnected)
                {
                    ledControlClient.Subscribe(new string[] { "application/2/node/60c5a8fffe000001/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });     
                    ledControlClientReconnectTimer.Stop();
                }
            }
            catch
            {

            }
        }
    }
}

