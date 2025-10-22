

namespace MES.PLC;

internal class SerialNumberGenerator(string prefix, int seedNumber, int digitCount, int serialNumberCount)
{
    private readonly string _prefix = prefix;
    private readonly int _seedNumber = seedNumber;
    private readonly int _digitCount = digitCount;
    private readonly int _serialCount = serialNumberCount;
    public List<string> serialNumbers = [];

    public List<string> GenerateSerialNumbers()
    {
        if (serialNumbers.Count == 0)
        {
            var serialNumber = _prefix + PadNumber(_seedNumber);
            serialNumbers.Add(serialNumber);
        }
        else
        {
            var lastNumber = int.Parse(serialNumbers[0][_prefix.Length..]);
            var serialNumber = _prefix + PadNumber(lastNumber + 1);
            serialNumbers.Insert(0, serialNumber);

            if (serialNumbers.Count > _serialCount)
            {
                serialNumbers.RemoveAt(serialNumbers.Count - 1);
            }

        }

        return serialNumbers;
    }


    private string PadNumber(int number) => number.ToString().PadLeft(_digitCount, '0');
}
