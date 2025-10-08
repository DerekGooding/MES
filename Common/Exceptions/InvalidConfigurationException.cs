

namespace MES.Common;

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException() : base("Data type is invalid.")
    { }
    public InvalidConfigurationException(string message) : base(message)
    { }
}
