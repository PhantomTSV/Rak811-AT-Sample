
#include <DHT.h>
#include "RAK811.h"
#include "SoftwareSerial.h"
#define WORK_MODE LoRaWAN   //  LoRaWAN or LoRaP2P
#define JOIN_MODE ABP    //  OTAA or ABP
#if JOIN_MODE == OTAA
String DevEui = "60C5A8FFFE000001";
String AppEui = "70B3D57EF00047C0";
String AppKey = "5D833B4696D5E01E2F8DC880E30BA5FE";
#else JOIN_MODE == ABP
String NwkSKey = "1DD9F788D6F7FEBD56CC46550F174DCD";
String AppSKey = "FD580689139A6C74A95C2D86B51592AD";
String DevAddr = "0000B003";
#endif
#define TXpin 11   // Set the virtual serial port pins
#define RXpin 10
#define ATSerial Serial
SoftwareSerial DebugSerial(RXpin, TXpin);   // Declare a virtual serial port
char* buffer = "000000000000007F0000000000000000";

RAK811 RAKLoRa(ATSerial);

#define DHTPIN 2
#define DHTTYPE DHT11  

DHT dht(DHTPIN, DHTTYPE);

void setup() {
  DebugSerial.begin(115200);
  while (DebugSerial.read() >= 0) {}
  while (!DebugSerial);
  DebugSerial.println("StartUP");

  ATSerial.begin(114000);

   dht.begin();

}

String to_hex(int a) {
  String str = String(a, HEX);
  str.toUpperCase();

  if (str.length() < 2)
    str = "0" + str;

    return str;
}

void loop() {

  float h = dht.readHumidity();
  float t = dht.readTemperature();

  if (isnan(t) || isnan(h)) {
    DebugSerial.println("Failed to read from DHT");
  } else {
    String tmp = "0x";
    DebugSerial.print("Humidity: "); 
    tmp = tmp + to_hex(int(h));
    DebugSerial.print(tmp);
    DebugSerial.print(" %\t");

    DebugSerial.print(h);
    DebugSerial.print(" %\t");
    
    DebugSerial.print("Temperature: "); 

    DebugSerial.print("0x" + to_hex(int(t)));
    DebugSerial.print(" *C\t");
    
    DebugSerial.print(t);
    DebugSerial.println(" *C");
  }

  DebugSerial.println(RAKLoRa.rk_getVersion());
  delay(500);
  DebugSerial.println(RAKLoRa.rk_getBand());
  delay(500);
  if (RAKLoRa.rk_setWorkingMode(WORK_MODE))
  {
    DebugSerial.println("you set Working mode is OK!");
    if (RAKLoRa.rk_initABP(DevAddr, NwkSKey, AppSKey))
    {
      DebugSerial.println("you init ABP parameter is OK!");
      if (RAKLoRa.rk_joinLoRaNetwork(JOIN_MODE))
      {
        DebugSerial.println("you join Network is OK!");
        ATSerial.setTimeout(8000);
        //String ver = ATSerial.readStringUntil('\n');
        //DebugSerial.println(ver);
        ATSerial.setTimeout(20000);
        //if (ver == STATUS_JOINED_SUCCESS)
        // {
        //while (RAKLoRa.rk_sendData(1,2,buffer))
        // {

        String b = to_hex(int(t)) + to_hex(int(h));
        
        RAKLoRa.rk_sendData(1, 2, b.c_str());
        DebugSerial.println(RAKLoRa.rk_recvData());
        // }
      }
    }
  }

  delay(60000);
}
