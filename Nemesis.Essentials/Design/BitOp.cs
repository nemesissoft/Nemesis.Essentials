using JetBrains.Annotations;

namespace Nemesis.Essentials.Design;

[UsedImplicitly]
public static class BitOp
{
    #region Bitwise operations

    public static int LoWord(int n) => n & 0xffff;

    public static int LoWord(IntPtr n) => LoWord(unchecked((int)(long)n));

    public static int HiWord(int n) => (n >> 16) & 0xffff;

    public static int HiWord(IntPtr n) => HiWord(unchecked((int)(long)n));

    public static byte LoByte(short value) => (byte)value;

    public static byte HiByte(short value) => (byte)(value >> 8);

    public static uint RotateLeft(uint value, int count) => (value << count) | (value >> (32 - count));

    public static uint RotateRight(uint value, int count) => (value >> count) | (value << (32 - count));

    /// <summary>
    /// Shifts all bits in b n places to left, and fills the last bit with the first bit (of the byte before shift)
    /// </summary>
    /// <example><![CDATA[
    /// byte b1 = RotateLeft(154, 1); // value of b1: 53
    /// byte b2 = RotateLeft(154, 2); // value of b2: 106
    /// ]]></example>
    public static byte RotateLeft(byte a, byte n) => (byte)(a << n | a >> (8 - n));

    /// <summary>
    /// Shifts all bits in b n places to right, and fills the first bit with the last bit (of the byte before shift)
    /// </summary>
    /// <example><![CDATA[
    /// byte b1 = RotateRight(155, 1); // value of b1: 205
    /// byte b2 = RotateRight(155, 2); // value of b2: 230
    /// ]]></example>
    public static byte RotateRight(byte a, byte n) => (byte)(a >> n | a << (8 - n));

    public static int CountBits(byte b)
    {
        int count = 0;
        while (b > 0)
        {
            if ((b & 1) == 1)
                count++;
            b >>= 1;
        }
        return count;
    }

    #endregion
}
