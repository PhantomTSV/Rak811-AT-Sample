#include "RAK811.h"
#include "SoftwareSerial.h"
#define WORK_MODE LoRaWAN   //  LoRaWAN or LoRaP2P
#define JOIN_MODE OTAA    //  OTAA or ABP
#if JOIN_MODE == OTAA
String DevEui = "60C5A8FFFE000001";
String AppEui = "0000000000000001";
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
char* buffer = "0104";

RAK811 RAKLoRa(ATSerial);

unsigned long prev_time = 0;
unsigned long period = 10ul * 60ul * 1000ul;

bool isJoin = false;

void setup() {
  DebugSerial.begin(115200);
  while (DebugSerial.read() >= 0) {}
  while (!DebugSerial);
  DebugSerial.println("StartUP");

  ATSerial.begin(114200);

  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(5, OUTPUT);

  DebugSerial.println(RAKLoRa.rk_getVersion());
  delay(500);
  DebugSerial.println(RAKLoRa.rk_getBand());
  delay(500);
  RAKLoRa.rk_setConfig("class", "2");

}

int hex2int(String s) {
  s.toUpperCase();
  if (s.length() < 2) {
    s = "0x0" + s;
  } else {
    s = "0x" + s;
  }

  return strtol(s.c_str(), NULL, 0);
}

void parsePayload(String payload) {
  payload.replace("at+recv=", "");

  int index = payload.indexOf(",");
  int confirmed = hex2int(payload.substring(0, index));  
  int last_index = index + 1;

  index = payload.indexOf(",", last_index);
  int port = hex2int(payload.substring(last_index , index));  
  last_index = index + 1;


  index = payload.indexOf(",", last_index);
  int num_bytes = hex2int(payload.substring(last_index, index));  
  last_index = index + 1;

  if (num_bytes > 0) {
    int data = hex2int(payload.substring(last_index));
    
    if (port == 2) {
      if (data & 0x01) {
        digitalWrite(2, HIGH);
      }
      else {
        digitalWrite(2, LOW);
      }

      if (data & 0x02) {
        digitalWrite(3, HIGH);
      }
      else {
        digitalWrite(3, LOW);
      }
    } else if (port == 3) {
      analogWrite(5, data);
    }

  }
}

void loop() {

  if (!isJoin && (millis() - prev_time > 10 * 1000) || isJoin && (millis() - prev_time > period) ) {
    if (RAKLoRa.rk_setWorkingMode(WORK_MODE))
    {
      DebugSerial.println("you set Working mode is OK!");

#if JOIN_MODE == OTAA
      bool initIsOK = RAKLoRa.rk_initOTAA(DevEui, AppEui, AppKey);
#else JOIN_MODE == ABP
      bool initIsOK = RAKLoRa.rk_initABP(DevAddr, NwkSKey, AppSKey);
#endif

      if (initIsOK)
      {
        DebugSerial.println("you init ABP parameter is OK!");
        if (RAKLoRa.rk_joinLoRaNetwork(JOIN_MODE))
        {
          DebugSerial.println("you join Network is OK!");
          ATSerial.setTimeout(8000);
          String ver = ATSerial.readStringUntil('\n');
          DebugSerial.println("1");
          DebugSerial.println(ver);
          ATSerial.setTimeout(2000);
          if (ver.startsWith("at+recv=3,0,0"))
          {
            isJoin = true;
            DebugSerial.println("2");

            delay(5000);
            //while (RAKLoRa.rk_sendData(1,1,buffer))
            RAKLoRa.rk_sendData(1, 2, buffer);
            {
              DebugSerial.println("3");
              String recvData = RAKLoRa.rk_recvData();
              DebugSerial.println(recvData);

              if (recvData.startsWith("at+recv=")) {
                parsePayload(recvData);
              }
            }
          } else {
            isJoin = false;
          }
        }
      }
    }
    prev_time = millis();
  }

  if (ATSerial.available() > 0) {
    DebugSerial.println("4");
    String recvData = RAKLoRa.rk_recvData();
    DebugSerial.println(recvData);

    if (recvData.startsWith("at+recv=")) {
      parsePayload(recvData);
    }
  }  
}
