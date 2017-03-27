/*
* Copyright (c) 2015, Majenko Technologies
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without modification,
* are permitted provided that the following conditions are met:
*
* * Redistributions of source code must retain the above copyright notice, this
*   list of conditions and the following disclaimer.
*
* * Redistributions in binary form must reproduce the above copyright notice, this
*   list of conditions and the following disclaimer in the documentation and/or
*   other materials provided with the distribution.
*
* * Neither the name of Majenko Technologies nor the names of its
*   contributors may be used to endorse or promote products derived from
*   this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
* ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
* ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

/* Create a WiFi access point and provide a web server on it. */

#include <ESP8266WiFi.h>
#include <WiFiClient.h> 
#include <ESP8266WebServer.h>
#include <EEPROM\EEPROM.h>

/* Set these to your desired credentials. */
const char *ssid = "wind.power";
const char *password = "test_123";

String deviceId = "a06a2ed4-ea12-420c-9c50-bb01266c981c";
String deviceName = "Wind Power";

#define pwmPin D1
int pwmFrequency;
int pwmDuty;
int modulationFrequency;
int modulationDuty;

bool isTurnOn;

ESP8266WebServer server(80);

/* Just a little test message.  Go to http://192.168.4.1 in a web browser
* connected to this access point to see it.
*/
void handleRoot() {
	server.send(200, "text/html", "<h1>You are connected</h1>");
}

void handleMainSwitch() {
	if (server.method() == HTTP_GET)
	{
		for (uint8_t i = 0; i<server.args(); i++) {
			DoCommand(server.argName(i), server.arg(i));
			Serial.println("cmd: " + server.argName(i) + "=" + server.arg(i));
		}
		SendState();
		return;
	}

	if (server.method() == HTTP_POST)
	{
		server.send(200, "text/html", "<h1>not implemented yet</h1>");
		return;
	}

}

void SendState()
{
	String isTurnOnString = isTurnOn == true ? "true" : "false";
	String message = "{'isTurnOn':" + isTurnOnString;

	message += ", 'pwmFrequency':" + String(pwmFrequency);

	message += ", 'pwmDuty':" + String(pwmDuty);
	
	message += "}";

	server.send(200, "application/json", message);
}

void handlePWM()
{
	if (server.method() != HTTP_GET)
		return;
	for (uint8_t i = 0; i<server.args(); i++) {
		DoPwmCommand(server.argName(i), server.arg(i));
	}
	ApplyPwmSettings();
	SendState();
}

void DoPwmCommand(String cmd, String value)
{
	int v = value.toInt();
	if (v <= 0)
		v = 1;
	if (cmd == "frequency")
	{
		pwmFrequency = v;		
	}
	else if (cmd == "duty")
	{
		pwmDuty = v;		
	}
	else if (cmd == "modFrequency")
	{
		modulationFrequency = v;
	}
	else if (cmd == "modDuty")
	{
		modulationDuty = v;
	}

}

void handleDeviceList()
{
	server.send(200, "application/json", "[{\"id\":\"" + deviceId + "\", \"name\":\"" + deviceName + "\", \"isOn\":\"" + isTurnOn + "\"}]");
}

void DoCommand(String cmd, String value)
{
	if (cmd == "turnOn")
	{
		if (value == "true")
			isTurnOn = true;
		else
			isTurnOn = false;

		UpdatePins();
	}

}

void UpdatePins()
{
	if (isTurnOn)
	{
		digitalWrite(LED_BUILTIN, LOW);
	}
	else
	{
		digitalWrite(LED_BUILTIN, HIGH);
	}
}

void setup() {
	pinMode(LED_BUILTIN, OUTPUT);
	UpdatePins();
	delay(1000);
	Serial.begin(115200);
	Serial.println();
	Serial.print("Configuring access point...");
	/* You can remove the password parameter if you want the AP to be open. */
	WiFi.softAP(ssid, password);

	IPAddress myIP = WiFi.softAPIP();
	Serial.print("AP IP address: ");
	Serial.println(myIP);

	server.on("/", handleRoot);
	server.on("/mainSwitch", handleMainSwitch);
	server.on("/pwm", handlePWM);
	server.on("/equipmentList", handleDeviceList);

	server.begin();
	Serial.println("HTTP server started");

	EEPROM.begin(8);

	InitPWM();
}

void loop() {
	server.handleClient();	

}

void InitPWM()
{
	PwmEepromRead();
	if (pwmFrequency == 0)
		pwmFrequency = 1000;
	pinMode(D1, OUTPUT);
	ApplyPwmSettings();
}

void ApplyPwmSettings()
{
	analogWriteFreq(pwmFrequency);
	analogWrite(D1, pwmDuty);
	PwmEepromWrite();
}

void PwmEepromWrite()
{
	EEPROMWriteInt(0, pwmFrequency);
	EEPROMWriteInt(2, pwmDuty);
	EEPROMWriteInt(4, modulationFrequency);
	EEPROMWriteInt(6, modulationDuty);

	

	EEPROM.commit();
}

void PwmEepromRead()
{
	pwmFrequency = EEPROMReadInt(0);
	pwmDuty = EEPROMReadInt(2);
	modulationFrequency = EEPROMReadInt(4);
	modulationDuty = EEPROMReadInt(6);
}

//This function will write a 2 byte integer to the eeprom at the specified address and address + 1
void EEPROMWriteInt(int p_address, int p_value)
{
	byte lowByte = ((p_value >> 0) & 0xFF);
	byte highByte = ((p_value >> 8) & 0xFF);

	EEPROM.write(p_address, lowByte);
	EEPROM.write(p_address + 1, highByte);
}

//This function will read a 2 byte integer from the eeprom at the specified address and address + 1
unsigned int EEPROMReadInt(int p_address)
{
	byte lowByte = EEPROM.read(p_address);
	byte highByte = EEPROM.read(p_address + 1);

	return ((lowByte << 0) & 0xFF) + ((highByte << 8) & 0xFF00);
}
