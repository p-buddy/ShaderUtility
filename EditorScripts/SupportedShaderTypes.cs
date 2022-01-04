using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Newtonsoft.Json;

using pbuddy.StringUtility.RuntimeScripts;
using static pbuddy.LoggingUtility.RuntimeScripts.ContextProvider;

namespace pbuddy.ShaderUtility.EditorScripts
{
    public static class SupportedShaderTypes
    {
        private const string GeneratedFilesDirectory = "TypeSystem";
        private const string SavedSupportedTypesFile = "CustomSupportedShaderTypes.json";

        /// <summary>
        /// 
        /// </summary>
        private static readonly Dictionary<Type, string> ShaderTypeNameByManagedType;
        
        /// <summary>
        /// 
        /// </summary>
        private static readonly Type[] DefaultSupportedManagedTypes;
        
        /// <summary>
        /// 
        /// </summary>
        private static readonly Dictionary<string, Type> SupportedManagedTypeByFullTypeName;
        
        #region Public Function(s)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsConvertibleToShaderType(this Type type)
        {
            return ShaderTypeNameByManagedType.ContainsKey(type) || type.IsArray && ShaderTypeNameByManagedType.ContainsKey(type.GetElementType());
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public static void AssertIsValidShaderType(this Type type)
        {
            Assert.IsTrue(type.IsConvertibleToShaderType(),
                          $"{Context()}{type} is not a type supported on shaders (or hasn't been added yet!)");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetShaderTypeName(this Type type)
        {
            return ShaderTypeNameByManagedType[type];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type LookUpManagedType(string typeName)
        {
            if (typeName.Contains("[]"))
            {
                Type elementType = SupportedManagedTypeByFullTypeName[typeName.RemoveSubString("[]")];
                return elementType.MakeArrayType();
            }
            return SupportedManagedTypeByFullTypeName[typeName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shaderTypeNameIfDifferentThanTypeName"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddSupportForType<T>(string shaderTypeNameIfDifferentThanTypeName = null) where T : struct
        {
            string shaderTypeNameToAdd = shaderTypeNameIfDifferentThanTypeName ?? typeof(T).Name;
            if (ShaderTypeNameByManagedType.TryGetValue(typeof(T), out string alreadyAddedTypeName))
            {
                Assert.AreEqual(alreadyAddedTypeName,
                                shaderTypeNameToAdd,
                                $"{Context()}Attempt to associate managed type {typeof(T).Name} with shader type '{shaderTypeNameToAdd}', " +
                                $"but it has already been associated with the name '{alreadyAddedTypeName}'." +
                                "In order to override this, go adjust the entry in: " +
                                $"{FileGenerator.GetPathToSubFile(Path.Combine(GeneratedFilesDirectory, SavedSupportedTypesFile))}");
                return;
            }

            ShaderTypeNameByManagedType[typeof(T)] = shaderTypeNameToAdd;
            WriteOutCustomSupportedTypes();
        }
        #endregion Public Function(s)

        static SupportedShaderTypes()
        {
            ShaderTypeNameByManagedType = new Dictionary<Type, string>
            {
                // NOTE: Bool is not supported as it is not blittable. A smarter person with the need could figure out what to do/how best to support it...
                {typeof(float), "float"},
                {typeof(float2), "float2"},
                {typeof(float3), "float3"},
                {typeof(float4), "float4"},
                {typeof(uint), "uint"},
                {typeof(int), "int"},
                {typeof(float4x4), "float4x4"},
                {typeof(Matrix4x4), "float4x4"},
            };
            DefaultSupportedManagedTypes = ShaderTypeNameByManagedType.Keys.ToArray();
            AppendCustomSupportedTypes();
            SupportedManagedTypeByFullTypeName = ShaderTypeNameByManagedType.ToDictionary(pair => pair.Key.FullName, pair => pair.Key);
        }

        private static void AppendCustomSupportedTypes()
        {
            string relativeLocation = Path.Combine(GeneratedFilesDirectory, SavedSupportedTypesFile);
            string[] contents = FileGenerator.GetGeneratedFileContents(FileGenerator.GetPathToSubFile(relativeLocation));
            if (contents == null || contents.Length == 0)
            {
                return;
            }
            
            string json = String.Join("", contents);
            var shaderTypeNameByFullManagedTypeName = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            string[] defaultTypeNames = DefaultSupportedManagedTypes.Select(type => type.FullName).ToArray();
            foreach (KeyValuePair<string, string> kvp in shaderTypeNameByFullManagedTypeName)
            {
                if (defaultTypeNames.Contains(kvp.Key))
                {
                    continue;
                }
                Type type = GetType(kvp.Key);
                Assert.IsNotNull(type, $"{Context()}The name of type '{kvp.Key}' could not be converted into a type.");
                ShaderTypeNameByManagedType[type] = kvp.Value;
            }
        }
        
        private static void WriteOutCustomSupportedTypes()
        {
            var shaderTypeNameByFullManagedTypeName = new Dictionary<string, string>(ShaderTypeNameByManagedType.Count);
            foreach (KeyValuePair<Type, string> kvp in ShaderTypeNameByManagedType)
            {
                if (!DefaultSupportedManagedTypes.Contains(kvp.Key))
                {
                    shaderTypeNameByFullManagedTypeName[kvp.Key.FullName] = kvp.Value;
                }
            }

            string json = JsonConvert.SerializeObject(shaderTypeNameByFullManagedTypeName, Formatting.Indented);
            string relativeLocation = Path.Combine(GeneratedFilesDirectory, SavedSupportedTypesFile);
            FileGenerator.GenerateArbitraryFile(json, relativeLocation);
        }
        
        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }
            
            foreach (var domain in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = domain.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }
}