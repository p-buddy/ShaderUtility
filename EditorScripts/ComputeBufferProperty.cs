using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace pbuddy.ShaderUtility.EditorScripts
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct ComputeBufferProperty
    {
        /// <summary>
        /// 
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public ComputeBuffer Buffer { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="buffer"></param>
        public ComputeBufferProperty(string propertyName, ComputeBuffer buffer)
        {
            PropertyName = propertyName;
            Buffer = buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="length"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ComputeBufferProperty Construct(string name, int length, Type type)
        {
            return new ComputeBufferProperty(name, new ComputeBuffer(length, Marshal.SizeOf(type)));
        } 
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="length"></param>
        /// <typeparam name="TType"></typeparam>
        /// <returns></returns>
        public static ComputeBufferProperty Construct<TType>(string name, int length)
        {
            return Construct(name, length, typeof(TType));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static ComputeBufferProperty Construct(string name, IGPUFunctionArgument argument)
        {
            return Construct(name, argument.ElementLength, argument.ElementType);
        }
        
    }
}