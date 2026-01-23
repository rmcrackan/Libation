# Install on Linux

## Packaging status

[![Packaging status](https://repology.org/badge/vertical-allrepos/libation.svg)](https://repology.org/project/libation/versions)

New Libation releases are automatically packed into `.deb` and `.rpm` package and are available from the [Libation repository's releases page](https://github.com/rmcrackan/Libation/releases).

Run these commands in your terminal to download and install Libation. **Make sure you replace** `X.X.X` with the latest Libation version and `ARCH` with your CPU's architechture (either `amd64` or `arm64`).

### Debian

```bash
wget -O libation.deb https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay-ARCH.deb
sudo apt install ./libation.deb
```

### Redhat and CentOS

```bash
wget -O libation.rpm https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay-ARCH.rpm
sudo yum install ./libation.rpm
```

### Fedora

```bash
wget -O libation.rpm https://github.com/rmcrackan/Libation/releases/download/vX.X.X/Libation.X.X.X-linux-chardonnay-ARCH.rpm
sudo dnf5 install ./libation.rpm
```

---

### AppImage

- Install via [AppMan](https://github.com/ivan-hc/AppMan) (rootless)
  ```bash
  appman -i libation
  ```
- Install via [AM](https://github.com/ivan-hc/AM)
  ```bash
  am -i libation
  ```
Thanks to Package Forge dev [Samuel](https://github.com/Samueru-sama) for [AppImage](https://github.com/pkgforge-dev/Libation-AppImage) maintenence.
  
### Arch Linux

```bash
yay -S libation
```

This package is available on [Arch User Repository](https://aur.archlinux.org/packages/libation), install via your choice of [AUR helpers](https://wiki.archlinux.org/title/AUR_helpers).

Thanks to [mhdi](https://aur.archlinux.org/account/mhdi) for taking care of AUR package maintenance.

### NixOS

- Install via `nix-shell`
  ```bash
  nix-shell -p libation
  ```
  A `nix-shell` will temporarily modify your $PATH environment variable. This can be used to try a piece of software before deciding to permanently install it.
- Install via NixOS configuration
  ```nix
  environment.systemPackages = [
    pkgs.libation
  ];
  ```
  Add the following Nix code to your NixOS Configuration, usually located in `/etc/nixos/configuration.nix`
- On NixOS via via `nix-env`
  ```bash
  nix-env -iA nixos.libation
  ```
- On Non NixOS via `nix-env`

  ```Console
  nix-env -iA nixpkgs.libation
  ```

  Warning: Using `nix-env` permanently modifies a local profile of installed packages. This must be updated and maintained by the user in the same way as with a traditional package manager.

  Thanks to [TomaSajt](https://github.com/tomasajt) for taking care of Nix package maintenance.

### Pacstall

Pacstall is the AUR Ubuntu wishes it had. It takes the concept of the AUR and puts a spin on it, making it easier to install programs without scouring github repos and the likes. See the [Pacstall](https://pacstall.dev/) project for more information.

```bash
pacstall -I libation-deb
```
Thanks to [Tobias Heinlein](https://github.com/niontrix) for Pacstall package maintenance.

---

If your desktop uses gtk, you should now see Libation among your applications.

Additionally, you may launch Libation, LibationCli, and Hangover (the Libation recovery app) via the command line using 'libation, libationcli', and 'hangover' aliases respectively.

Report bugs to https://github.com/rmcrackan/Libation/issues
