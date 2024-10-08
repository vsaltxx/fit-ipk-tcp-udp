# Changelog

## [Unreleased]

### Added
- Implemented full functionality for TCP client
- Added proper communication for messages with types Auth, Msg, Bye, Reply, Confirm for UDP client
- Implemented client-side instructions: /auth, /join, /rename, /help

### Changed
- Updated basic argument parsing for TCP client
- Reworked code for better readability and maintainability

### Removed
- Removed redundant code and unused variables from UDP client implementation
- Removed outdated comments and documentation from the codebase

## [Version 1.0.0] - 2024-04-01

### TCP
- Full functionality implemented
- Basic argument parsing

### Additional TCP Functionality
- When the client inputs "BYE" into the console for TCP client, the program now terminates

### UDP
- Proper communication for messages with types Auth, Msg, Bye, Reply, Confirm implemented
- ERR message handling not added
- Basic regular functionality implemented

### Common Implementation for UDP and TCP
- Client-side instructions implemented: /auth, /join, /rename, /help

### Limitations
- In some cases, the program incorrectly responds to the Ctrl+C signal


