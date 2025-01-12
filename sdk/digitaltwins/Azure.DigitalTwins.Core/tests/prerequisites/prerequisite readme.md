# Prerequisites

## Install

### 1. Install the latest Azure CLI package

- If already installed, check latest version:
  - Run `az --version` to make sure `azure-cli` is at least **version 2.0.8**
  - If it isn't, update it
- Use this link to install [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest])

### 2. Install the ADT extension

ADT's CLI commands are not standard in the Azure CLI package yet, so you must first download the latest extension

- Download the [latest snapshot](https://github.com/Azure/azure-digital-twins/tree/private-preview/CLI) of the ADT enabled IoT CLI extension (a .whl file)
- Open Windows Powershell at the location you downloaded the extension to
- Run `az extension list`
  - If you have **azure-iot** or **azure-cli-iot-ext** installed, remove both with `az extension remove --name azure-iot` (current alias) and `az extension remove --name azure-cli-iot-ext` (legacy alias)
- Add the new extension with `az extension add -y --source <whl-filename>`
- See the top-level ADT commands with `az dt -h`

### 3. Whitelist subscription

To access the digital twins service in your subscription, it must be whitelisted for private preview access. Ask on Teams for a contact.

## Delete

To delete the digital twins instance, you need to first delete the endpoint added by the script (the service doesn't yet support cascading delete).

1. To do this, run the command `az dt endpoint delete -n <dt name> -g <rg name> --en someEventHubEndpoint`.
1. If you have other endpoints that have been added outside this script, you can discover them with the command `az dt endpoint list -n <dt name> -g <rg name>`.
  - Then delete them with the same command in step 1.

## Maintenance

In order to maintain the functionality of the Setup.ps1 file, make sure this document stays updated with all the required changes if you run/alter this script.

Currently, this script works with version 0.0.1.dev9.
