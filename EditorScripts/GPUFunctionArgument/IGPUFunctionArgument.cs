using System;

namespace pbuddy.ShaderUtility.EditorScripts
{
    public interface IGPUFunctionArgument
    {
        Type Type { get; }
        Type ElementType { get; }
        InputModifier InputModifier { get; }
        object Value { get; }
        bool RequiresWriting { get; }
        bool IsArray { get; }

        int ElementLength { get; }

        void SetValue(object updatedValue);
        public T GetValue<T>();
    }
}