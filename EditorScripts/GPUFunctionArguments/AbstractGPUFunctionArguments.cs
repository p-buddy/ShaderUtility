using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Android;
using UnityEngine.Assertions;

namespace pbuddy.ShaderUtility.EditorScripts
{
    public abstract class AbstractGPUFunctionArguments : IGPUFunctionArguments
    {
        private const int MaxArgumentCount = 10;
        
        protected AbstractGPUFunctionArguments()
        {
            argumentsSetAfterConstruction = new IGPUFunctionArgument[MaxArgumentCount];
        }
        
        protected AbstractGPUFunctionArguments(IGPUFunctionArgument[] arguments)
        {
            this.arguments = arguments;
            values = arguments.Select(input => input.Value).ToArray();
            types = arguments.Select(input => input.Type).ToArray();
            modifiers = arguments.Select(input => input.InputModifier).ToArray();
        }

        #region Private Member Variables
        private readonly IGPUFunctionArgument[] argumentsSetAfterConstruction;
        
        private IGPUFunctionArgument[] arguments;
        private Type[] types;
        private object[] values;
        private InputModifier[] modifiers;
        #endregion Private Member Variables
        
        #region Protected Functions
        protected void SetArgumentAfterConstruction(int index, IGPUFunctionArgument argument) =>
            argumentsSetAfterConstruction[index] = argument;
        protected IGPUFunctionArgument GetArgument(int index) => GetArguments()[index];
        #endregion Protected Functions
        
        /// <summary>
        /// 
        /// </summary>
        public int ArgumentCount => GetArguments().Length;
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Type[] GetInputTypes() => types ??= GetArguments().Select(argument => argument.Type).ToArray();
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object[] GetInputValues() => values ??= GetArguments().Select(argument => argument.Value).ToArray();
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public InputModifier[] GetModifiers() =>
            modifiers ??= GetArguments()
                          .Select(argument => argument.InputModifier)
                          .ToArray();
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IGPUFunctionArgument[] GetArguments() =>
            arguments ??= argumentsSetAfterConstruction.Where(argument => argument != null)
                                                       .ToArray();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedValues"></param>
        public void SetValues(object[] updatedValues)
        {
            Assert.AreEqual(ArgumentCount, updatedValues.Length);
            for (int i = 0; i < ArgumentCount - 1; i++)
            {
                UpdateInputValueAtIndex(updatedValues[i], i);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedValue"></param>
        /// <param name="index"></param>
        public void UpdateInputValueAtIndex(object updatedValue, int index)
        {
            arguments[index].SetValue(updatedValue);
        }
    }
}