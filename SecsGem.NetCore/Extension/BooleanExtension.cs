namespace SecsGem.NetCore.Extension
{
    public static class BooleanExtension
    {
        public static byte ToByte(this bool value)
        {
            return (byte)(value ? 1 : 0);
        }
    }
}