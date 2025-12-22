# Docker Support

> [!WARNING] Breaking Changes
>
> - The docker image now runs as user 1001 and group 1001. Make sure that the permissions on your volumes allow user 1001 to read and write to them or see the User section below for other options, or if you're not sure.
> - `SLEEP_TIME` is now set to `-1` by default. This means the image will run once and exit. If you were relying on the previous default, you'll need to explicitly set the `SLEEP_TIME` environment variable to `30m` to replicate the previous behavior.
> - The docker image now ignores the values in `Settings.json` for `Books` and `InProgress`. You can now change the folder that books are saved to by using the `LIBATION_BOOKS_DIR` environment variable.

## Disclaimer

The docker image is provided as-is. We hope it can be useful to you but it is not officially supported.

## Configuration

Configuration in Libation is handled by two files, `AccountsSettings.json` and `Settings.json`. These files can usually be found in the Libation folder in your user's home directory. The easiest way to configure these is to run the desktop version of Libation and then copy them into a folder, such as `/opt/libation/config`, that you'll volume mount into the image. `Settings.json` is technically optional, and, if not provided, Libation will run using the default settings. Additionally, the `Books` and `InProgress` settings in `Settings.json` will be ignored and the image will instead substitute it's own values.

## Running

Once the configuration files are copied, the docker image can be run with the following command.

```bash
sudo docker run -d \
  -v /opt/libation/config:/config \
  -v /opt/libation/books:/data \
  --name libation \
  --restart=always \
  rmcrackan/libation:latest
```

By default the container will scan for new books once and download any new ones. This is configurable by passing in a value for the `SLEEP_TIME` environment variable. For example, if you pass in `10m` it will keep running, scan for new books, and download them every 10 minutes.

```bash
sudo docker run -d \
  -v /opt/libation/config:/config \
  -v /opt/libation/books:/data \
  -e SLEEP_TIME='10m' \
  --name libation \
  --restart=always \
  rmcrackan/libation:latest
```

## Environment Variables

| Env Var                    | Default | Description                                                                                                           |
| -------------------------- | ------- | --------------------------------------------------------------------------------------------------------------------- |
| SLEEP_TIME                 | -1      | Length of time to sleep before doing another scan/download. Set to -1 to run one.                                     |
| LIBATION_BOOKS_DIR         | /data   | Folder where books will be saved                                                                                      |
| LIBATION_CONFIG_DIR        | /config | Folder to read configuration from.                                                                                    |
| LIBATION_DB_DIR            | /db     | Optional folder to load database from. If not mounted, will load database from `LIBATION_CONFIG_DIR`.                 |
| LIBATION_DB_FILE           |         | Name of database file to load. By default it will look for all `.db` files and load one if there is only one present. |
| LIBATION_CREATE_DB         | true    | Whether or not the image should create a database file if none are found.                                             |
| LIBATION_CONNECTION_STRING |         | Connection string for Postgresql.                                                                                     |

## User

This docker image runs as user `1001`. In order for the image to function properly, user `1001` must be able to read and write the volumes that are mounted in. If they are not, you will see errors, including [sqlite error](#1060), [Microsoft.Data.Sqlite.SqliteException](#1110), [unable to open database file](#1113), [Microsoft.EntityFrameworkCore.DbUpdateException](#1049)

If you're not sure what your user number is, check the output of the `id` command. Docker should normally run with the number of the user who configured and ran it.

If you want to change the user the image runs as, you can specify `-u <uid>:<gid>`. For example, to run it as user `2000` and group `3000`, you could do the following:

```bash
sudo docker run -d \
  -u 2000:3000 \
  -v /opt/libation/config:/config \
  -v /opt/libation/books:/data \
  --name libation \
  --restart=always \
  rmcrackan/libation:latest
```

If the user it's running as is correct, and it still cannot write, be sure to check whether the files and/or folders might be owned by the wrong user. You can use the `chown` command to change the owner of the file to the correct user and group number, for example: `chown -R 1001:1001 /mnt/audiobooks /mnt/libation-config`

## Advanced Database Options

The docker image supports an optional database mount location defined by `LIBATION_DB_DIR`. This allows the database to be mounted as read/write, while allowing the rest of the configuration files to be mounted as read only. This is specifically useful if running in Kubernetes where you can use Configmaps and Secrets to define the configuration. If the `LIBATION_DB_DIR` is mounted, it will be used, otherwise it will look for the database in `LIBATION_CONFIG_DIR`. If it does not find the database in the expected location, it will attempt to make an empty database there.

## Getting Help

As mentioned above: docker is not officially supported. I'm adding this at the bottom of the page for anyone serious enough to have read this far. If you've tried everything above and would still like help, you can open an [issue](https://github.com/rmcrackan/Libation/issues). Please include `[docker]` in the title. There are also some docker folks who have offered occasional assistance who you can tag within your issue: `@ducamagnifico` , `@wtanksleyjr` , `@CLHatch`.

**Reminder** that these are just friendly users who are sometimes around. They're _not_ our customer support.
