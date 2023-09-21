#include <SoftwareSerial.h>

const int SW = 2, VrX = 0, VrY = 1;

bool s1 = false;
int s2 = 0;
int oldZ = 1;

long time1 = 0, time2 = 0;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);

  pinMode(SW, INPUT_PULLUP);
}

void loop() {
  // put your main code here, to run repeatedly:
  int x = analogRead(VrX);
  int y = analogRead(VrY);
  int z = digitalRead(SW);
  if(z && !oldZ)
    s1 = !s1;
  oldZ = z;

  char *buffer = new char[64];
  sprintf(buffer, "(%d, %d), %d", x, y, s1 ? 1 : 0);

  if(millis() - time2 > 30)
  {
    Serial.println(buffer);
    time2 = millis();
  }

  delete[] buffer;
}
