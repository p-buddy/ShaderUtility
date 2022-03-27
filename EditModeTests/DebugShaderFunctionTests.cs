using System.Linq;
using NUnit.Framework;
using Unity.Mathematics;

using pbuddy.ShaderUtility.EditorScripts;

namespace pbuddy.ShaderUtility.EditModeTests
{
    public class DebugShaderFunctionTests
    {
        [Test]
        public void TestFunctionWithInOutVariable()
        {
            const int startingValue = 3;
            const int updateFactor = -1;
            string gpuFunctionToTest = 
@$"float3 SomeFunction(inout int i)
{{
    int before = {startingValue};
    i = i * {updateFactor};
    return float3(before, before, before);
}}";
            
            DebugAndTestGPUCodeUtility.GenerateCgFile(gpuFunctionToTest, out string fileName);

            var input = new NonspecificNamedGPUFunctionArguments
            {
                Argument0 = GPUFunctionArgument.InOut(startingValue)
            };
            
            input.SendToCgFunctionAndGetOutput(fileName, "SomeFunction", out float3 output);
            Assert.AreEqual(output, new float3(startingValue, startingValue, startingValue));
            Assert.AreEqual(input.Argument0.GetValue<int>(), startingValue * updateFactor);
        }
        
        [Test]
        public void TestFunctionWithInVariable()
        {
            const int testValue = 3;

            string gpuFunctionToTest =
@"float3 SomeFunction(int i)
{{
    return float3(i, i, i);
}}";
            
            DebugAndTestGPUCodeUtility.GenerateCgFile(gpuFunctionToTest, out string fileName);

            var input = new NonspecificNamedGPUFunctionArguments
            {
                Argument0 = GPUFunctionArgument.In(testValue)
            };
            
            input.SendToCgFunctionAndGetOutput(fileName, "SomeFunction", out float3 output);
            Assert.AreEqual(output, new float3(testValue, testValue, testValue));
        }
   
        [Test]
        public void TestFunctionWithOutVariable()
        {
            const int valueToSet = 10;
            string gpuFunctionToTest = 
@$"float3 SomeFunction(out int i)
{{
    i = {valueToSet};
    return float3({valueToSet}, {valueToSet}, {valueToSet});
}}";
            
            DebugAndTestGPUCodeUtility.GenerateCgFile(gpuFunctionToTest, out string fileName);

            var input = new NonspecificNamedGPUFunctionArguments
            {
                Argument0 = GPUFunctionArgument.Out<int>()
            };
            
            input.SendToCgFunctionAndGetOutput(fileName, "SomeFunction", out float3 ouput);
            Assert.AreEqual(input.Argument0.GetValue<int>(), valueToSet);
            Assert.AreEqual(ouput, new float3(valueToSet, valueToSet, valueToSet));
        }
        
        [Test]
        public void TestFunctionWithInArrayVariable()
        {
            const string functionName = "ReturnLastElement";
            const int valueToSet = 10;
            const int arraySize = 5;
            
            string gpuFunctionToTest = 
@$"int {functionName}(in int i[{arraySize}])
{{
    return i[{arraySize - 1}];
}}";
            
            DebugAndTestGPUCodeUtility.GenerateCgFile(gpuFunctionToTest, out string fileName);

            var input = new NonspecificNamedGPUFunctionArguments
            {
                Argument0 = GPUFunctionArgument.In(Enumerable.Repeat(valueToSet, arraySize).ToArray())
            };
            
            input.SendToCgFunctionAndGetOutput(fileName, functionName, out int output);
            Assert.AreEqual(valueToSet, output);
        }
        
        [Test]
        public void TestFunctionWithInOutArrayVariable()
        {
            const string functionName = "SquareAndReturnLast";
            const int arraySize = 5;
            const int valueToSet = 10;
            
            string gpuFunctionToTest = 
@$"int {functionName}(inout int arr[{arraySize}])
{{
    const int last = arr[{arraySize - 1}];
    arr[{arraySize - 1}] = last * last;
    return arr[{arraySize - 1}];
}}";
            
            DebugAndTestGPUCodeUtility.GenerateCgFile(gpuFunctionToTest, out string fileName);

            int[] values = Enumerable.Repeat(valueToSet, arraySize).ToArray();
            var input = new NonspecificNamedGPUFunctionArguments
            {
                Argument0 = GPUFunctionArgument.InOut(values)
            };

            int squared = valueToSet * valueToSet;
            input.SendToCgFunctionAndGetOutput(fileName, functionName, out int output);
            Assert.AreEqual(squared, input.Argument0.GetValue<int[]>()[arraySize - 1]);
            Assert.AreEqual(squared, output);
        }
    }
}