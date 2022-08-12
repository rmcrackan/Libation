Streamlined example is in \Source\_Demos\LoadByOS

MUST follow naming conventions in InteropFactory

Windows : Path.GetFileName(a).StartsWithInsensitive("win")
Linux   : Path.GetFileName(a).StartsWithInsensitive("linux")
MacOs   : Path.GetFileName(a).StartsWithInsensitive("mac") || a.StartsWithInsensitive("osx")