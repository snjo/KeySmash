# KeySmash

Simulates key presses so you can input text into a remote controlled computer when the clipboard is not available, or manual input is difficult.

**Warning**: If you add a password to the clipboard, this can end up in the remote computer's clipboard and possibly be logged by an application.

## Local hotkeys

F3 - Move application to the mouse position (while the app has focus)

## Global hotkeys
*These can be changed in the settings menu*

Ctrl+Shift+G - Send clipboard text to the app's text field. Max 50 characters.
Ctrl+Shift+K - Type the text from the text field (simulate key presses)

# How to use

- Enter text into the KeySmash text field.
(You can press Ctrl+Shift+G to fill the text field with the current clipboard text)

### Using the Send Text hotkey
- Switch to the target application and press the hotkey Ctrl+Shift+K to input the text.

### Using the Send button
If the hotkey doesn't work, use the Send button instead.

- Click Send
- Switch to the target text field before the timer expires.

The timer delay can be set in the options menu.

# Opions (⚙)

### Hotkeys
Configure the hotkeys for getting clipboard data and sending text.
The full list of key names can be found here: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-7.0

### Delay
Set the delay between pressing the Send button and keystrokes starting

### Color
Set the color of the app background to make it easier to find on screen

### Slow output
Outputs the keystrokes slower in case the receiving system is struggling to keep up.

### Fix caret (^)
Prevents the ^ character from being output as & on Scandinavian/German keyboard layouts. Turn off if using a US keyboard layout.

### Get clipboard also clears clipboard
After using the Get Clipboard hotkey, the clipboard is wiped so that e.g. passwords aren't accidentally left on the clipboard.
