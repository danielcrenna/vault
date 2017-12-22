[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib?ref=badge_shield)

<a href="https://scan.coverity.com/projects/danielcrenna-chainlib">
  <img alt="Coverity Scan Build Status"
       src="https://img.shields.io/coverity/scan/14680.svg"/>
</a>

ChainLib
=========

A general purpose blockchain programming library for .NET Standard 2.0.

The goal of the project is to be a useful library for creating blockchains for various purposes, with a specific goal of enabling private, enterprise, and consortium chains, as well as provide cross-chain compatibility for any blockchain programmed using this library, with pluggable support for other blockchains where needed.

Crypto-economics is not the current focus, though that work will exist under a separate package.

Third Party Software
====================

Initially based on an idiomatic port of https://github.com/conradoqg/naivecoin for .NET Core 2.0, licensed under Apache 2.0
License terms are available here: https://github.com/conradoqg/naivecoin/blob/master/LICENSE

This code relies on lib-sodium and a custom build of lib-sodium-net for cryptography: https://github.com/adamcaudill/libsodium-net, licensed under MIT
License terms are available here: https://github.com/adamcaudill/libsodium-net/blob/master/LICENSE


## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fdanielcrenna%2FChainLib?ref=badge_large)
