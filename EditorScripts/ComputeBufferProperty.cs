using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace pbuddy.ShaderUtility.EditorScripts
{
    public readonly struct ComputeBufferProperty
    {
        public string PropertyName { get; }
        public ComputeBuffer Buffer { get; }
        public ComputeBufferProperty(string propertyName, ComputeBuffer buffer)
        {
            PropertyName = propertyName;
            Buffer = buffer;
        }

        public static ComputeBufferProperty Construct(string name, int length, Type type)
        {
            return new ComputeBufferProperty(name, new ComputeBuffer(length, Marshal.SizeOf(type)));
        } 
        
        public static ComputeBufferProperty Construct<TType>(string name, int length)
        {
            return Construct(name, length, typeof(TType));
        }
        
        public static ComputeBufferProperty Construct(string name, IGPUFunctionArgument argument)
        {
            return Construct(name, argument.ElementLength, argument.ElementType);
        }
        
    }
}