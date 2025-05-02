# Softalleys.Utilities.Protobuf

A collection of extension methods for converting between [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) geometry types and [Google.Protobuf](https://github.com/protocolbuffers/protobuf) `ByteString`/WKB representations.

## Features

- Convert NetTopologySuite `Geometry` objects to and from Protobuf `ByteString` and WKB byte arrays.
- Support for all major geometry types: `Point`, `LineString`, `Polygon`, `MultiPoint`, `MultiLineString`, `MultiPolygon`, and `GeometryCollection`.
- Simple extension methods for serialization and deserialization.

## Installation

Install via NuGet:

```
dotnet add package Softalleys.Utilities.Protobuf
```

## Usage

Add the following namespaces:

```csharp
using Softalleys.Utilities.Protobuf;
using NetTopologySuite.Geometries;
using Google.Protobuf;
```

### Convert Geometry to ByteString

```csharp
Geometry geometry = ...;
ByteString bytes = geometry.ToByteString();
```

### Convert ByteString to Geometry

```csharp
ByteString bytes = ...;
Geometry? geometry = bytes.ToNetTopologyGeometry();
```

### Convert Byte Array to ByteString

```csharp
byte[] data = ...;
ByteString bytes = data.ToByteString();
```

### Convert ByteString to Specific Geometry Types

```csharp
Point? point = bytes.ToNetTopologyPoint();
LineString? line = bytes.ToNetTopologyLineString();
Polygon? polygon = bytes.ToNetTopologyPolygon();
MultiPoint? multiPoint = bytes.ToNetTopologyMultiPoint();
MultiLineString? multiLine = bytes.ToNetTopologyMultiLineString();
MultiPolygon? multiPolygon = bytes.ToNetTopologyMultiPolygon();
GeometryCollection? collection = bytes.ToNetTopologyGeometryCollection();
```

## License

MIT

## Author

Miguel Matthew Montes de Oca Guzmán / Softalleys S.A. de C.V.

## Repository

[https://github.com/Softalleys/Softalleys.Utilities](https://github.com/Softalleys/Softalleys.Utilities)
