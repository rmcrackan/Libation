## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PalPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.


# Run Libation on MacOS
This walkthrough should get you up and running with Libation on your Mac.

## Install Libation

- Download the `Libation.app.x.x.x.tar.gz` file from the latest release and extract it. 
- Move the extracted Libation app bundle to your applications folder.
- Open a terminal (Go > Utilities > Terminal)
- In the terminal type the following commands
  - `sudo spctl --add --label "Libation" /Applications/Libation.app` (you'll be prompted to enter your password.)
  - `sudo spctl --master-disable`
- Keep the terminal open and run the Libation app
- Go back to terminal and type the following command
  - `sudo spctl --master-enable`
- Close the terminal

Libation is now registered with gatekeeper and will run even when gatekeeper is turned back on.

## Running Hangover

Libation comes with a recovery app called Hangover. You can start it by running this command:
```Console
open /Applications/Libation.app --args hangover
```

## Runnign LibationCli

Libation comes with a command-line interface. Unfortunately, due to the way apps are sandboxed on mac, its use is somewhat limited. To open a new sandboxed terminal in LibationCli's directory, run the following command:
```Console
open /Applications/Libation.app --args cli
```
To use LibationCli from an unsandboxed terminal, you must disable gatekeeper again and run the program directly at `/Applications/Libation.app/Contents/MacOS/LibationCli`

## Get Libation running on Mac

[Run Libation on MacOS](https://user-images.githubusercontent.com/37587114/213933357-983d8ede-2738-4b32-9c6e-40de21ff09c2.mp4)
