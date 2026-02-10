using System.Reflection;
using Shababeek.Utilities;

namespace Shababeek.Sequencing.Editors
{
    /// <summary>
    /// Provides reflection-based access to BranchCondition's private serialized fields.
    /// Used by both the inspector editor and the graph window's transition detail panel.
    /// </summary>
    internal static class BranchConditionHelper
    {
        private static readonly FieldInfo VariableField =
            typeof(BranchCondition).GetField("variable", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ComparisonField =
            typeof(BranchCondition).GetField("comparison", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo BoolValueField =
            typeof(BranchCondition).GetField("boolValue", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo IntValueField =
            typeof(BranchCondition).GetField("intValue", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo FloatValueField =
            typeof(BranchCondition).GetField("floatValue", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo StringValueField =
            typeof(BranchCondition).GetField("stringValue", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Sets the ScriptableVariable on a BranchCondition.
        /// </summary>
        public static void SetVariable(BranchCondition c, ScriptableVariable v) => VariableField?.SetValue(c, v);

        /// <summary>
        /// Sets the ComparisonType on a BranchCondition.
        /// </summary>
        public static void SetComparison(BranchCondition c, ComparisonType v) => ComparisonField?.SetValue(c, v);

        /// <summary>
        /// Gets the bool target value from a BranchCondition.
        /// </summary>
        public static bool GetBool(BranchCondition c) => BoolValueField != null && (bool)BoolValueField.GetValue(c);

        /// <summary>
        /// Sets the bool target value on a BranchCondition.
        /// </summary>
        public static void SetBool(BranchCondition c, bool v) => BoolValueField?.SetValue(c, v);

        /// <summary>
        /// Gets the int target value from a BranchCondition.
        /// </summary>
        public static int GetInt(BranchCondition c) => IntValueField != null ? (int)IntValueField.GetValue(c) : 0;

        /// <summary>
        /// Sets the int target value on a BranchCondition.
        /// </summary>
        public static void SetInt(BranchCondition c, int v) => IntValueField?.SetValue(c, v);

        /// <summary>
        /// Gets the float target value from a BranchCondition.
        /// </summary>
        public static float GetFloat(BranchCondition c) =>
            FloatValueField != null ? (float)FloatValueField.GetValue(c) : 0f;

        /// <summary>
        /// Sets the float target value on a BranchCondition.
        /// </summary>
        public static void SetFloat(BranchCondition c, float v) => FloatValueField?.SetValue(c, v);

        /// <summary>
        /// Gets the string target value from a BranchCondition.
        /// </summary>
        public static string GetString(BranchCondition c) =>
            StringValueField != null ? (string)StringValueField.GetValue(c) ?? "" : "";

        /// <summary>
        /// Sets the string target value on a BranchCondition.
        /// </summary>
        public static void SetString(BranchCondition c, string v) => StringValueField?.SetValue(c, v);
    }
}
