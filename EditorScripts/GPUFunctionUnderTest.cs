using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

using static pbuddy.LoggingUtility.RuntimeScripts.ContextProvider;

namespace pbuddy.ShaderUtility.EditorScripts
{
    /// <summary>
    /// A object that is the 'managed' representation of a GPU function that can/will be tested,
    /// including its <see cref="OutputType"/>
    /// and the <see cref="FunctionArguments"/> that will be passed to it in order to test it.
    /// </summary>
    [Serializable]
    public class GPUFunctionUnderTest
    {
        #region Serialized Info (For Saving -- that's why it's public)
        public string FunctionUnderTestName;
        public string FullPathToFileContainingFunction;
        public string ReturnTypeName;
        public string[] ArgumentTypeNames;
        public string[] InputModifierNames;
        #endregion Serialized Info (For Saving -- that's why it's public)

        #region Private properties for converting strings to usable types
        private Type[] InputTypeNamesToTypes => ArgumentTypeNames.Select(SupportedShaderTypes.LookUpManagedType).ToArray();
        private InputModifier[] InputModifierStringsToValues => InputModifierNames.Select(InputModiferValueForName).ToArray();
        #endregion  Private properties for converting strings to usable types

        #region RunTime Info
        /// <summary>
        /// 
        /// </summary>
        public Type OutputType { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public IGPUFunctionArguments FunctionArguments { get; private set; }
        #endregion RunTime Info

        private readonly int hash;
        
        /// <summary>
        /// Construct a new <see cref="GPUFunctionUnderTest"/>
        /// </summary>
        /// <param name="functionUnderTestName">Name of function being tested</param>
        /// <param name="fullPathToFileContainingFunction">Full path (on system) to file containing the function to test</param>
        /// <param name="outputType">Output type of function under test (NOTE: Arrays are currently not supported)</param>
        /// <param name="functionArguments"><see cref="IGPUFunctionArguments"/> to be passed to function under test in order to test it</param>
        public GPUFunctionUnderTest(string functionUnderTestName,
                                   string fullPathToFileContainingFunction,
                                   Type outputType,
                                   IGPUFunctionArguments functionArguments)
        {
            FunctionUnderTestName = functionUnderTestName;
            FullPathToFileContainingFunction = fullPathToFileContainingFunction;
            Assert.IsTrue(!outputType.IsArray, Context().WithMessage($"Array outputs are not currently supported, and thus {functionUnderTestName} cannot be tested (currently)"));
            OutputType = outputType;
            FunctionArguments = functionArguments;
            ReturnTypeName = outputType.FullName;
            ArgumentTypeNames = functionArguments.GetInputTypes().Select(type => type.FullName).ToArray();
            InputModifierNames = functionArguments.GetModifiers().Select(InputModiferNameForValue).ToArray();
            hash = HashingFunction(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => hash;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSaveData() => JsonUtility.ToJson(this, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveData"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static GPUFunctionUnderTest FromSaveData(string saveData)
        {
            object GetDefaultValue(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
            
            var gpuFunction = JsonUtility.FromJson<GPUFunctionUnderTest>(saveData);
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

        private static int HashingFunction(GPUFunctionUnderTest functionUnderTest)
        {
            return String.Join("", functionUnderTest.FunctionUnderTestName, functionUnderTest.FullPathToFileContainingFunction, 
                        String.Join("", functionUnderTest.InputModifierNames), 
                        String.Join("", functionUnderTest.ArgumentTypeNames), 
                        functionUnderTest.ReturnTypeName).GetHashCode();
        }
    }
}