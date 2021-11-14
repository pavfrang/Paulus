//Commands:
//START REGEN 1
//STOP REGEN 1
//START MEASURE 1
//STOP MEASURE 1
//START REGEN 2
//STOP REGEN 2
//START MEASURE 2
//STOP MEASURE 2
//RESET

#include "DigitalPin\DigitalPin.h"

//In order for the relays to go to their default state the signal must be on.
DigitalPin
regen[2] = { DigitalPin{ 2,HIGH }, DigitalPin{ 5,HIGH } },//NO
power[2] = { DigitalPin{ 3,HIGH }, DigitalPin{ 6,HIGH } }, //NC
measure[2] = { DigitalPin{ 4,HIGH }, DigitalPin{ 7,HIGH } }; //NO

void setup()
{
	Serial.begin(9600);
	while (!Serial); //wait for serial to connect
	Serial.println("Welcome to the LAT Boschino by Paul, Elias, George");
	Serial.println("----------------------------------------------");
}

void loop()
{
	if (Serial.available())
	{
		String command = Serial.readStringUntil('\n');

		if (command.equalsIgnoreCase("START REGEN 1"))
			regen[0].setValue(PinValue::Low);
		else if (command.equalsIgnoreCase("START REGEN 2"))
			regen[1].setValue(PinValue::Low);
		else if (command.equalsIgnoreCase("STOP REGEN 1"))
			regen[0].setValue(PinValue::High);
		else if (command.equalsIgnoreCase("STOP REGEN 2"))
			regen[1].setValue(PinValue::High);
		else if (command.equalsIgnoreCase("START MEASURE 1"))
		{
			power[0].setValue(PinValue::Low); measure[0].setValue(PinValue::Low);
		}
		else if (command.equalsIgnoreCase("START MEASURE 2"))
		{
			power[1].setValue(PinValue::Low); measure[1].setValue(PinValue::Low);
		}
		else if (command.equalsIgnoreCase("STOP MEASURE 1"))
		{
			power[0].setValue(PinValue::High); measure[0].setValue(PinValue::High);
		}
		else if (command.equalsIgnoreCase("STOP MEASURE 2"))
		{
			power[1].setValue(PinValue::High); measure[1].setValue(PinValue::High);
		}
		else if (command.equalsIgnoreCase("RESET"))
			ResetPin::reset(12);
		else
			Serial.println("[ERROR] Unknown command.");
	}
}
