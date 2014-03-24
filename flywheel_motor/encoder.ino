#include "encoder.h"
#include "Arduino.h"

static const byte PIN_A = 3;
static const byte PIN_B = 2;
static int sPinASet = 0;
static int sPinBSet = 0;
static unsigned long sCount = 0;

static void pinAChange() {
  sPinASet = digitalRead(PIN_A) == HIGH;
  sCount += (sPinASet != sPinBSet) ? 1 : -1;
}
static void pinBChange() {
  sPinBSet = digitalRead(PIN_B) == HIGH;
  sCount += (sPinASet == sPinBSet) ? 1 : -1;
}

void setupEncoder() {
  // encoder init
  pinMode(PIN_A, INPUT);
  digitalWrite(PIN_A, HIGH);
  pinMode(PIN_B, INPUT);
  digitalWrite(PIN_B, HIGH);

  sPinASet = digitalRead(PIN_A);
  sPinBSet = digitalRead(PIN_B);

  attachInterrupt(0, pinAChange, CHANGE);
  attachInterrupt(1, pinBChange, CHANGE);
}

long getCount() {
  return sCount;
}
