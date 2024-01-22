using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Network;

public enum CaptchaState
{
    Waiting,
    Verifying,
    Verified,
    Ok,
    Invalid
}