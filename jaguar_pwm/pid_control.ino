#include "pid_control.h"

// PID control
const double K_P = 0.002;
const double K_I = 0.0;
const double K_D = 0.00001;

double prevTime = -1;
double prevError = 0;
double errorIntegral = 0;

double doPidControl(double error, double time) {
  double output = 0.0;

  output -= K_P * error;

  if (prevTime > 0) {
    double timePassed = time - prevTime;
    errorIntegral += error * timePassed;
    output -= K_I * errorIntegral;

    double errorSpeed = (error - prevError) / timePassed;
    output -= K_D * errorSpeed;
    prevError = error;
  }
  prevTime = time;
  prevError = error;

  return output;
}
