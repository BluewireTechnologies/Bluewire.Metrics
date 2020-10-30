using System;
using System.Security.Principal;

namespace Metrics.IISApplicationCounters
{
    internal static class Util
    {
        internal static string GetHelpMessage() => $"The application is currently running as user {GetIdentity()}. Make sure the user has access to the performance counters. The user needs to be either Admin or belong to Performance Monitor user group.";

        private static string GetIdentity()
        {
            try
            {
                return WindowsIdentity.GetCurrent().Name;
            }
            catch (Exception x)
            {
                return "[Unknown user | " + x.Message + " ]";
            }
        }
    }
}
