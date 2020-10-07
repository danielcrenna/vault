
This repository contains a large collection of open source software projects (34!) that I've written, going as far back as 2009.

Many were/are popular libraries downloaded over a hundred thousand times. Many were and still are little libraries I use often when I don't want to write the same code over and over again. 

For various reasons, like technology churn, but mainly the amount of time and attention it takes to curate open source projects, projects come and go, interests wane, relentless requests for help from addled freelancers pile on...

I often utilize these libraries for commercial projects, where needed, as they contain perennial solutions to many common challenges, especially in web development (DI, caching, data connection scoping, bulk copying, et al.).

For the most part, every project in this vault has a separate README file so you get a sense of what it is used for.

However, all code in this repository is considered "final state". No pull requests or issues will be fielded. That said, I do update libraries from time to time if clients permit, I have spare time, or I need to solve my own problem. If you need commercial support or custom development using these libraries, feel free to get in touch.

If you want to use code from this repository, fork, and provide a citation for the project's respective license.

*All licenses and copyrights for respective sub-projects in this repo remain intact.*


A quick note about NuGet:
-------------------
While most of these projects have NuGet libraries, I can't promise a timely upgrade schedule for those packages. 


## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdanielcrenna%2Fvault.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdanielcrenna%2Fvault?ref=badge_large)
=======
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib?ref=badge_shield)

ChainLib
=========

A general purpose blockchain programming library for .NET Standard 2.0.

The goal of the project is to be a useful library for creating blockchains for various purposes, outside of crypto-economics, i.e. no work proofs.

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

Initially based on an idiomatic port of https://github.com/conradoqg/naivecoin for .NET Core, licensed under Apache 2.0
License terms are available here: https://github.com/conradoqg/naivecoin/blob/master/LICENSE

This code relies on lib-sodium for cryptography: https://github.com/adamcaudill/libsodium-net, licensed under MIT.
License terms are available here: https://github.com/adamcaudill/libsodium-net/blob/master/LICENSE

WarpWallet support uses self-contained SCrypt and PBKDF2 functions written by James F. Bellinger, licensed under MIT.
License terms are available here: https://github.com/danielcrenna/ChainLib/blob/master/src/ChainLib.WarpWallet/Internal/SCrypt.cs
