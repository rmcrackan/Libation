{
  description =
    "Generic flake to run shell.nix with a locked nixos pkgs version. The locking happens on first run of 'nix develop'";
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable"; # or your preferred nixpkgs version
  };
  outputs = {nixpkgs, ... }:
    let
      systems = [ "x86_64-linux" "aarch64-linux" "x86_64-darwin" "x86_64-windows"];
    in {
      # This works on all the machines listed in systems array above.
      devShells = builtins.listToAttrs (map (system: {
        name = system;
        value = {
          default = import ./shell.nix {
            pkgs = import nixpkgs { inherit system; };
          };
        };
      }) systems);
    };
}
