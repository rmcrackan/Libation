## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



# Disclaimer
The docker image is provided as-is. We hope it can be useful to you but it is not officially supported.

### Setup
In order to use the docker image, you'll need to provide it with a copy of the `AccountsSettings.json`, `Settings.json`, and `LibationContext.db` files. These files can usually be found in the Libation folder in your user's home directory. If you haven't run Libation yet, you'll need to launch it to generate these files and setup your accounts. Once you have them, copy these files to a new location, such as `/opt/libation/config`. Before using them we'll need to make a couple edits so that the filepaths referenced are correct when running from the docker image.

In Settings.json, make the following changes:
* Change `Books` to `/data`
* Change `InProgress` to `/tmp` *

*You may have to paste the following at the end of your your Settings.json file if `InProgess` is not present:

```
  "InProgress": "/tmp"
```
![image](https://github.com/patienttruth/Libation/assets/105557996/cf65a108-cadf-4284-9000-e7672c99c59b)


### Running
Once the configuration files are copied and edited, the docker image can be run with the following command.
```
sudo docker run -d \
  -v /opt/libation/config:/config \
  -v /opt/libation/books:/data \
  --name libation \
  --restart=always \
  rmcrackan/libation
```

By default the container will scan for new books every 30 minutes and download any new ones. This is configurable by passing in a value for the `SLEEP_TIME` environment variable. Additionally, if you pass in `-1` it will scan and download books once and then exit.

```
sudo docker run -d \
  -v /opt/libation/config:/config \
  -v /opt/libation/books:/data \
  -e SLEEP_TIME='10m' \
  --name libation \
  --restart=always \
  rmcrackan/libation
```

