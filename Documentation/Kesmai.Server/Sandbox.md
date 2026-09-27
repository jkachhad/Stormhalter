# Kesmai Sandbox Guide

The Sandbox is an isolated Kesmai server instance for testing server and module changes without affecting the shared live server. Each collaborator has a separate instance, connection port, and saved world state.

## Before you start

Ask a server administrator to confirm that:

- your Discord account has a sandbox assigned;
- you have the current sandbox connection details.

The Sandbox is normally managed through Discord slash commands. Type `/sandbox` in the configured Discord server to see the available commands.

## Normal workflow

1. Select the server version with `/sandbox version`.
2. Select module branches with `/sandbox module`.
3. Apply module changes with `/sandbox reset`.
4. Start the instance with `/sandbox start`.
5. Connect using the host and port reported by the start command.
6. Check state or connection details with `/sandbox status`.
7. Stop the instance with `/sandbox stop` when finished.

The selected version and module branches are independent. Changing either one does not automatically restart a running instance.

## Commands

### `/sandbox version`

Displays the installed server versions and lets you select the version for your sandbox.

The selection is stored for your sandbox, but it does not change a server process that is already running. Stop and start the sandbox after changing the version. Existing and new deployments may use either three-part or four-part versions, such as `0.116.3` or `0.116.3.1`.

### `/sandbox module`

Displays the modules configured in your sandbox and lets you choose a branch for a module.

The branch selection applies on the next reset; use `/sandbox reset` before starting again if you want the new branch applied.

Only modules made available by the project team can be selected. Ask an administrator if the module you need is missing.

### `/sandbox reset`

Recreates your sandbox content and applies the selected module branches.

Reset is destructive to changes inside your sandbox instance. Back up any files or data you need before using it. Reset also stops the sandbox if it is running, so start it again afterward.

Use reset when:

- a module branch has changed;
- you want a clean copy of the sandbox;
- the sandbox content is out of sync;
- you changed the selected server version and want to prepare the instance before starting it.

### `/sandbox start`

Starts your isolated server and returns its connection host and port.

If the sandbox is already running, the command returns its existing connection details. A start failure normally means the selected version is unavailable, the sandbox files need a reset, or the server process failed during startup.

### `/sandbox stop`

Stops your sandbox server. Stop the server before changing versions or when you are finished testing.

### `/sandbox status`

Shows:

- whether the sandbox is running;
- the connection host and port;
- the selected server version;
- the version currently running;
- whether a restart is required;
- the configured module branches.

If the selected and running versions differ, stop and start the sandbox to apply the selected version.

## Suggested development loop

For a server or module change, use this sequence:

```text
/sandbox module       # choose the branch to test
/sandbox version      # choose the server build
/sandbox reset        # apply branch and rebuild the sandbox content
/sandbox start        # start the isolated server
/sandbox status       # confirm the selected and running versions
```

After making another change, stop the sandbox if necessary, follow the normal project workflow, reset the sandbox, and start it again.

## Troubleshooting

### “No installed sandbox server versions are available.”

No server versions are currently available for selection. This is an administrator-side deployment problem. Contact the server administrator and include the command, your Discord account, and the time of the failure.

### `/sandbox start` fails immediately

Run `/sandbox status` and note the selected and running versions. If the selected version is unavailable, choose another version with `/sandbox version`. If the sandbox needs to be recreated, use `/sandbox reset`, then start it again.

### Module changes are not visible

Changing a branch does not update an existing sandbox immediately. Use `/sandbox reset` after changing the branch, then confirm the branch shown by `/sandbox status`.

### The server starts but the client cannot connect

Use the host and port returned by `/sandbox start` or `/sandbox status`. Do not reuse an old port from a previous sandbox instance. If the address is correct, send the administrator the sandbox status, command, and time of the failure.

### A reset or start reports a permissions error

Do not change ownership or permissions manually unless you are the server administrator. Report the Discord account, command, timestamp, and complete error message so the deployment permissions can be repaired safely.

## Data and safety notes

- Your sandbox is isolated from other collaborators, but it is still a shared development system.
- Do not put production credentials, private keys, or sensitive account data in sandbox files or module configuration.
- Treat `/sandbox reset` as destructive to mutable sandbox data.
- Stop the sandbox when it is not being used so it does not consume server resources.
- Include the selected version, running version, module branches, and timestamp when reporting a problem.
