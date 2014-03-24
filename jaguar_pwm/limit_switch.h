#pragma once

#define SWITCH_UNPLUGGED -1
#define SWITCH_OPEN 0
#define SWITCH_CLOSED 1

void setupLimitSwitch();

int getLowerLimitState();
