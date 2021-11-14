//To initialize and set 0% and 100% type:"j"
//COMMANDS:

//RESET

//FLAPA50.8
//FLAPB60
//FLAPC20
//SETUPA
//SETUPB
//SETUPC
//SETUP
//READALL
//READA
//READB
//READC
//SAVE

#include <Flap/Flap.h>

#define PIN_FLAP_A_READ 1
#define PIN_FLAP_B_READ 2
#define PIN_FLAP_C_READ 3

#define PIN_FLAP_A_WRITE 9
#define PIN_FLAP_B_WRITE 10
#define PIN_FLAP_C_WRITE 11

Flap flapA(PIN_FLAP_A_READ, PIN_FLAP_A_WRITE, 0),
flapB(PIN_FLAP_B_READ, PIN_FLAP_B_WRITE, 20),
flapC(PIN_FLAP_C_READ, PIN_FLAP_C_WRITE, 40);
Flap flaps[] = { flapA,flapB,flapC }; //needed for the the setup function

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
	flapA.initialize();
	flapB.initialize();
	flapC.initialize();

	Serial.println("[Status] Initialized");
}

// the loop function runs over and over again forever
void loop() {
	if (Serial.available() > 0) {
		String command = Serial.readStringUntil('\n');

		if (command.startsWith("FLAPA"))
		{
			float value = command.substring(5).toFloat();
			flapA.setPosition(value);
		}
		else if (command.startsWith("FLAPB")) {
			float value = command.substring(5).toFloat();
			flapB.setPosition(value);
		}
		else if (command.startsWith("FLAPC")) {
			float value = command.substring(5).toFloat();
			flapC.setPosition(value);
		}
		else if (command.startsWith("SETUPA"))
			flapA.calibrate();
		else if (command.startsWith("SETUPB"))
			flapB.calibrate();
		else if (command.startsWith("SETUPC"))
			flapC.calibrate();
		else if (command.startsWith("SETUP"))
			Flap::calibrate(3, flaps);
		else if (command.startsWith("RESET"))
			resetArduino(12);
		else if (command.startsWith("READALL")) {
			Serial.print("[Read A]"); flapA.print(); Serial.print('|');
			Serial.print("[Read B]"); flapB.print(); Serial.print('|');
			Serial.print("[Read C]"); flapC.print(); Serial.println();
		}
		else if (command.startsWith("READA")) {
			Serial.print("[Read A]"); flapA.print(); Serial.println();
		}
		else if (command.startsWith("READB")) {
			Serial.print("[Read B]"); flapB.print(); Serial.println();
		}
		else if (command.startsWith("READC")) {
			Serial.print("[Read C]");flapB.print(); Serial.println();
		}
		else if (command.startsWith("SAVE"))
		{
			flapA.saveStateToMemory();
			flapB.saveStateToMemory();
			flapC.saveStateToMemory();
			Serial.println("[Status] Saved states to internal memory.");
		}
	}

}

