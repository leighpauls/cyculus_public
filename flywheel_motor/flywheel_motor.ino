#include "encoder.h"

const int POT_PIN = A0;
const int RESISTANCE_PWM_PIN = 9;

void setup() {
  setupEncoder();
  pinMode(RESISTANCE_PWM_PIN, OUTPUT);
  setupSerial();
}

void setFlywheelResistance(double forceNewtons) {
  double forceTicks = forceNewtons * -1;
  int outputTicks = forceTicks;
  outputTicks = min(255, max(0, outputTicks));
  analogWrite(RESISTANCE_PWM_PIN, outputTicks);
}

static double sPrevTransmitTime = -1;
const double TRANSMIT_PERIOD = 0.02;
static unsigned long sPrevEncoderPosition = 0;

void loop() {
  double time = micros() * 1e-6;
  if (time - sPrevTransmitTime > TRANSMIT_PERIOD) {
    int potTicks = analogRead(POT_PIN);
    //medium = 463, 90 degrees = 363 in either direction, low is right
    double handlebarAngleDeg = -(potTicks - 463) * 90.0 / 363.0;
    
    double deltaTime = time - sPrevTransmitTime;
    long curEncoderPosition = getCount();
    long deltaPositionTicks = curEncoderPosition - sPrevEncoderPosition;
    sPrevEncoderPosition = curEncoderPosition;
    double deltaPositionMeters = ((double)deltaPositionTicks) / 200.0 * (2.0 / 12.0) / 4.0 * 3.14 * 0.5;
    
    double velocity = deltaPositionMeters / deltaTime;
    
    Serial.print("controls ");
    Serial.print(handlebarAngleDeg);
    Serial.print(" ");
    Serial.print(velocity);
    Serial.println();
 
    sPrevTransmitTime = time;
  }
 
 processSerialInput();
}
