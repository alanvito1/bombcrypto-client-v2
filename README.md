# Bombcrypto Game Client

## Overview

This project is the **Game Client** for the **Bombcrypto** project.  
We have decided to open-source it under the **AGPL (GNU Affero General Public License)**.

Please note that this project **cannot operate as a standalone application**.  
A compatible **server is required** for the client to function correctly.

Most sensitive credentials and configuration values have been **intentionally removed**.  
These values must be provided before the project can be fully compiled,  
or they may be bypassed depending on your experimentation needs.

We will continue to provide updates to minimize friction during setup and testing.

---

## Requirements

- **Unity**: 2022.3  
- **Operating System**: macOS  
- **Target Platform**: WebGL  

---

## Initial Setup

```bash
# AppConfig.json is the main configuration file for the project
# We are unable to provide these values until all related projects are fully open-sourced
cp Assets/Resources/configs/AppConfig.json.sample Assets/Resources/configs/AppConfig.json
```
