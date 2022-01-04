using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

using static pbuddy.LoggingUtility.RuntimeScripts.ContextProvider;

namespace pbuddy.ShaderUtility.EditorScripts
{
    [Serializable]
    public class TestableGPUFunction
    {
        #region Serialized Info (For Saving)
        public string FunctionUnderTestName;
        public string FullPathToFileContainingFunction;
        public string ReturnTypeName;
        public string[] ArgumentTypeNames;
        public string[] InputModifierNames;
        #endregion Serialized Info (For Saving)

        #region Private properties for converting strings to usable types
        private Type[] InputTypeNamesToTypes => ArgumentTypeNames.Select(SupportedShaderTypes.LookUpManagedType).ToArray();
        private InputModifier[] InputModifierStringsToValues => InputModifierNames.Select(InputModiferValueForName).ToArray();
        #endregion  Private properties for converting strings to usable types

        #region RunTime Info
        public Type OutputType { get; private set; }
        public IGPUFunctionArguments FunctionArguments { get; private set; }
        #endregion RunTime Info

        private readonly int hash;
        public TestableGPUFunction(string functionUnderTestName,
                                   string fullPathToFileContainingFunction,
                                   Type outputType,
                                   IGPUFunctionArguments functionArguments)
        {
            FunctionUnderTestName = functionUnderTestName;
            FullPathToFileContainingFunction = fullPathToFileContainingFunction;
            Assert.IsTrue(!outputType.IsArray);
            OutputType = outputType;
            FunctionArguments = functionArguments;
            ReturnTypeName = outputType.FullName;
            ArgumentTypeNames = functionArguments.GetInputTypes().Select(type => type.FullName).ToArray();
            InputModifierNames = functionArguments.GetModifiers().Select(InputModiferNameForValue).ToArray();
            hash = String.Join("", FunctionUnderTestName, FullPathToFileContainingFunction, 
                               String.Join("", InputModifierNames), 
                               String.Join("", ArgumentTypeNames), 
                               ReturnTypeName).GetHashCode();
        }

        public override int GetHashCode() => hash;

        public string GetSaveData() => JsonUtility.ToJson(this, true);

        public static TestableGPUFunction FromSaveData(string saveData)
        {
            object GetDefaultValue(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
            
            var gpuFunction = JsonUtility.FromJson<TestableGPUFunction>(saveData);
            if (gpuFunction is null)
            {
                throw new NullReferenceException($"{Context()}Function could not be retrieved from save data: {saveData}");
            }

            try
            {
                gpuFunction.OutputType = SupportedShaderTypes.LookUpManagedType(gpuFunction.ReturnTypeName);
                Assert.IsTrue(!gpuFunction.OutputType.IsArray);
            }
            catch
            {
                throw new KeyNotFoundException($"{Context()}Return type string of '{gpuFunction.ReturnTypeName}' could not be converted to runtime type.");
            }
            
            Type[] inputTypes = gpuFunction.InputTypeNamesToTypes;
            InputModifier[] inputModifiers = gpuFunction.InputModifierStringsToValues;
            Assert.AreEqual(inputTypes.Length, inputModifiers.Length);
            
            IGPUFunctionArgument GetFunctionArgument(int index)
            {
                Type type = inputTypes[index];
                return new GPUFunctionArgument(type, inputModifiers[index], GetDefaultValue(type));
            }

            IGPUFunctionArgument[] functionArguments = Enumerable.Range(0, inputTypes.Length)
                                                                 .Select(GetFunctionArgument)
                                                                 .ToArray();
            gpuFunction.FunctionArguments = new UnnamedGPUFunctionArguments(functionArguments);
            return gpuFunction;
        }
        
        private static string InputModiferNameForValue(InputModifier modifier) 
            => Enum.GetNames(typeof(InputModifier))[(int)modifier];

        private static InputModifier InputModiferValueForName(string name) =>
            (InputModifier)Enum.Parse(typeof(InputModifier), name);
    }
}