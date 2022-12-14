#include <avr/eeprom.h>
#include "DigiKeyboard.h"
// Make sure that you're using the DigiKeyboard library with required modifications as the original
// has no keyboard leds handling code. Big thanks to Danjovic for this!

#define BUTTON0 0
#define BUTTON1 2
#define BUILTIN_LED 1
#define PWD_MAX_LENGTH 32

uint8_t transferedBit = 255;      // currently expected bit number, 255 is used as ending condition
uint8_t transferedCrc = 255;      // checksum is just a sum of 1's in the uploaded stream
uint8_t transferedCrc2 = 255;
uint8_t transferedOK = 0;         // this counter decreases to 0 so the status fades in time
uint8_t transferedERR = 0;        // this counter decreases to 0 so the status fades in time
uint8_t progressState = 0;        // helper variable for status indication by blinking onboard led

char buffer[PWD_MAX_LENGTH];      // main buffer for exchanging passwords between RAM and EEPROM
const char* MESSAGE = NULL;       // used only as a result message for password upload

void keyboardLedChanged(uint8_t ledsOld, uint8_t ledsNew)
{
  // The following code uses keyboard leds to get information from computer it is connected to.
  // The protocol starts from waiting for CAPS-LOCK to be turned on. Then 255 bits are retrieved
  // from switching statuses of NUM-LOCK and SCROLL-LOCK. The SCROLL-LOCK means 0, NUM_LOCK - 1.
  // After it reaches the 255th change of bits, it waits for another change of CAPS-LOCK. If it
  // happens before the process stops and error is indicated on built in LED. If everything went
  // fine the CRC is calculated and compared with last 7 bits of transfered 255. 256'th bit is 
  // not used.
  if((ledsNew & CAPS_LOCK) && !(ledsOld & CAPS_LOCK)) { // if capslock turned on
    transferedBit = transferedCrc = transferedOK = transferedERR = 0; // clear state machine
    memset(buffer, 0, sizeof(buffer)); // zero the buffer
  } 
  else if(transferedBit == 255 && !(ledsNew & CAPS_LOCK) && (ledsOld & CAPS_LOCK) && transferedCrc < 255) {  // if capslock turned off and 255 bits transfered
    bool checksumValid = transferedBit == 255 && (buffer[PWD_MAX_LENGTH-1] & 127) == (transferedCrc & 127); // last bit is not used
    transferedOK = checksumValid ? 100 : 0; // 10 seconds of waiting for user to choose button
    transferedERR = checksumValid ? 0 : 20; // 2 seconds of indicating the error
    transferedCrc2 = buffer[PWD_MAX_LENGTH-1];
    buffer[PWD_MAX_LENGTH-1] = 0; // last char must be 0 to properly end a string
    // send a message via keyboard
    MESSAGE = transferedOK ? "ok\r\n" : "fail\r\n"; // we don't want to block this function for too long so sending messages will be done in main loop
  }
  else if(transferedBit < 255) {
    if((ledsNew ^ ledsOld) & SCROLL_LOCK) { // if scrolllock state has changed
      buffer[transferedBit >> 3] |= 1 << (transferedBit & 7); // set 1 in proper bit of the buffer
      if(transferedBit < (PWD_MAX_LENGTH-1)*8) transferedCrc++; // checksum is calculated without last byte (which IS a checksum)
      transferedBit++;
    }
    else if((ledsNew ^ ledsOld) & NUM_LOCK) { // if numlock state has changed
      transferedBit++;
    }
    else if((ledsNew ^ ledsOld) & CAPS_LOCK) { // if capslock state has changed before end of transfer
      // this error might appear during regular keyboard usage so let's only indicate it via built-in led, who uses a capslock these days anyway? ;)
      transferedERR = 20; // 2 seconds of indicating the error
    }
  }
}

void bufferToEeprom(uint8_t blockNo) { // blockNo: 0-15 because each block is 32 bytes and we have only 512 eeprom bytes
  while (!eeprom_is_ready()) DigiKeyboard.delay(1);
  eeprom_write_block(buffer, (void*)(blockNo*PWD_MAX_LENGTH), PWD_MAX_LENGTH);
}

void eepromToBuffer(uint8_t blockNo) { // blockNo: 0-15 because each block is 32 bytes and we have only 512 eeprom bytes
  while (!eeprom_is_ready()) DigiKeyboard.delay(1);
  eeprom_read_block(buffer, (void*)(blockNo*PWD_MAX_LENGTH), PWD_MAX_LENGTH);
}

void sendKeyStroke(byte keyStroke, byte modifiers) {
  DigiKeyboard.sendKeyPress(0, modifiers);
  DigiKeyboard.sendKeyPress(keyStroke, modifiers);
}

void writeChar(uint8_t chr) {
  uint8_t data = pgm_read_byte_near(ascii_to_scan_code_table + (chr - 8));
  sendKeyStroke(data & 0b01111111, data >> 7 ? MOD_SHIFT_LEFT : 0);
}

void sendUsbKeys(const char* word) {
  size_t size = strlen(word);
  while (size--) writeChar(*word++);
  DigiKeyboard.sendKeyStroke(0);
}

void indicateProgress(uint8_t speed) { // speed: 0 - 5Hz, 1 - 2.5Hz, 2 - 1.5Hz, 4 - 1Hz, ..., 9 - 0.5Hz
  if(++progressState > speed) progressState = 0;
  if(!progressState) digitalWrite(BUILTIN_LED, !digitalRead(BUILTIN_LED));
}
void progressClear() {
  transferedOK = transferedERR = 0;
  digitalWrite(BUILTIN_LED, LOW);
}

void setup() {
  onKeyboardLedChanged = &keyboardLedChanged;
	pinMode(BUTTON0, INPUT);
	pinMode(BUTTON1, INPUT);
	pinMode(BUILTIN_LED, OUTPUT);
}

void loop() {
  if(transferedOK) {
    indicateProgress(0); // indicate waiting for button to save the password (fast)
    if(!--transferedOK) { transferedERR = 19; } // saving timeout, switch to error indicating but no debug (transferedERR < 20)
    // waiting for buttons to save the buffer to proper eeprom block
    if(digitalRead(BUTTON0)) { bufferToEeprom(1); progressClear(); }
    if(digitalRead(BUTTON1)) { bufferToEeprom(2); progressClear(); }
  } else if(transferedERR) {
/* // DEBUG ONLY
    if(transferedERR==20) { // show debug only for 20 
      DigiKeyboard.sendKeyStroke(0);
      DigiKeyboard.delay(50);
      DigiKeyboard.print(transferedCrc2 & 127);
      DigiKeyboard.print("/");
      DigiKeyboard.print(transferedCrc & 127);
      DigiKeyboard.print("\r\n");
      for(uint8_t i=0;i<255;i++) {
        DigiKeyboard.print(((buffer[i>>3] >> (i&7)) & 1) ? '#' : '-');
      }
      buffer[PWD_MAX_LENGTH-1] = transferedCrc2;
      DigiKeyboard.print((buffer[PWD_MAX_LENGTH-1] >>7) ? '#' : '-');
      buffer[PWD_MAX_LENGTH-1] = 0;
      DigiKeyboard.print("\r\n");
      DigiKeyboard.delay(50);
    } */
    indicateProgress(4); // indicate error (slow)
    if(!--transferedERR) { progressClear(); } // finish error indication
  } else {
    // waiting for buttons to render password from eeprom
    if(digitalRead(BUTTON0)) { eepromToBuffer(1); sendUsbKeys(buffer); }
    if(digitalRead(BUTTON1)) { eepromToBuffer(2); sendUsbKeys(buffer); }
  }

  if(MESSAGE) { sendUsbKeys(MESSAGE); MESSAGE = NULL; }

	DigiKeyboard.delay(100);
}
