namespace pbuddy.ShaderUtility.EditorScripts
{
    /// <summary>
    /// 
    /// </summary>
    public static class IGPUFunctionArgumentsExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputArguments"></param>
        /// <param name="cgFile"></param>
        /// <param name="functionName"></param>
        /// <param name="output"></param>
        /// <typeparam name="TOutput"></typeparam>
        public static void SendToCgFunctionAndGetOutput<TOutput>(this IGPUFunctionArguments inputArguments,
                                                                 string cgFile,
                                                                 string functionName,
                                                                 out TOutput output)
        {
            DebugAndTestGPUCodeUtility.SendArgumentsToCgFunctionAndGetOutput(inputArguments, cgFile, functionName, out output);
        }
    }
}