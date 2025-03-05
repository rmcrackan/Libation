## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.


## Packaging status

[![Packaging status](https://repology.org/badge/vertical-allrepos/libation.svg)](https://repology.org/project/libation/versions)

New Libation releases are automatically packed into `.deb` and `.rpm` package and are available from the Libation repository's releases page.

Run this command in your terminal to download and install Libation, replacing the url with the latest Libation package url:

### Debian
  ```Console
  wget -O libation.deb https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay.deb &&
  sudo apt install ./libation.deb
  ```
### Redhat and CentOS
  ```Console
  wget -O libation.rpm https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay.rpm &&
  sudo yum install ./libation.rpm
  ```
### Fedora
  ```Console
  wget -O libation.rpm https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay.rpm &&
  sudo dnf5 install ./libation.rpm
  ```
---
### Arch Linux
  ```Console
  yay -S libation
  ```
  This package is available on [Arch User Repository](https://aur.archlinux.org/packages/libation), install via your choice of [AUR helpers](https://wiki.archlinux.org/title/AUR_helpers).
  
  Thanks to [mhdi](https://aur.archlinux.org/account/mhdi) for taking care of AUR package maintenance.
### NixOS
  - Install via `nix-shell`
    ```Console
    nix-shell -p libation
    ```
    A `nix-shell` will temporarily modify your $PATH environment variable. This can be used to try a piece of software before deciding to permanently install it.
  - Install via NixOS configuration
    ```Console
    environment.systemPackages = [
      pkgs.libation
    ];
    ```
    Add the following Nix code to your NixOS Configuration, usually located in `/etc/nixos/configuration.nix`
  - On NixOS via via `nix-env`
    ```Console
    nix-env -iA nixos.libation
    ```
  - On Non NixOS via `nix-env`
    ```Console
    nix-env -iA nixpkgs.libation
    ```
    Warning: Using `nix-env` permanently modifies a local profile of installed packages. This must be updated and maintained by the user in the same way as with a traditional package manager.

    Thanks to [TomaSajt](https://github.com/tomasajt) for taking care of Nix package maintenance.

If your desktop uses gtk, you should now see Libation among your applications.

Additionally, you may launch Libation, LibationCli, and Hangover (the Libation recovery app) via the command line using 'libation, libationcli', and 'hangover' aliases respectively.

Report bugs to https://github.com/rmcrackan/Libation/issues
