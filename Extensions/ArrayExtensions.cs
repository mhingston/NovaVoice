namespace NovaVoice.Extensions;

public static class ArrayExtensions
{
    public static byte[] ToByteArray(this short[] shortArray)
    {
        var byteArray = new byte[shortArray.Length * 2];
        
        for (var i = 0; i < shortArray.Length; i++)
        {
            var bytes = BitConverter.GetBytes(shortArray[i]);
            
            if (BitConverter.IsLittleEndian)
            {
                byteArray[i * 2] = bytes[1];
                byteArray[i * 2 + 1] = bytes[0];
            }
            
            else
            {
                byteArray[i * 2] = bytes[0];
                byteArray[i * 2 + 1] = bytes[1];
            }
        }
        
        return byteArray;
    }
    
    public static short[] ToShortArray(this byte[] byteArray)
    {
        var shortArray = new short[byteArray.Length / 2];
        
        for (var i = 0; i < shortArray.Length; i++)
        {
            if (BitConverter.IsLittleEndian)
            {
                shortArray[i] = BitConverter.ToInt16([byteArray[i * 2 + 1], byteArray[i * 2]], 0);
            }
            else
            {
                shortArray[i] = BitConverter.ToInt16([byteArray[i * 2], byteArray[i * 2 + 1]], 0);
            }
        }
        
        return shortArray;
    }
}