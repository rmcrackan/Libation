{pkgs ? import <nixpkgs> {}}: let
  libPath = with pkgs;
    lib.makeLibraryPath [
      # load external libraries that you need in your dotnet project here
      xorg.libX11
      xorg.libICE
      xorg.libSM
      libGL
      fontconfig
    ];
in
  pkgs.mkShell {
    buildInputs = with pkgs; [
      dotnet-sdk_9
    ];
    
    DOTNET_ROOT = "${pkgs.dotnet-sdk_9}/share/dotnet";
    LD_LIBRARY_PATH =libPath;
  }
