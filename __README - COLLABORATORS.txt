Thank you for looking at the code for Libation. Here are some guidelines and expectations for contributors.


IDEALS

* Libation has always been and will always be free and open source.
* I work on this as time permits; this is part of the reason Libation is free. I work as much or as little as I feel like. You should feel empowered to do likewise.
* I welcome collaboration, ideas, and helpful criticism. At the end of the day though, the final call is mine. If you disagree, you're welcome to fork the repo. (git terminology is weird. I didn't want to tell you to go fork yourself, but here we are.)
* I believe deeply in respecting user privacy.
  - As little personally identifiable info is retained as possible. Log files even contain masked user names.
  - Passwords are never stored; only tokens are.
  - Passwords are never displayed or logged. Metadata is allowed. Eg: logging debug info like password length can be useful to identify nulls or blank fields.
  - Nothing is transmitted back to me unless explicitly initiated by the user. So far this isn't done at all, but I could imagine a future feature where the user could choose to transmit log files.
  - The rules are looser for Verbose mode logging and the user is appropriately warned.
* Elephant in the room: piracy. It's impossible to remove DRM without addressing this. I 100% do not endorse or condone piracy. I believe in personal choice and personal responsibility. The user should have the choice of how to access, use, store, manipulate, and backup the content they have purchased. It is their responsibility how they handle this freedom.


STRUCTURE

* Folders in the solution are numbered. Eg: "4 Domain (db)"
* All projects should only refer to other projects in the same folder or to projects in folders with smaller numbers.
* 1 Core Libraries
  This is code which has roughly equivilent priority and knowledge as the BCL. In practice, if code is this universal then it doesn't live here long and is instead moved into Dinah.Core.
* 2 Utilities (domain ignorant)
  Stand-alone libraries with no knowledge of anything having to do with Libation or other programs. In theory any of these should be able to one day be converted to a nuget pkg
* 3 Domain Internal Utilities (db ignorant)
  Can have knowledge of Libation concepts. Cannot access the database.
* 4 Domain (db)
  All database access
* 5 Domain Utilities (db aware)
  This is often where database, domain-ignorant util.s, and non-database capabilities come together to provide the domain-conscious access points
* 6 Application
  GUI, CLI, and shared scaffolding


CODING GUIDELINES

* Follow existing coding style when possible
* No changing things just to be 'cleaner'. You can clean up as you go but 'style wars' serve no one and do not enhance the product. If you go through and change all the namespaces for consistency, I'm going to reject the PR
* I have no tabs vs spaces preference. Do whichever one you want


IDIOSYNCRASIES

* Libation has been a labor of love for years. This passion project has also been a 'forever project' and thus a place to test out new technologies, styles, and ideas. Consistency is not guaranteed. Legacy code is a certainty.


BEING NOTIFIED OF CHANGES

If you're interested in being notified of changes, I recommend subscribing to the github rss feeds:

https://github.com/rmcrackan/Dinah.Core/commits/master.atom
https://github.com/rmcrackan/AudibleApi/commits/master.atom
https://github.com/rmcrackan/Libation/commits/master.atom
