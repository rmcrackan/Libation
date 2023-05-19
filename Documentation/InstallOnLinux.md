## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/MBucari?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.


### Install and Run Libation on Ubuntu

New Libation releases are automatically packed into .deb and .rpm package and are available from the Libation repository's releases page.


Run this command in your terminal to dowbnload and install Libation, replacing the url with the latest Libation package url:

- Debian
  ```Console
  wget -O libation.deb https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay.deb &&
  sudo apt install ./libation.deb
  ```
- Redhat and CentOS
  ```Console
  wget -O libation.rpm https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay.rpm &&
  sudo yum install ./libation.rpm
  ```


If your desktop uses gtk, you should now see Libation among your applications.

Additionally, you may launch Libation, LibationCli, and Hangover (the Libation recovery app) via the command line using 'libation, libationcli', and 'hangover' aliases respectively.

Report bugs to https://github.com/rmcrackan/Libation/issues
