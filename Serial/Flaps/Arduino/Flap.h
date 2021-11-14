/*
  Flap.h - Library for managing exhaust flaps.
  Created by Pavlos E. Fragkiadoulakis, July 31, 2016.
  Released into the public domain.
  */
#ifndef Flap_h
#define Flap_h

#include <EEPROM.h>
#include "Arduino.h"
#include <stdarg.h> //va_list, va_start, va_arg, va_end
class Flap
{
private:
	uint8_t pinRead, pinWrite;
	int baseMemoryAddress;

	float calibratedOutput; //was vA, vB, vC
	float minimumVoltage;
	float maximumVoltage;

	void initializePins()
	{
		//no need for analogRead
		//pinMode(pinRead, INPUT);

		pinMode(pinWrite, OUTPUT);
	}

	void readFromMemoryAndSendLastVoltage()
	{
		EEPROM.get(baseMemoryAddress, calibratedOutput);
		calibratedOutput = constrain(calibratedOutput, 26, 255);

		analogWrite(pinWrite, calibratedOutput); //calibrated outputs

		EEPROM.get(baseMemoryAddress + 5, minimumVoltage);
		minimumVoltage = constrain(minimumVoltage, 0, 10);

		EEPROM.get(baseMemoryAddress + 10, maximumVoltage);
		maximumVoltage = constrain(maximumVoltage, 0, 10);
	}

public:
	Flap(uint8_t pinRead, uint8_t pinWrite, int baseMemoryAddress)
		:pinRead(pinRead), pinWrite(pinWrite), baseMemoryAddress(baseMemoryAddress)
	{	}

	void initialize()
	{
		initializePins();

		readFromMemoryAndSendLastVoltage();
	}

	//Calibrates the minimumVoltage, maximumVoltage internal variables needed in order to set the correct voltage.
	void calibrate()
	{
		Serial.println("[Status] Flap calibration started...");

		//minimum voltage input for the npn transistor (max output of npn)
		//26 corresponds to 10 V transistor output  (0 corresponds to 12 V transistor output)
		//max transistor output corresponds to Klapeto Fully Open
		analogWrite(pinWrite, 26);
		delay(15000);
		maximumVoltage = readVoltage();

		//maximum voltage input for the npn transistor (min output of npn)
		//255 corresponds to 0 V transistor output
		//min transistor output corresponds to Klapeto Fully Closed
		analogWrite(pinWrite, 255);
		delay(15000);
		minimumVoltage = readVoltage();

		saveStateToMemory();

		Serial.println("[Status] Flap calibration finished");
	}

	/*
	//Calibrates the minimumVoltage, maximumVoltage internal variables needed in order to set the correct voltage.
	static void calibrate(int n, ...)
	{
		Serial.println("[Status] Flap calibration started...");

		va_list vl;

		//set the maximum voltage
		va_start(vl, n);
		for (int i = 0;i < n;i++)
		{
			Flap f = va_arg(vl, Flap);
			analogWrite(f.pinWrite, 26);
		}
		va_end(vl);

		delay(15000);

		va_start(vl, n);
		for (int i = 0;i < n;i++)
		{
			Flap f = va_arg(vl, Flap);
			f.maximumVoltage = f.readVoltage();
		}
		va_end(vl);

		//set the minimum voltage
		va_start(vl, n);
		for (int i = 0;i < n;i++)
		{
			Flap f = va_arg(vl, Flap);
			analogWrite(f.pinWrite, 255);
		}
		va_end(vl);

		delay(15000);

		va_start(vl, n);
		for (int i = 0;i < n;i++)
		{
			Flap f = va_arg(vl, Flap);
			f.minimumVoltage = f.readVoltage();
			f.saveStateToMemory();
		}
		va_end(vl);

		Serial.println("[Status] Flap calibration finished");
	}
	*/

	//Calibrates the minimumVoltage, maximumVoltage internal variables needed in order to set the correct voltage.
	static void calibrate(int n, Flap flaps[])
	{
		Serial.println("[Status] Flap calibration started...");

		//set the maximum voltage
		for (int i = 0;i < n;i++)
			analogWrite(flaps[i].pinWrite, 26);
		delay(15000);

		for (int i = 0;i < n;i++)
			flaps[i].maximumVoltage = flaps[i].readVoltage();

		//set the minimum voltage
		for (int i = 0;i < n;i++)
			analogWrite(flaps[i].pinWrite, 255);
		delay(15000);

		for (int i = 0;i < n;i++)
		{
			flaps[i].minimumVoltage = flaps[i].readVoltage();
			flaps[i].saveStateToMemory();
		}

		Serial.println("[Status] Flap calibration finished");
	}


	//This must be used during the Setup.
	void setMinimumVoltage(float voltage)
	{
		minimumVoltage = voltage;
	}

	//Returns the minimum voltage output of the transistor.
	float getMinimumVoltage()
	{
		return minimumVoltage;
	}

	//This must be used during the Setup.
	void setMaximumVoltage(float voltage)
	{
		maximumVoltage = voltage;
	}

	//Returns the maximum voltage output of the transistor.
	float getMaximumVoltage()
	{
		return maximumVoltage;
	}

	//------------------------

	//Saves the flap state to Arduino memory.
	void saveStateToMemory()
	{
		EEPROM.put(baseMemoryAddress, calibratedOutput);
		EEPROM.put(baseMemoryAddress + 5, minimumVoltage);
		EEPROM.put(baseMemoryAddress + 10, maximumVoltage);
	}

	//Sets the position of the flap [0-100].
	void setPosition(float position)
	{
		//get the corresponding voltage (0-10V) based on position (0-100)
		float voltage = position* (maximumVoltage - minimumVoltage) / 100.0 + minimumVoltage;
		voltage = constrain(voltage, 0, 10);

		//get the calibrated output based on experimental curve
		calibratedOutput = 0.410484*voltage*voltage - 27.4277*voltage + 257.259 + 0.5;
		calibratedOutput = constrain(calibratedOutput, 26, 255);

		analogWrite(pinWrite, calibratedOutput);
	}

	//Returns a value in the range [0-10V].
	float readVoltage()
	{
		const int iterations = 10;

		int s = 0;

		for (int i = 0;i < iterations;i++)
			s += analogRead(pinRead);

		return (float)s / (float)iterations / 102.3f;//  = 10.0f/1023.0f;
	}

	//Returns the position of the flap in the range [0-100].
	float readPosition()
	{
		float voltage = readVoltage();
		return 100.0f*(voltage - minimumVoltage) / (maximumVoltage - minimumVoltage);
	}

	//Prints the Voltage, ReadPosition, MinimumVoltage and MaximumVoltage to the serial port.
	void print()
	{
		float voltage = readVoltage();
		float readPosition = 100.0f*(voltage - minimumVoltage) / (maximumVoltage - minimumVoltage);
		Serial.print(voltage); Serial.print(',');
		Serial.print(readPosition); Serial.print(',');
		Serial.print(minimumVoltage);Serial.print(',');
		Serial.print(maximumVoltage);
	}
};

#endif