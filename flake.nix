{
  description =
    "Generic flake to run shell.nix with a locked nixos pkgs version. If you have
    flakes enabled just run 'nix develop' in the same folder as flake.nix";
  inputs = {
    #// use your preferred nixpkgs version
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable"; 
  };

  outputs = { nixpkgs, ... }:
    let
      pkgs = nixpkgs.legacyPackages.x86_64-linux;
    in {
      devShells.x86_64-linux.default = import ./shell.nix {inherit pkgs;};
    };
}