# FiveRP gamemode repository #

# WARNING
THIS REPO IS AN ARCHIVED REPO FROM OVER 4 YEARS AGO.
* It was based on GTANetwork (the multiplayer mod)
* Unmaintained.
* Do not expect it to launch or run!

## How to set up and compile for server ##
1. Copy the server.cfg file to the root folder of your server directory.

2. Ensure that your environment variable `FIVERP_OUT_DIR` is set to the folder path of your server: `server_roo\resources\fiverp` (no trailing slash!). The build event will move the relevant files into this folder upon building the server, saving you time.

3. Restart VS
4. Build and run.
(make sure to add the gamemode resource to settings.xml first)

5. Use the included SQL file, or set up a connection with the test database. (edit server.cfg)

You will also need to set an environment variable `FIVERP_OUT_DIR` that points
to the resource folder you are using for the gamemode.
Example: `D:\GTANetworkServer\resources\fiverp` (no trailing slash!). This is where the files will
be copied to once you build the gamemode.