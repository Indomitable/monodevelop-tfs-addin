//
// AssemblyInfo.cs
//
// Author:
//   Joel Reed (joelwreed@gmail.com)
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyVersion ("1.0.0")]
[assembly: SatelliteContractVersion ("1.0.0")]

[assembly: AssemblyTitle("Microsoft.TeamFoundation.dll")]
[assembly: AssemblyDescription("Microsoft.TeamFoundation.dll")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCopyright("(c) 2007 Joel W. Reed")]
[assembly: AssemblyTrademark("")]
#if TARGET_JVM
[assembly: CLSCompliant(false)]
#else
[assembly: CLSCompliant(true)]
#endif

[assembly: ComVisible(false)]
[assembly: AssemblyDefaultAlias("Microsoft.TeamFoundation.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]
