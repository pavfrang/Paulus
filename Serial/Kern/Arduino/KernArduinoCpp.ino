#define V1 2
#define V2 4
#define V3 6
#define V4 8
#define V5 10
#define V6 12
#define R1 13

bool ignoreCommands;

void setup()
{
	Serial.begin(9600);
	pinMode(V1, OUTPUT);    pinMode(V2, OUTPUT);    pinMode(V3, OUTPUT);    pinMode(V4, OUTPUT);    pinMode(V5, OUTPUT);    pinMode(V6, OUTPUT);    pinMode(R1, OUTPUT);
	ignoreCommands = false;
}

void measure()
{
	digitalWrite(V1, HIGH);
	digitalWrite(V2, LOW);
	digitalWrite(V3, LOW);
	digitalWrite(V4, LOW);
	digitalWrite(V5, LOW);
	digitalWrite(V6, LOW);
}
void check()
{

	for (int i = 1;i <= 3;i++)
	{
		digitalWrite(V1, HIGH);       digitalWrite(V2, LOW);      delay(100);
		digitalWrite(V2, HIGH);       digitalWrite(V5, LOW);      delay(100);
		digitalWrite(V5, HIGH);       digitalWrite(V3, LOW);      delay(100);
		digitalWrite(V3, HIGH);       digitalWrite(V4, LOW);      delay(100);
		digitalWrite(V4, HIGH);       digitalWrite(V6, LOW);      delay(100);
		digitalWrite(V6, HIGH);       digitalWrite(V1, LOW);      delay(100);
	}
}


void loop()
{

	//    if(millis() % 1000)
	//    {
	//      int value = analogRead(A0);
	//      if(value/1023.0*100.0 >60.0)
	//      {
	//        digitalWrite(R1,LOW);
	//        measure();
	//        ignoreCommands = true;
	//        return;
	//      }
	//      Serial.print(value);
	//      Serial.print("\n");
	//    }
	delay(1000);
	int value = analogRead(0);
	double temperature = 100.0*value / 1023 * (0.5 / 0.357);
	Serial.println(temperature);
	//Serial.print("\n");

	if (ignoreCommands)
		return;

	String command;
	if (Serial.available() > 0)
	{
		command = Serial.readStringUntil('\n');
		if (command == "O")
		{
			digitalWrite(V1, HIGH);
			digitalWrite(V2, LOW);
			digitalWrite(V3, HIGH);
			digitalWrite(V4, LOW);
			digitalWrite(V5, HIGH);
			digitalWrite(V6, HIGH);
		}
		else if (command == "U")
		{
			digitalWrite(V1, LOW);
			digitalWrite(V2, HIGH);
			digitalWrite(V3, LOW);
			digitalWrite(V4, HIGH);
			digitalWrite(V5, LOW);
			digitalWrite(V6, LOW);
		}
		else if (command == "CL")
		{
			digitalWrite(V1, HIGH);
			digitalWrite(V2, HIGH);
			digitalWrite(V3, LOW);
			digitalWrite(V4, LOW);
			digitalWrite(V5, LOW);
			digitalWrite(V6, LOW);
		}
		else if (command == "M")
			measure();
		else if (command == "K")
			check();
		else if (command == "C")
		{
			digitalWrite(V1, LOW);
			digitalWrite(V2, LOW);
			digitalWrite(V3, LOW);
			digitalWrite(V4, LOW);
			digitalWrite(V5, LOW);
			digitalWrite(V6, LOW);
		}
		else if (command == "P6")
		{
			digitalWrite(V1, LOW);
			digitalWrite(V2, LOW);
			digitalWrite(V3, LOW);
			digitalWrite(V4, LOW);
			digitalWrite(V5, LOW);
			digitalWrite(V6, HIGH);
		}
		else if (command == "R")
			digitalWrite(R1, HIGH); //ANAPSE TAINIA
		else if (command == "T")
			digitalWrite(R1, LOW); //SBHSE TAINIA
								   // else if(command=="H")


	}
}