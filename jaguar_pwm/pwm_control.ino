#include "pwm_control.h"
#include "Arduino.h"
#include <Servo.h>

static Servo sJagServo;
#define JAG_REVERSE_FULL_US 690
#define JAG_REVERSE_DEADZONE_US 1445
#define JAG_FORWARD_DEADZONE_US 1545
#define JAG_FORWARD_FULL_US 2300
#define JAG_NEUTRAL_US ((JAG_REVERSE_DEADZONE_US + JAG_FORWARD_DEADZONE_US) / 2)

// #define MAX_POWER 0.8 // Don't ever pass 80% (could damage motor)
#define MAX_POWER 0.7

void setupJaguar() {
  sJagServo.attach(5, JAG_REVERSE_FULL_US, JAG_FORWARD_FULL_US);
  writeJaguar(0.0);
}

// Jaguar control
void writeJaguar(double powerNormalized) {
  powerNormalized = max(-MAX_POWER, min(MAX_POWER, powerNormalized));

  // Smoothen out the deadband
  int outputUs;
  if (powerNormalized == 0.0) {
    outputUs = JAG_NEUTRAL_US;
  } else if (powerNormalized > 0.0) {
    outputUs = JAG_FORWARD_DEADZONE_US
        + (int)(powerNormalized
                * (JAG_FORWARD_FULL_US - JAG_FORWARD_DEADZONE_US));
  } else {
    outputUs = JAG_REVERSE_DEADZONE_US
        + (int)(powerNormalized
                * (JAG_REVERSE_DEADZONE_US - JAG_REVERSE_FULL_US));
  }
  sJagServo.writeMicroseconds(outputUs);
}
