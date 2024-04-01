using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Client;

/// <summary>Represents options for an individual <see cref="IWorldConnection"/>.</summary>
public class ConnectionOptions()
{
    /// <summary>A password to log in with when the client is ready.</summary>
    public string? Password;
    /// <summary>A nickname to set when the client is ready.</summary>
    public string? Nickname;
}
