# DigiPass

DigiSpark-based keyboard emulator used to quickly type stored passwords.

## Handled platforms

DigiSpark USB or other compatible ATtiny85 boards equiped with USB.

## Features

- each password can be max 31 characters length
- store up to 16 passwords (limited by EEPROM size - 512 bytes)
- 2 buttons/pins assigned for selecting passwords
- use keyboard leds for communication with management software

## Build and deployment

1. Install the DigiSpark USB drivers (ATtiny85)
2. Copy the modified files from folder src/Arduino15 to c:\Users\<profile>\AppData\Local\Arduino15
3. Use Arduino IDE with installed Digistump/Digispark boards to compile and upload sketch from src\Digispark_keyboard\ to the device
4. Use Visual Studio 2022 Community to compile the tool WinDigiPass. This application is used to put passwords into the device.
5. By default the sketch is using only 2 pins for buttons: 0 and 2. Both should be pulled down to ground using some resistor (2kOhm+). And the switches should be connected to VCC.

![image](https://github.com/daniszewski/DigiPass/blob/main/images/DigiSpark1.png)

## Usage instruction

### Programming passwords

1. Connect the device to computer and wait for the bootloader switch to normal keyboard mode (few seconds)
2. Run the WinDigiPass tool
3. Generate or type-in the password and click SEND. 
4. The leds on keyboard will flicker for around 10 seconds. 
5. After that in the textbox you'll see "ok" or "fail"
6. In case of "ok" the device will flicker rapidly for another 10 seconds - this is the time when the password waits in RAM for any switch to be assigned. 
7. Clicking the button on device will store the password in proper block of EEPROM.

### Using passwords

1. Connect device to USB and use one of buttons. The password assigned to it will be keyed in automatically.
