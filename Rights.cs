internal class Rights
{
    internal static readonly string[] skCalls =
    {
        "skGetId",
        "skLaunchSetup",
        "skLaunch",
        "skRecryptListValid",
        "skRecryptBegin",
        "skRecryptData",
        "skRecryptComputeState",
        "skRecryptEnd",
        "skSignHash",
        "skVerifyHash",
        "skGetConsumption",
        "skAdvanceTicketWindow",
        "skSetLimit",
        "skExit",
        "skKeepAlive"
    };

    public static string GetCalls(uint skRights)
    {
        var list = string.Empty;
        for (int i = 0; i < skCalls.Length; i++)
            if ((skRights & 1 << i) > 0)
                list += list.Length > 0 ? $", {skCalls[i]}" : skCalls[i];
        return list;
    }
}