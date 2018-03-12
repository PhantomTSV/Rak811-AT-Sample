

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
SoftwareSerial DebugSerial(RXpin,TXpin);    // Declare a virtual serial port
char* buffer = "0104";

RAK811 RAKLoRa(ATSerial);



void setup() {
 DebugSerial.begin(115200);
 while(DebugSerial.read()>= 0) {}
 while(!DebugSerial);
 DebugSerial.println("StartUP");

 ATSerial.begin(114000);

 pinMode(2, OUTPUT); 
 pinMode(3, OUTPUT); 

}

void loop() {
 
 DebugSerial.println(RAKLoRa.rk_getVersion());
 delay(500);
 DebugSerial.println(RAKLoRa.rk_getBand());
 delay(500);
 if(RAKLoRa.rk_setWorkingMode(WORK_MODE))
 {
  DebugSerial.println("you set Working mode is OK!");
  if (RAKLoRa.rk_initABP(DevAddr,NwkSKey,AppSKey))
    {
      DebugSerial.println("you init ABP parameter is OK!");
      if (RAKLoRa.rk_joinLoRaNetwork(JOIN_MODE))
       {
           DebugSerial.println("you join Network is OK!");
           ATSerial.setTimeout(8000);
           String ver = ATSerial.readStringUntil('\n');
           DebugSerial.println(ver);
           ATSerial.setTimeout(2000);
          // if (ver == STATUS_JOINED_SUCCESS)
           {
            //while (RAKLoRa.rk_sendData(1,1,buffer))
            RAKLoRa.rk_sendData(1,2,buffer);
            {
              String recvData = RAKLoRa.rk_recvData();              
              DebugSerial.println(recvData);

              if (recvData.startsWith("at+recv=")){
                recvData.replace("at+recv=", "");
                String ss = recvData.substring(recvData.length() - 2);       

                DebugSerial.println(ss);

                if (ss.substring(0, 1) == "1") {
                  digitalWrite(2, HIGH);
                  DebugSerial.println("hi1");
                }
                else {
                  digitalWrite(2, LOW);
                  DebugSerial.println("lo1");
                }

                DebugSerial.println(ss.substring(1));
                
                if (ss.substring(1) == "1") {
                  digitalWrite(3, HIGH);
                  DebugSerial.println("hi2");
                }
                else {
                  digitalWrite(3, LOW);
                  DebugSerial.println("lo2");
                }
              
              }
            }
           }
        }
     }
 }
 delay(5000);
}
