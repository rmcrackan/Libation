# Run Libation on Ubuntu

## Dependencies
### Dotnet Runtime
You must install the dotnet 6.0 runtime on your machine.

First, add the Microsoft package signing key to your list of trusted keys and add the package repository.

<details>
  <summary>Ubuntu 22.04</summary>
  
  ```console
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  ```
</details>

<details>
  <summary>Ubuntu 21.10</summary>
  
  ```console
  wget https://packages.microsoft.com/config/ubuntu/21.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  ```
</details>

<details>
  <summary>Ubuntu 20.04</summary>
  
  ```console
  wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  ```
</details>

Then install the dotnet 6.0 runtime

```console
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-runtime-6.0
```
### FFMpeg (Optional)
If you want to convert your audiobooks to mp3, install FFMpeg using the following command:

```console
sudo apt-get install -y ffmpeg
```

## Install Libation

1. Download and extract the most recent linux-64 build.
2. Run Libation, either from the terminal or by double-clicking the Libation file in your desktop environment.
3. Follow the prompts to setup your installation.
4. Report bugs to https://github.com/rmcrackan/Libation/issues
