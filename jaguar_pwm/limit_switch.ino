#include "limit_switch.h"
#include "Arduino.h"

static const int lowerLimitOpenPin = 10;
static const int lowerLimitClosedPin = 11;

static int getSwitchState(int openPin, int closedPin) {
  boolean openState = digitalRead(openPin);
  boolean closedState = digitalRead(closedPin);
  if (openState == closedState) {
    return SWITCH_UNPLUGGED;
  }
  return openState ? SWITCH_OPEN : SWITCH_CLOSED;
}

int getLowerLimitState() {
  return getSwitchState(lowerLimitOpenPin, lowerLimitClosedPin);
}


void setupLimitSwitch() {
  // limit switch init
  pinMode(lowerLimitOpenPin, INPUT_PULLUP);
  pinMode(lowerLimitClosedPin, INPUT_PULLUP);
}
