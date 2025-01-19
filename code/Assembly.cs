global using System;
global using System.IO;
global using Sandbox;
global using System.Collections.Generic;
global using System.Runtime.InteropServices;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Threading;
global using System.Text.Json.Serialization;
global using System.Buffers;
global using System.Collections;
global using System.IO.Compression;

global using Boxfish.Utility;

global using static Boxfish.BoxfishGlobal;

namespace Boxfish;

internal static class BoxfishGlobal
{
	internal static Sandbox.Diagnostics.Logger Logger { get; } = new( "Boxfish" );
}
