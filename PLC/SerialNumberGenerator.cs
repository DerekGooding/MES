

namespace MES.PLC;

internal class SerialNumberGenerator
{
    private string _prefix;
    private int _seedNumber;
    private int _digitCount;
    private int _serialCount = 0;
    public List<string> serialNumbers = new List<string>();

    public SerialNumberGenerator(string prefix, int seedNumber, int digitCount, int serialNumberCount)
    {
        _prefix = prefix;
        _seedNumber = seedNumber;
        _digitCount = digitCount;
        _serialCount = serialNumberCount;

    }

    public List<string> GenerateSerialNumbers()
    {
        if (serialNumbers.Count == 0)
        {
            string serialNumber = _prefix + PadNumber(_seedNumber);
            serialNumbers.Add(serialNumber);
        }
        else
        {
            int lastNumber = int.Parse(serialNumbers.First().Substring(_prefix.Length));
            string serialNumber = _prefix + PadNumber(lastNumber + 1);
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
