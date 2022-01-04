using System;

using UnityEngine.Assertions;

using static pbuddy.LoggingUtility.RuntimeScripts.ContextProvider;

namespace pbuddy.ShaderUtility.EditorScripts
{
    public struct GPUFunctionArgument : IGPUFunctionArgument
    {
        public Type Type { get; }
        public Type ElementType { get; }
        public InputModifier InputModifier { get; }
        public object Value { get; private set; }
        public bool RequiresWriting { get; }
        public bool IsArray { get; }
        public int ElementLength { get; }

        public void SetValue(object updatedValue)
        {
            Assert.IsTrue(updatedValue.GetType() == Type);
            Value = updatedValue;
        }

        internal GPUFunctionArgument(Type type, InputModifier inputModifier, object value)
        {
            Assert.IsTrue(type.IsConvertibleToShaderType(), $"{Context()} {type} type cannot be converted to a type usable in GPU code.");
            Type = type;
            ElementType = type.IsArray ? type.GetElementType() : type;
            InputModifier = inputModifier;
            RequiresWriting = InputModifier == InputModifier.Out || InputModifier == InputModifier.InOut;
            Value = value;
            IsArray = type.IsArray;
            ElementLength = !IsArray ? 1 : ((Array)value)?.Length ?? 0;
        }

        public static GPUFunctionArgument In<T>(T value)
        {
            return new GPUFunctionArgument(typeof(T), InputModifier.In, value);
        }
        
        public static GPUFunctionArgument InOut<T>(T value)
        {
            return new GPUFunctionArgument(typeof(T), InputModifier.InOut, value);

        }
        
        public static GPUFunctionArgument Out<T>()
        {
            return new GPUFunctionArgument(typeof(T), InputModifier.Out, default);
        }
        
        public T GetValue<T>()
        {
            Assert.IsTrue(typeof(T) == Type);
            return (T)Value;
        }
    }
}