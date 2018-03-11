[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib?ref=badge_shield)

<a href="https://scan.coverity.com/projects/danielcrenna-chainlib">
  <img alt="Coverity Scan Build Status"
       src="https://img.shields.io/coverity/scan/14680.svg"/>
</a>

ChainLib
=========

A general purpose blockchain programming library for .NET Standard 2.0.

The goal of the project is to be a useful library for creating blockchains for various purposes, with a specific goal of enabling private, enterprise, and consortium chains,
as well as provide cross-chain compatibility for any blockchain programmed using this library, with pluggable support for other blockchains where needed.

"White label" blockchains may be created using JSON definitions, such as:

```json
{
  "name": "MyChain",
  "storageEngine": "sqlite",
  "storageDirectory": "",
  "genesisBlock": {
    "timestamp": 1465154705,
    "objects": [],
    "previousHash": [ 0 ],
    "hash": [ 223, 200, 53, 69, 156, 0, 241, 84, 112, 105, 230, 141, 19, 145, 92, 120, 96, 73, 218, 216, 195, 150, 243, 213, 69, 192, 77, 148, 75, 47, 111, 149 ]
  },
  "proofOfWork": "none",
  "hash": [ 209, 47, 27, 131, 77, 179, 186, 26, 35, 127, 46, 150, 242, 141, 251, 47, 70, 14, 188, 126, 33, 176, 205, 113, 72, 50, 50, 139, 71, 9, 188, 181 ]
}
```

Third Party Software
====================

Initially based on an idiomatic port of https://github.com/conradoqg/naivecoin for .NET Core 2.0, licensed under Apache 2.0
License terms are available here: https://github.com/conradoqg/naivecoin/blob/master/LICENSE

This code relies on lib-sodium and a custom build of lib-sodium-net for cryptography: https://github.com/adamcaudill/libsodium-net, licensed under MIT
License terms are available here: https://github.com/adamcaudill/libsodium-net/blob/master/LICENSE

WarpWallet support uses self-contained SCrypt and PBKDF2 functions written by James F. Bellinger, licensed under MIT
License terms are available here: https://github.com/danielcrenna/ChainLib/blob/master/src/ChainLib.WarpWallet/Internal/SCrypt.cs

## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib?ref=badge_large)