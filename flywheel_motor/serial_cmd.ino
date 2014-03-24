#include "serial_cmd.h"
#include "flywheel_motor.h"

#include "Arduino.h"
#include <Serial.h>
#include <cstring>

static int sRxBufferLength = 0;
#define RX_BUFFER_SIZE 254
static char sRxBuffer[RX_BUFFER_SIZE];

static boolean extractWord(char* dest, const char* wordStart, int maxLen) {
  char* wordEnd = (char*)memchr(wordStart, ' ', maxLen);
  if (wordEnd == NULL) {
    strncpy(dest, wordStart, maxLen-1);
    dest[maxLen-1] = '\0';
    if (strnlen(dest, maxLen) == 0) {
      return false;
    }
    return true;
  }
  unsigned wordLen = (unsigned)wordEnd - (unsigned)wordStart;
  if (wordLen > maxLen - 1 || wordLen <= 0) {
    return false;
  }
  strncpy(dest, wordStart, wordLen);
  dest[wordLen] = '\0';
  return true;
}

#define COMMAND_NAME_MAX_LENGTH 16
#define NUMBER_MAX_LENGTH 16

void handleForceCommand(const char* commandName) {
  char forceValueBuffer[NUMBER_MAX_LENGTH];
  if (!extractWord(forceValueBuffer, sRxBuffer + strlen(commandName) + 1, NUMBER_MAX_LENGTH)) {
    // couldn't read the force
    Serial.println("NACK");
    return;
  }
  double forceNewtons = atof(forceValueBuffer);
  setFlywheelResistance(forceNewtons);
}

static void processSerialBuffer() {
  // find the command name
  char commandName[COMMAND_NAME_MAX_LENGTH];
  if (!extractWord(commandName, sRxBuffer, COMMAND_NAME_MAX_LENGTH)) {
    Serial.println("NACK");
    return;
  }

  // decide what to do based on the name
  if (strcmp("f", commandName) == 0) {
    handleForceCommand(commandName);
  } else {
    // unknown command
    Serial.println("NACK");
  }
}

void setupSerial() {
  Serial.begin(115200);
  Serial.println();
  Serial.println("flywheelReady");
}

void processSerialInput() {
  if (Serial.available() <= 0) {
    return;
  }
  int incomingByte = Serial.read();
  if (incomingByte == '\n') {
    sRxBuffer[sRxBufferLength] = '\0';
    processSerialBuffer();
    sRxBufferLength = 0;
    return;
  }
  // overflow protection
  if (sRxBufferLength >= RX_BUFFER_SIZE - 1) {
    sRxBufferLength = 0;
  }
  sRxBuffer[sRxBufferLength] = (char)incomingByte;
  sRxBufferLength++;
}
