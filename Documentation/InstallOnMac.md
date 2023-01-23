## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PalPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Run Libation on MacOS
This walkthrough should get you up and running with Libation on your Mac.

## Install Libation

- Download latest MacOS zip to downloads folder
- Extract and rename folder to Libation
- in terminal type cd and then drag your folder of libation to terminal so it looks like `cd/users/YourName/Downloads/Libation`
- Type following commands

```console
chmod +x ./Libation
sudo spctl --add --label "Libation" ./Libation
./Libation
```

## Trouble with Gatekeeper?

If Gatekeeper is giving you trouble with Libation:

Disable the block

`sudo spctl --master-disable`

Launch Libation and login, etc. and allow the rules to update then re-enable the block.

`sudo spctl --master-enable`

Once Gatekeeper reenabled, you can open Libation again without it being blocked.

Thanks [joseph-holland](https://github.com/rmcrackan/Libation/issues/327#issuecomment-1268993349)!

Report bugs to https://github.com/rmcrackan/Libation/issues

## Get Libation running on Mac

[Run Libation on MacOS](https://user-images.githubusercontent.com/37587114/213933357-983d8ede-2738-4b32-9c6e-40de21ff09c2.mp4)
