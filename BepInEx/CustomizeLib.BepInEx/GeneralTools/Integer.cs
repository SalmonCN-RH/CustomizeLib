using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeLib.BepInEx.GeneralTools
{
    public struct Integer
    {
        private readonly object _value;
        // 构造方法
        public Integer(sbyte value) => _value = value;
        public Integer(byte value) => _value = value;
        public Integer(short value) => _value = value;
        public Integer(ushort value) => _value = value;
        public Integer(int value) => _value = value;
        public Integer(uint value) => _value = value;
        public Integer(long value) => _value = value;
        public Integer(ulong value) => _value = value;
        public Integer(nint value) => _value = value;
        public Integer(nuint value) => _value = value;

        // 隐式转换
        public static implicit operator Integer(sbyte value) => new(value);
        public static implicit operator Integer(byte value) => new(value);
        public static implicit operator Integer(short value) => new(value);
        public static implicit operator Integer(ushort value) => new(value);
        public static implicit operator Integer(int value) => new(value);
        public static implicit operator Integer(uint value) => new(value);
        public static implicit operator Integer(long value) => new(value);
        public static implicit operator Integer(ulong value) => new(value);
        public static implicit operator Integer(nint value) => new(value);
        public static implicit operator Integer(nuint value) => new(value);

        // 显式转换
        public static explicit operator sbyte(Integer i) => (sbyte)i._value;
        public static explicit operator byte(Integer i) => (byte)i._value;
        public static explicit operator short(Integer i) => (short)i._value;
        public static explicit operator ushort(Integer i) => (ushort)i._value;
        public static explicit operator int(Integer i) => (int)i._value;
        public static explicit operator uint(Integer i) => (uint)i._value;
        public static explicit operator long(Integer i) => (long)i._value;
        public static explicit operator ulong(Integer i) => (ulong)i._value;
        public static explicit operator nint(Integer i) => (nint)i._value;
        public static explicit operator nuint(Integer i) => (nuint)i._value;

        public static Integer operator +(Integer a, Integer b) => new((long)a + (long)b);
        public static Integer operator -(Integer a, Integer b) => new((long)a - (long)b);
        public static Integer operator *(Integer a, Integer b) => new((long)a * (long)b);
        public static Integer operator /(Integer a, Integer b) => new((long)a / (long)b);

        public readonly ValueSign GetSign()
        {
            var value = Convert.ToDecimal(_value);
            if (value > 0) return ValueSign.Positive;
            else if (value == 0) return ValueSign.Zero;
            else return ValueSign.Negative;
        }
        public readonly T To<T>() => (T)_value;
        public readonly object Value() => _value;
        public enum ValueSign
        {
            Positive,
            Zero,
            Negative
        }
    }
}
