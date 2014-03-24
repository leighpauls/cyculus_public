#include "serial_cmd.h"
#include "jaguar_pwm.h"

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

static void handleSetCommand(const char* commandName) {
  // setpoint command, double-confirm the new value
  char firstNumber[NUMBER_MAX_LENGTH];
  char secondNumber[NUMBER_MAX_LENGTH];
  char messageNumber[NUMBER_MAX_LENGTH];
  if (!extractWord(
          firstNumber,
          sRxBuffer + strlen(commandName) + 1,
          NUMBER_MAX_LENGTH)
      || !extractWord(
          secondNumber,
          sRxBuffer + strlen(commandName) + strlen(firstNumber) + 2,
          NUMBER_MAX_LENGTH)
      || strcmp(firstNumber, secondNumber)
      || !extractWord(
          messageNumber,
          sRxBuffer + strlen(commandName) + strlen(firstNumber)*2 + 3,
          NUMBER_MAX_LENGTH)) {
    // corrupted numbers
    Serial.println("NACK");
    return;
  }
  char *endPtr;
  int newSetPoint = strtol(firstNumber, &endPtr, 10);
  if (endPtr != firstNumber + strlen(firstNumber)) {
    // not a number
    Serial.println("NACK");
    return;
  }

  if (setSetPoint(newSetPoint)) {
    // success
    Serial.print("ack ");
    Serial.println(messageNumber);
  } else {
    // not in control mode
    Serial.println("NACK");
  }
}

static void processSerialBuffer() {
  // find the command name
  char commandName[COMMAND_NAME_MAX_LENGTH];
  if (!extractWord(commandName, sRxBuffer, COMMAND_NAME_MAX_LENGTH)) {
    Serial.println("NACK");
    return;
  }

  // decide what to do based on the name
  if (strcmp("set", commandName) == 0) {
    handleSetCommand(commandName);
  } else if (strcmp("fd", commandName) == 0) {
    cmdTryFeedDown();
  } else if (strcmp("s", commandName) == 0) {
    cmdStop();
  } else if (strcmp("control", commandName) == 0) {
    cmdStartControl();
  } else if (strcmp("pos", commandName) == 0) {
    Serial.println(getCount());
  } else if (strcmp("switch", commandName) == 0) {
    Serial.println(getLowerLimitState());
  } else {
    // unknown command
    Serial.println("NACK");
  }
}

void setupSerial() {
  Serial.begin(115200);
  Serial.println();
  Serial.println("jagReady");
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
