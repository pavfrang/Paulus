/*
DigitalPin.h - Library for encapsulating a digital pin.
The class saves the state and mode (value) of a pin.

Created by Pavlos E. Fragkiadoulakis, May 3, 2014.
Released into the public domain.
*/
#ifndef DigitalPin_h
#define DigitalPin_h

#include "Arduino.h"

enum class PinMode : uint8_t
{
	Input = INPUT,
	Output = OUTPUT,
	InputPullup = INPUT_PULLUP
};

enum class PinValue : uint8_t
{
	Low = LOW,
	High = HIGH
};


enum class PinNumber : uint8_t
{
	//PIN0 and PIN1 are used by the serial port (TX/RX)
	Pin2 = PIN2,
	Pin3 = PIN3,
	Pin4 = PIN4,
	Pin5 = PIN5,
	Pin6 = PIN6,
	Pin7 = PIN7,
	Pin8 = 8,
	Pin9 = 9,
	Pin10 = 10,
	Pin11 = 11,
	Pin12 = 12,
	Pin13 = 13 //Arduino UNO pins until here
};

class DigitalPin {
public:
	DigitalPin(PinNumber pin, PinValue initialValue = PinValue::Low, PinMode mode = PinMode::Output, bool reportEnabled = true)
		:_pin(pin), _value(initialValue), _mode(mode), _reportEnabled(reportEnabled)
	{
		pinMode((uint8_t)pin, (uint8_t)mode);
		digitalWrite((uint8_t)pin, (uint8_t)initialValue);
	}

	DigitalPin(uint8_t pin, uint16_t initialValue = LOW, uint8_t mode = OUTPUT, bool reportEnabled = true)
		:_pin((PinNumber)pin), _value((PinValue)initialValue), _mode((PinMode)mode), _reportEnabled(reportEnabled)
	{
		pinMode((uint8_t)pin, (uint8_t)mode);
		digitalWrite((uint8_t)pin, (uint8_t)initialValue);
	}

protected:
	PinNumber _pin;
	PinMode _mode;
	PinValue _value;
	bool _reportEnabled;
public:
	void setReportEnabled(bool value)
	{
		_reportEnabled = value;
	}

	bool getReportEnabled()
	{
		return _reportEnabled;
	}

	//Returns the pin number.
	PinNumber getPin()
	{
		return _pin;
	}

	//Returns the current value of the pin.
	PinValue getValue()
	{
		return _value;
	}

	//Sets a new value for the pin and returns the previous value (optional).
	PinValue setValue(PinValue value)
	{
		PinValue previousValue = _value;
		_value = value;
		digitalWrite((uint8_t)_pin, (uint8_t)_value);

		if (_reportEnabled)
		{
			Serial.print("[COMMAND] Pin "); Serial.print((uint8_t)_pin); Serial.print(" set to "); Serial.println(value == PinValue::High ? "HIGH." : "LOW.");
		}

		return previousValue;
	}

	uint8_t setValue(uint8_t value)
	{
		return (uint8_t)setValue((PinValue)value);
	}

	//Processes commands ON<PIN> or OFF<PIN>. Returns true if it successfully processes the command.
	virtual bool processCommand(String command)
	{
		if (command.startsWith("ON"))
		{
			int pin = command.substring(2).toInt();
			if (pin == (uint8_t)_pin)
			{
				setValue(PinValue::High);
				return true;
			}
		}
		else if (command.startsWith("OFF"))
		{
			int pin = command.substring(3).toInt();
			if (pin == (uint8_t)_pin)
			{
				setValue(PinValue::Low);
				return true;
			}
		}
		return false;
	}
};

class ResetPin
{
private:
	PinNumber _pin;

public:
	ResetPin(uint8_t pin) :_pin((PinNumber)pin) {}
	ResetPin(PinNumber pin) :_pin(pin) {}

	void reset()
	{
		//the pin mode must be set here
		pinMode((uint8_t)_pin, OUTPUT);
		digitalWrite((uint8_t)_pin, HIGH);
		delay(500);
		digitalWrite((uint8_t)_pin, LOW);
	}

	static void reset(uint8_t pin)
	{
		//the pin mode must be set here
		pinMode((uint8_t)pin, OUTPUT);
		digitalWrite((uint8_t)pin, HIGH);
		delay(500);
		digitalWrite((uint8_t)pin, LOW);
	}

	bool processCommand(String command)
	{
		if (command.equalsIgnoreCase("RESET"))
		{
			reset(); return true;
		}
		return false; //we will never get here if the reset is used
	}


};
#endif

