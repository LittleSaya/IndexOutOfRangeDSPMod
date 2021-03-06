For other languages of this README, please refer to

[中文说明](https://github.com/LittleSaya/IndexOutOfRangeDSPMod/blob/master/DSPTransportStat/README-zh-Hans.md "中文说明")

# DSP Transport Stat

Press *Ctrl+F* to open/close transport stations window.

Great thanks to authors of LSTM and Unity Explorer.

## Features

- Listing all transport stations in every corner of your galaxy. Showing the location, state and storage of stations.
- Open the Station Window remotely and directly in a list without the need to stand near the station or land on the station's planet.
- Filtering though station type, location, name, items and logistic logic (takes effect when item filter is not empty).
- Sorting by station location and name, ascending or descending.

![Usage](https://raw.githubusercontent.com/LittleSaya/IndexOutOfRangeDSPMod/master/DSPTransportStat/Doc/brief.jpg "Usage")

## Compatibility

### 0.0.8 ~ 0.0.17

Game Version: Early Access 0.9.25.12201, BepInEx: 5.4.19

### 0.0.1 ~ 0.0.7

Game Version: Early Access 0.9.25.12077, BepInEx: 5.4.19

## Known Issues

- Causing null pointer exception when used with LSTM.

## Todo List

Ideas are welcome :-)

## Change log

### 0.0.16 -> 0.0.17

- Fix error when clicking on local transport tower with remote planet's station's window opened

### 0.0.15 -> 0.0.16

- Searching will ignore case

- Improve compatibility with original station window

- Fix a bug that when you try to open a local station remotely in the station list, an interstellar station be opened instead of the local station you want

### 0.0.14 -> 0.0.15

- Add logistic logic filter

### 0.0.13 -> 0.0.14

- Fix possible exception throwing in `Patch_UIStationWindow.OnPlayerIntendToTransferItems()`

### 0.0.12 -> 0.0.13

- Make the station list window respond to esc key

- Fix failing to open station window by clicking on the station tower after closing the station window opened remotely in the station list window

### 0.0.11 -> 0.0.12

- Fix NPE after quiting the game without closing the station list window

### 0.0.10 -> 0.0.11

- Add an option to enable or disable item transfer

- Fix the window failing to open after reloading game save

### 0.0.9 -> 0.0.10

- Fix NullPointerException when transfering items between player inventory and station
- Fix NullPointerException when opening the Transport Stations Window after removing existing stations

### 0.0.8 -> 0.0.9

- Fix null pointer exception after close station window

### 0.0.7 -> 0.0.8

- Fix error when picking up items in player inventory after closing the station window opened from station list.

### 0.0.6 -> 0.0.7

- Fix image url in this README.md file.

### 0.0.5 -> 0.0.6

- Add a button to open the station window.
- Add station count in title bar.
- Add color to item's usage logic text, which is similiar to those in the station window.

- Fix english translations broken.
- Fix some mistakes in station type filtering logic.

### 0.0.4 -> 0.0.5

- Add chinese translations.

### 0.0.3 -> 0.0.4

- Querying and sorting
- Column head

### 0.0.1 -> 0.0.3
- Change shortcut key from Ctrl+T to Ctrl+F to avoid conflict with LSTM's shortcut key.
