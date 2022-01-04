using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Unity.Mathematics;

using pbuddy.ShaderUtility.EditorScripts;

namespace pbuddy.ShaderUtility.EditModeTests
{
    public class JsonTest
    {
        [Test]
        public void Test()
        {
            Type outputType = typeof(float4x4);
            IGPUFunctionArguments arguments = new NonspecificNamedGPUFunctionArguments
            {
                Argument0 = GPUFunctionArgument.In(0),
                Argument1 = GPUFunctionArgument.Out<float>(),
                Argument2 = GPUFunctionArgument.InOut(new float2())
            };
            TestableGPUFunction function = new TestableGPUFunction("func",
                                                                   Path.Combine("home", "dummy", "file.cginc"),
                                                                   outputType,
                                                                   arguments);
            string saveData = function.GetSaveData();
            function = TestableGPUFunction.FromSaveData(saveData);
            
            Assert.AreEqual(outputType, function.OutputType);
            Type[] inputTypes = arguments.GetInputTypes();
            InputModifier[] inputModifiers = arguments.GetModifiers();
            Assert.IsTrue(inputTypes.SequenceEqual(function.FunctionArguments.GetInputTypes()));
            Assert.IsTrue(inputModifiers.SequenceEqual(function.FunctionArguments.GetModifiers()));
        }
    }
}