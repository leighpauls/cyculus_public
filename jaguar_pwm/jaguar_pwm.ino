#include "jaguar_pwm.h"
#include "pid_control.h"
#include "pwm_control.h"
#include "limit_switch.h"
#include "encoder.h"
#include "serial_cmd.h"

#define STATE_WAITING 0
#define STATE_FEED_DOWN 2
#define STATE_CONTROLLING 3

static int sProgramState;

// start with a very small allowed limit area
static double sLowerLimitPosition = 200;
static const double LIMIT_PADDING = 10;
static const double ZERO_TO_LOWER_LIMIT = 230;
static const double MAX_RANGE_RADIUS = 180;

const double SET_POINT_ALPHA = 0.05;
static double sFilteredSetPoint = 0;
static double sSetPoint = 0;
boolean setSetPoint(double newSetPoint) {
  if (sProgramState != STATE_CONTROLLING) {
    return false;
  }
  sSetPoint = sLowerLimitPosition
    - ZERO_TO_LOWER_LIMIT
    + max(-MAX_RANGE_RADIUS, min(MAX_RANGE_RADIUS, newSetPoint));
  return true;
}


void cmdTryFeedDown() {
  if (sProgramState == STATE_CONTROLLING) {
    return;
  }
  sProgramState = STATE_FEED_DOWN;
}

void cmdStartControl() {
  sProgramState = STATE_CONTROLLING;
  sSetPoint = sFilteredSetPoint = getCount();
}

void cmdStop() {
  sProgramState = STATE_WAITING;
}

void setup() {
  setupEncoder();
  setupJaguar();
  setupLimitSwitch();
  setupSerial();
  sProgramState = STATE_WAITING;
}

static double sPrevUpdateTime = -1.0;
const double UPDATE_PERIOD = 0.005;

void loop() {
  switch (sProgramState) {
    case STATE_WAITING:
      writeJaguar(0.0);
      break;
    case STATE_FEED_DOWN:
      writeJaguar(-0.15);
      if (getLowerLimitState() != SWITCH_OPEN) {
        sLowerLimitPosition = getCount();
        sProgramState = STATE_WAITING;
        writeJaguar(0.0);
      }
      break;
    case STATE_CONTROLLING: {
      double time = micros() * 1e-6;
      if (time - sPrevUpdateTime > UPDATE_PERIOD) {
        sFilteredSetPoint += (sSetPoint - sFilteredSetPoint) * SET_POINT_ALPHA;
        double error = sFilteredSetPoint - getCount();
        double output = doPidControl(error, time);
        writeJaguar(output);
        sPrevUpdateTime = time;
      }
      break;
    }
  }
  processSerialInput();
}


