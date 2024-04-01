using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Client;

/// <summary>Result from a <see cref="IOwopClient.Connect(string, Owop.Client.ConnectionOptions?)"/> call.</summary>
public enum ConnectResult
{
    /// <summary>The client IP address has reached the <see cref="ServerInfo.MaxConnectionsPerIp"/>.</summary>
    LimitReached,
    /// <summary>A connection already exists in this world.</summary>
    Exists, // TODO: Support multiple connections to a single world
    /// <summary>Successful connection.</summary>
    Activated
}
