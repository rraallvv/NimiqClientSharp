Nimiq Sharp Client
==================

[![build](https://github.com/rraallvv/NimiqClientSharp/workflows/build/badge.svg)](https://github.com/rraallvv/NimiqClientSharp/actions)
[![Nuget Version](https://img.shields.io/nuget/v/NimiqClient)](https://www.nuget.org/packages/NimiqClient/)
[![Maintainability](https://api.codeclimate.com/v1/badges/26e0dcd2f26a87848906/maintainability)](https://codeclimate.com/github/rraallvv/NimiqClientSharp/maintainability)
[![Coverage Status](https://coveralls.io/repos/github/rraallvv/NimiqClientSharp/badge.svg?branch=master)](https://coveralls.io/github/rraallvv/NimiqClientSharp?branch=master)

> C# implementation of the Nimiq RPC client specs.

## Usage

Send requests to a Nimiq node with `NimiqClient` object.

```c#
var config = new Config(
    scheme: "http",
    host: "127.0.0.1",
    port: 8648,
    user: "luna",
    password: "moon"
);

var client = new NimiqClient(config)
```

Once the client have been set up, we can call the methodes with the appropiate arguments to make requests to the Nimiq node.

When no `config` object is passed in the initialization it will use defaults for the Nimiq node.

```c#
var client = NimiqClient();

// make rpc call to get the block number
var blockNumber = await client.blockNumber();

Console.WriteLine(blockNumber) // displays the block number, for example 748883
```

## API

The complete [API documentation](docs) is available in the `/docs` folder.

Check out the [Nimiq RPC specs](https://github.com/nimiq/core-js/wiki/JSON-RPC-API) for behind the scene RPC calls.

## Installation

The recommended way to install Nimiq Sharp Client is adding to the dependencies the Nuget package from the Visual Studio IDE.

## Contributions

This implementation was originally contributed by [rraallvv](https://github.com/rraallvv/).

Please send your contributions as pull requests.

Refer to the [issue tracker](https://github.com/rraallvv/NimiqClientSharp/issues) for ideas.

### Develop

After cloning the repository, open the solucion file in the repository root folder.

All done, happy coding!

### Testing

Tests are stored in the `/NimiqClientTest` folder and can be run from the Visual Studio IDE.

### Documentation

The documentation is generated automatically running [Doxygen](https://www.doxygen.nl/download.html#srcbin) from the repository root directory.

```
C:\NimiqClient>doxygen doxygenfile
```

## License

[MIT](LICENSE)
