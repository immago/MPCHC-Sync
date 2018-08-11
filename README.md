## MPCHC-Sync
Program for synchronizing the status of multiple copies of Media Player Classic by Internet

### Installation
MPCHC-Sync **needs** [server](https://github.com/immago/MPCHC-Sync-Server) 
1. [Download last release](https://github.com/immago/MPCHC-Sync/releases)
2. Unpack, launch MPCHC-Sync.exe
3. Allow connection firewall (if requested)
4. In configuration set: 
- address (ex. google.com), 
- port (ex. 5000), 
- auth token (ex. 91d7636f-8716-4393-9fea-27e0d0bdbfea)
- Media Player Classic address (optional)

### Usage
1. Download the video locally to all computers.
2. The first person clicks "New session ..." and selects the file. Then it copies the identifier and passes it to other users.
3. The following users can join the session by inserting an identifier and by clicking the Join button (to the left of the identifier)
4. You can then use the player as usual

### Known issues
1. If the settings do not apply, restart the player.
2. Invalid json. Reconnect to the server.
3. Untrasted developer. Press 'more info' - 'run anyway'
