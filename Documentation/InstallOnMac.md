## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Run Libation on MacOS
This walkthrough should get you up and running with Libation on your Mac.

## Supports macOS 13 (Ventura) and above

## Install Libation

- Download the file from the latest release and extract it.
  - Apple Silicon (M1, M2, ...): `Libation.x.x.x-macOS-chardonnay-`**arm64**`.tgz`
  - Intel: `Libation.x.x.x-macOS-chardonnay-`**x64**`.tgz`
- Move the extracted Libation app bundle to your applications folder.
- Right-click on Libation and then click on open
- The first time, it will not immediately show you an option to open it. Just dismiss the dialog and do the same thing again (right-click -> open) then you will get an option to run the unsigned application. This takes about 10 seconds.

## If this doesn't work

You can add Libation as a safe app without touching Gatekeeper.

- Copy/paste/run the following command. Adjust the file path to the Libation.app on your computer if necessary.

  ```Console
  xattr -r -d com.apple.quarantine ~/Downloads/Libation.app
  ```
- Close the terminal and use Libation!

## If this still doesn't work

- Copy/paste/run the following command (you'll be prompted to enter your Mac password)

  ```Console
  sudo spctl --master-disable && sudo spctl --add --label "Libation" /Applications/Libation.app && open /Applications/Libation.app && sudo spctl --master-enable
  ```

* Close the terminal and use Libation!

## "Apple can't check app for malicious software"

From: [How to Open Anyway](https://support.apple.com/guide/mac-help/apple-cant-check-app-for-malicious-software-mchleab3a043/mac):

* On your Mac, choose Apple menu > System Settings, then click Privacy & Security in the sidebar. (You may need to scroll down.)
* Go to Security, then click Open.
* Click Open Anyway. This button is available for about an hour after you try to open the app.
* Enter your login password, then click OK.

## Troubleshooting

If Libation fails to start after completing the above steps, try the following:

1. Right-click the Libation app in your applications folder and select _Show Package Contents_
2. Open the `Contents` folder and then the `MacOS` folder.
3. Find the file named `Libation`, right-click it, and then select _Open_.

Libation _should_ launch, and you should now be able to open Libation by just double-clicking the app bundle in your applications folder.


## Running Hangover

Libation comes with a recovery app called Hangover. You can start it by running this command:
```Console
open /Applications/Libation.app --args hangover
```

## Running LibationCli

Libation comes with a command-line interface. Unfortunately, due to the way apps are sandboxed on mac, its use is somewhat limited. To open a new sandboxed terminal in LibationCli's directory, run the following command:
```Console
open /Applications/Libation.app --args cli
```
To use LibationCli from an unsandboxed terminal, you must disable gatekeeper again and run the program directly at `/Applications/Libation.app/Contents/MacOS/LibationCli`

Then use `./LibationCli` to execute a command.

## Get Libation running on Mac

[Run Libation on MacOS](https://user-images.githubusercontent.com/37587114/219271379-a922e4e1-48a0-48e4-bd81-48aa1226a4f5.mp4)
