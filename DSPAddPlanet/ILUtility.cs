using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPAddPlanet
{
    internal static class ILUtility
    {
        static private ManualLogSource logger = null;

        static public void Initialize (ManualLogSource logSource)
        {
            logger = logSource;
        }

        static public void PrintAllCodes (CodeMatcher matcher)
        {
            int tempPos = matcher.Pos;
            matcher.Start();
            StringBuilder codes = new StringBuilder();
            while (matcher.IsValid)
            {
                codes.Append($"\r\n{matcher.Pos,5}: {matcher.Opcode.Name,10} {(matcher.Operand == null ? "" : matcher.Operand.ToString() + '(' + matcher.Operand.GetType() + ')')}");
                matcher.Advance(1);
            }
            matcher.Start();
            matcher.Advance(tempPos);
            logger.LogInfo(codes);
        }

        static public void PrintInt (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintIntInternal");
        }

        static public void PrintInt (CodeMatcher matcher, VariableType variableType, int variableIndex, FieldInfo fieldInfo, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            LoadField(matcher, fieldInfo);
            CallInternalMethod(matcher, "PrintIntInternal");
        }

        static public void PrintFloat (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintFloatInternal");
        }

        static public void PrintByteArrayLength (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintByteArrayLengthInternal");
        }

        static public void PrintByteArrayLength (CodeMatcher matcher, VariableType variableType, int variableIndex, FieldInfo fieldInfo, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            LoadField(matcher, fieldInfo);
            CallInternalMethod(matcher, "PrintByteArrayLengthInternal");
        }

        static public void PrintIntArrayLength (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintIntArrayLengthInternal");
        }

        static public void PrintVector3ArrayLength (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintVector3ArrayLengthInternal");
        }

        static public void PrintVector3 (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintVector3Internal");
        }

        static public void PrintByteArray (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintByteArrayInternal");
        }

        static public void PrintByteArray (CodeMatcher matcher, VariableType variableType, int variableIndex, FieldInfo field, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            LoadField(matcher, field);
            CallInternalMethod(matcher, "PrintByteArrayInternal");
        }

        static public void PrintIntArray (CodeMatcher matcher, VariableType variableType, int variableIndex, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            CallInternalMethod(matcher, "PrintIntArrayInternal");
        }

        static public void PrintIntArray (CodeMatcher matcher, VariableType variableType, int variableIndex, FieldInfo field, string variableName)
        {
            LoadString(matcher, variableName);
            LoadVariable(matcher, variableType, variableIndex);
            LoadField(matcher, field);
            CallInternalMethod(matcher, "PrintIntArrayInternal");
        }

        static private void PrintIntInternal (string name, int value)
        {
            logger.LogInfo($"PrintInt, name: {name}, value: {value}");
        }

        static private void PrintFloatInternal (string name, float value)
        {
            logger.LogInfo($"PrintFloat, name: {name}, value: {value}");
        }

        static private void PrintByteArrayLengthInternal (string name, byte[] value)
        {
            logger.LogInfo($"PrintByteArrayLength, name: {name}, value: {value.Length}");
        }

        static private void PrintIntArrayLengthInternal (string name, int[] value)
        {
            logger.LogInfo($"PrintIntArrayLength, name: {name}, value: {value.Length}");
        }

        static private void PrintVector3ArrayLengthInternal (string name, Vector3[] value)
        {
            logger.LogInfo($"PrintVector3ArrayLength, name: {name}, value: {value.Length}");
        }

        static private void PrintVector3Internal (string name, Vector3 value)
        {
            logger.LogInfo($"PrintVector3, name: {name}, value: ({value.x}, {value.y}, {value.z})");
        }

        static private void PrintByteArrayInternal (string name, byte[] value)
        {
            StringBuilder str = new StringBuilder();
            str.Append($"PrintByteArray, name: {name}, length: {value.Length}, value:");
            for (int i = 0; i < value.Length; ++i)
            {
                str.Append($"\r\n    {i,+9}: {value[i]}");
            }
            logger.LogInfo(str);
        }

        static private void PrintIntArrayInternal (string name, int[] value)
        {
            StringBuilder str = new StringBuilder();
            str.Append($"PrintIntArray, name: {name}, length: {value.Length}, value:");
            for (int i = 0; i < value.Length; ++i)
            {
                if (value[i] == 0) continue;
                str.Append($"\r\n    {i,+9}: {value[i]}");
            }
            logger.LogInfo(str);
        }

        static private void LoadString (CodeMatcher matcher, string str)
        {
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, str));
        }

        static private void LoadVariable (CodeMatcher matcher, VariableType variableType, int variableIndex)
        {
            switch (variableType)
            {
                case VariableType.Local:
                    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, variableIndex));
                    break;
                case VariableType.Argument:
                    matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, variableIndex));
                    break;
                default:
                    throw new Exception("Unknown variableType: " + variableType);
            }
        }

        static private void LoadField (CodeMatcher matcher, FieldInfo fieldInfo)
        {
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, fieldInfo));
        }

        static private void CallInternalMethod (CodeMatcher matcher, string methodName)
        {
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, typeof(ILUtility).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)));
        }

        public enum VariableType
        {
            Local,
            Argument
        }
    }
}
