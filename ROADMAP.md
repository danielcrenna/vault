TODO/Ideas:
===========
Blockchain:
[ ] Hoist proof system in ChainLib
[ ] Smart contracts
[ ] Side chains

UI:
[ ] Block Explorer
[ ] Coin Logos
[ ] CLI client
[ ] Light client

I/O:
[ ] Pluggable storage
[ ] Bootstrap chain (ala log shipping)
[ ] Database performance improvements
[ ] GetAllBlocks should support Range, or even enforce it
[ ] Performance optimization of block objects using snapshots

Security:
[ ] Need to wipe all working arrays (ala Chaos.NaCl)
[ ] Per-block-object encryption (address level)

Mining:
[ ] Proof of Stake implementation
[ ] Built-in stratum pool miner?

Wallets:
[ ] Passphrase wallet (non-human)
[ ] Vanity addresses (ala vanitygen)
[ ] Offline wallet
[ ] Online wallet
[ ] Brainwallets (improvements ala warpwallet)
[ ] BIP39/44

Peers:
[ ] Full replication (default)
[ ] Partial replication (DHT?)
[ ] Proper peer network (Lidgren, Neo, etc.)
[ ] Peer discovery over UPNP/UDP beacons
[ ] Peer discovery with seeds
[ ] Consensus with Paxos and/or Raft?
[ ] Pluggable consensus?
[ ] Need a way to enforce coin settings are the same between peers (use chain hash)

Misc:
[ ] Fix merkle root hash implementation
[ ] Dynamic serialization for block objects (i.e. custom objects should be auto-serialized)
[ ] 100% unit test coverage
[ ] Audit access of _logger
[ ] Audit access of JsonConvert
[ ] Remove exceptions thrown when they can be handled higher up (see: Blockchain.cs)
[ ] Investigate performance of removing first-class transaction objects
[ ] Remove Index from KeyPair?
[ ] Switch to NSec when it releases?
[ ] coveralls and coverity badges