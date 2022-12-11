# DigiPass

DigiSpark-based keyboard emulator used to quickly type stored passwords.

## Handled platforms

DigiSpark USB or other compatible ATtiny85 boards equiped with USB.

## Features

- store up to 16 passwords (limited by EEPROM size - 512 bytes)
- each password can be max 31 characters length
- changing passwords possible wihtout flashing
- 2 buttons/pins assigned for selecting passwords
- communication with management software done by keyboard LED statuses

## Prerequisites for building

1. [Arduino IDE](https://www.arduino.cc/en/Main/Software) or [VSCode](https://code.visualstudio.com/) + [PlatformIO](https://platformio.org/)
2. [Additional boards for above](http://digistump.com/package_digistump_index.json)
3. [USB Drivers](https://github.com/digistump/DigistumpArduino/tree/master/tools)
4. [Visual Studio](https://visualstudio.microsoft.com/vs/community)

## Build and deployment

1. depending on which platform you use, copy the modified files from folder src/ArduinoLibraryFix
 - Arduino15 to c:\Users\<profile>\AppData\Local\Arduino15\packages\digistump\hardware\avr\1.6.7\libraries\DigisparkKeyboard
 - PlatformIO to c:\Users\<profile>\.platformio\packages\framework-arduino-avr-digistump\libraries\DigisparkKeyboard
2. use Arduino IDE with installed Digistump/Digispark boards to compile and upload sketch from src\Digispark_keyboard\ to the device
3. use Visual Studio 2022 Community to compile the tool WinDigiPass. This application is used to put passwords into the device.
4. by default the sketch is using only 2 pins for buttons: 0 and 2. Both should be pulled down to the ground using some resistor (2kOhm+). The switches should be connected to VCC.

![image](https://github.com/daniszewski/DigiPass/blob/main/images/DigiSpark1.png)

## Usage instruction

### Programming passwords

1. connect the device to computer and wait for the bootloader switch to normal keyboard mode (few seconds)
2. run the WinDigiPass tool
3. generate or type-in the password and click SEND. 
4. the leds on keyboard will flicker for around 10 seconds. This way the password is being sent to the device and stored in RAM.
5. after that in the textbox you'll see "ok" or "fail". Nothing means that the device didn't respond. Check if it has been recognized as HID device. "Fail" generally means that there was some disruption and you need to try again and in some cases might be needed to reduce the transfer speed. During tests the speed 32 bytes for 10 seconds was working perfectly.
6. in case of "ok" received from the device the led on it will start flickering rapidly for another 10 seconds - this is the time when the password waits in RAM for any switch to be assigned. 
7. pressing one of the switches will store the password in proper block of EEPROM.

### Using passwords

1. connect device to USB and use one of buttons. The password assigned to it will be keyed in automatically.

## Thanks

Big thanks to [Danjovic](https://github.com/Danjovic) for his great work on adding LED control to DigisparkKeyboard library.
