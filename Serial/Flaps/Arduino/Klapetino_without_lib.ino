//To initialize and set 0% and 100% type:"j"
//COMMANDS:
//FLAPA50.8
//FLAPB60
//FLAPC20
//SETUP
//RESET
//READALL
//READA
//READB
//READC

#include <EEPROM.h>

#define PIN_FLAP_A_READ 1
#define PIN_FLAP_B_READ 2
#define PIN_FLAP_C_READ 3

#define PIN_FLAP_A_WRITE 9
#define PIN_FLAP_B_WRITE 10
#define PIN_FLAP_C_WRITE 11

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
	{
		this->pinRead = pinRead;
		this->pinWrite = pinWrite;
		this->baseMemoryAddress = baseMemoryAddress;
	}

	void Initialize()
	{
		initializePins();

		readFromMemoryAndSendLastVoltage();
	}

	//This must be used during the Setup.
	void SetMinimumVoltage(float voltage)
	{
		minimumVoltage = voltage;
	}

	//This must be used during the Setup.
	void SetMaximumVoltage(float voltage)
	{
		maximumVoltage = voltage;
	}

	void SaveStateToMemory()
	{
		EEPROM.put(baseMemoryAddress, calibratedOutput);
		EEPROM.put(baseMemoryAddress + 5, minimumVoltage);
		EEPROM.put(baseMemoryAddress + 10, maximumVoltage);
	}

	//Sets the position of the flap [0-100].
	void SetPosition(float position)
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
	float ReadVoltage()
	{
		const int iterations = 10;

		int s = 0;

		for (int i = 0;i < iterations;i++)
			s += analogRead(pinRead);

		return (float)s / (float)iterations / 102.3f;//  = 10.0f/1023.0f;
	}

	//Returns the position of the flap in the range [0-100].
	float ReadPosition()
	{
		float voltage = ReadVoltage();
		return 100.0f*(voltage - minimumVoltage) / (maximumVoltage - minimumVoltage);
	}

	//Prints the Voltage, ReadPosition, MinimumVoltage and MaximumVoltage to the serial port.
	void Print()
	{
		float voltage = ReadVoltage();
		float readPosition = 100.0f*(voltage - minimumVoltage) / (maximumVoltage - minimumVoltage);
		Serial.print(voltage); Serial.print(',');
		Serial.print(readPosition); Serial.print(',');
		Serial.print(minimumVoltage);Serial.print(',');
		Serial.print(maximumVoltage);
	}
};

Flap flapA(PIN_FLAP_A_READ, PIN_FLAP_A_WRITE, 0),
flapB(PIN_FLAP_B_READ, PIN_FLAP_B_WRITE, 20),
flapC(PIN_FLAP_C_READ, PIN_FLAP_C_WRITE, 40);

void setupFlapLimits()
{
	Serial.println("[Status] Flap calibration started...");

	//minimum voltage input for the npn transistor (max output of npn)
	//26 corresponds to 10 V transistor output  (0 corresponds to 12 V transistor output)
	//max transistor output corresponds to Klapeto Fully Open
	analogWrite(PIN_FLAP_A_WRITE, 26);
	analogWrite(PIN_FLAP_B_WRITE, 26);
	analogWrite(PIN_FLAP_C_WRITE, 26);

	delay(15000);

	flapA.SetMaximumVoltage(flapA.ReadVoltage());
	flapB.SetMaximumVoltage(flapB.ReadVoltage());
	flapC.SetMaximumVoltage(flapC.ReadVoltage());

	//maximum voltage input for the npn transistor (min output of npn)
	//255 corresponds to 0 V transistor output
	//min transistor output corresponds to Klapeto Fully Closed
	analogWrite(PIN_FLAP_A_WRITE, 255);
	analogWrite(PIN_FLAP_B_WRITE, 255);
	analogWrite(PIN_FLAP_C_WRITE, 255);

	delay(15000);

	flapA.SetMinimumVoltage(flapA.ReadVoltage());
	flapB.SetMinimumVoltage(flapB.ReadVoltage());
	flapC.SetMinimumVoltage(flapC.ReadVoltage());

	flapA.SaveStateToMemory();
	flapB.SaveStateToMemory();
	flapC.SaveStateToMemory();

	Serial.println("[Status] Flap calibration finished");
}

void resetArduino(uint8_t pinToSendResetCommand = 12)
{
	pinMode(pinToSendResetCommand, OUTPUT);
	digitalWrite(pinToSendResetCommand, HIGH);
	delay(500);
	digitalWrite(pinToSendResetCommand, LOW);
}

// the setup function runs once when you press reset or power the board
void setup()
{
	//setup serial port
	Serial.begin(9600);

	Serial.println("Welcome to the LAT Klapetino by George and Paul");
	Serial.println("-----------------------------------------------");
	Serial.println("[Status] Initializing...");

	//initialize exhaust flaps
	flapA.Initialize();
	flapB.Initialize();
	flapC.Initialize();

	Serial.println("[Status] Initialized");
}

// the loop function runs over and over again forever
void loop() {
	if (Serial.available() > 0) {
		String command = Serial.readStringUntil('\n');

		if (command.startsWith("FLAPA"))
		{
			float value = command.substring(5).toFloat();
			flapA.SetPosition(value);
		}
		else if (command.startsWith("FLAPB")) {
			float value = command.substring(5).toFloat();
			flapB.SetPosition(value);
		}
		else if (command.startsWith("FLAPC")) {
			float value = command.substring(5).toFloat();
			flapC.SetPosition(value);
		}
		else if (command.startsWith("SETUP"))
			setupFlapLimits();
		else if (command.startsWith("RESET"))
			resetArduino(12);
		else if (command.startsWith("READALL")) {
			Serial.print("[Read A]"); flapA.Print(); Serial.print('|');
			Serial.print("[Read B]"); flapB.Print(); Serial.print('|');
			Serial.print("[Read C]"); flapC.Print(); Serial.println();
		}
		else if (command.startsWith("READA")) {
			Serial.print("[Read A]"); flapA.Print(); Serial.println();
		}
		else if (command.startsWith("READB")) {
			Serial.print("[Read B]"); flapB.Print(); Serial.println();
		}
		else if (command.startsWith("READC")) {
			Serial.print("[Read C]");flapB.Print(); Serial.println();
		}
		else if (command.startsWith("SAVE"))
		{
			flapA.SaveStateToMemory();
			flapB.SaveStateToMemory();
			flapC.SaveStateToMemory();
			Serial.println("[Status] Saved states to internal memory.");
		}
	}

}

