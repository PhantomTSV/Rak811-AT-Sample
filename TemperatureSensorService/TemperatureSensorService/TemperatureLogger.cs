using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Net;
using System.Threading;

namespace TemperatureSensorService
{
    class TemperatureLogger
    {
        string MQTT_BROKER_ADDRESS = "127.0.0.1";
        string APP_ID = "1";
        MqttClient termometrClient;
        Timer termometrClientReconnectTimer;

        public TemperatureLogger()
        {
            termometrClientReconnectTimer = new Timer(OnTermometrClientReconnectTimer, null, Timeout.Infinite, Timeout.Infinite);         
            termometrClient = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));
            termometrClient.MqttMsgPublishReceived += termometrClient_MqttMsgPublishReceived;
            termometrClient.ConnectionClosed += termometrClient_ConnectionClosed;
            
            try
            {
                string clientId = Guid.NewGuid().ToString();
                termometrClient.Connect(clientId);
                termometrClient.Subscribe(new string[] { "application/" + APP_ID + "/node/+/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            catch
            {
                termometrClientReconnectTimer.Change(10000, 10000);
            }

        }

        private void termometrClient_ConnectionClosed(object sender, EventArgs e)
        {
            termometrClientReconnectTimer.Change(10000, 10000);
        }

        public void OnTermometrClientReconnectTimer(Object stateInfo)
        {
            try
            {
                string clientId = termometrClient.ClientId == null ? Guid.NewGuid().ToString() : termometrClient.ClientId;
                termometrClient.Connect(clientId);
                if (termometrClient.IsConnected)
                {
                    termometrClient.Subscribe(new string[] { "application/" + APP_ID + "/node/+/rx" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                    termometrClientReconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch
            {

            }
        }

        private void termometrClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            String str = System.Text.UTF8Encoding.UTF8.GetString(e.Message);
            var info = JsonConvert.DeserializeObject<MqttMessage>(str);

            if (info.Object != null)
            {
                string Connect = "Database=term;Data Source=127.0.0.1;User Id=root;Password=";
                MySqlConnection myConnection = new MySqlConnection(Connect);

                try
                {
                    myConnection.Open();
                }
                catch
                {
                    return;
                }
                
                string query = "INSERT INTO temp_values (dev_eui, temperature, humidity, log_date) VALUES (@dev_eui, @temperature, @humidity, CURRENT_TIMESTAMP)";
                MySqlCommand command = new MySqlCommand(query, myConnection);
                command.Parameters.AddWithValue("@dev_eui", info.devEUI);
                command.Parameters.AddWithValue("@temperature", info.Object.Temperature);
                command.Parameters.AddWithValue("@humidity", info.Object.Humidity);
                command.ExecuteNonQuery();
                myConnection.Close();
            }
        }    

    }
}
