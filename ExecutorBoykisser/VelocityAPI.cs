namespace VelocityAPI;

/// <summary>
/// Stub for the real VelocityAPI assembly.
/// Replace this file with the actual VelocityAPI.dll from your Velocity executor installation.
/// </summary>
public enum VelocityStates
{
    Executed,
    Attached,
    NotAttached,
    NoProcessFound,
    TamperDetected,
    Error,
    Attaching,
}

public class VelAPI
{
    public void StartCommunication() { }
    public void StopCommunication() { }
    public async Task Attach(int pid)
    {
        await Task.CompletedTask;
    }
    public VelocityStates Execute(string luaScript)
    {
        return VelocityStates.Executed;
    }
    public bool IsAttached(int pid) => true;
    public string Base64Encode(string input) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
    public static byte[] Base64Decode(string base64) => Convert.FromBase64String(base64);
}
