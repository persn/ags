using System.Reflection;

namespace AGS.Editor.Extensions
{
    public static class ObjectExtensions
    {
        private const BindingFlags HIDDEN_MEMBERS_FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Gets a private or protected field instance from a object using reflection.
        /// </summary>
        /// <typeparam name="T">The type we want to cast the result to.</typeparam>
        /// <param name="obj">The object to get the field instance from.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns>An object instance with the field data. Or default value for type if the field could not be found.</returns>
        public static T GetHiddenFieldValue<T>(this object obj, string name)
        {
            return (T)obj.GetType().GetField(name, HIDDEN_MEMBERS_FLAGS).GetValue(obj);
        }

        /// <summary>
        /// Sets a private or protected field instance from a object using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object to get the field instance from.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The new value for the field.</param>
        public static void SetHiddenFieldValue<T>(this object obj, string name, T value)
        {
            obj.GetType().GetField(name, HIDDEN_MEMBERS_FLAGS).SetValue(obj, value);
        }

        /// <summary>
        /// Gets a private or protected property instance from a object using reflection.
        /// </summary>
        /// <typeparam name="T">The type we want to cast the result to.</typeparam>
        /// <param name="obj">The object to get the property instance from.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>An object instance with the property data. Or default value for type if the property could not be found.</returns>
        public static T GetHiddenPropertyValue<T>(this object obj, string name)
        {
            return (T)obj.GetType().GetProperty(name, HIDDEN_MEMBERS_FLAGS).GetValue(obj);
        }

        /// <summary>
        /// Sets a private or protected property instance from a object using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetHiddenPropertyValue<T>(this object obj, string name, T value)
        {
            obj.GetType().GetProperty(name, HIDDEN_MEMBERS_FLAGS).SetValue(obj, value);
        }

        /// <summary>
        /// Invoke hidden methods from a object using reflection and returns the result.
        /// </summary>
        /// <param name="obj">The object to get the method from.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters for the method.</param>
        /// <returns>The result of the invoked method.</returns>
        public static void InvokeHiddenMethod(this object obj, string name, params object[] parameters)
        {
            obj.GetType().GetMethod(name, HIDDEN_MEMBERS_FLAGS).Invoke(obj, parameters);
        }

        /// <summary>
        /// Invoke hidden methods from a object using reflection and returns the result.
        /// </summary>
        /// <param name="obj">The object to get the method from.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameters">The parameters for the method.</param>
        /// <returns>The result of the invoked method.</returns>
        public static T InvokeHiddenMethod<T>(this object obj, string name, params object[] parameters)
        {
            return (T)obj.GetType().GetMethod(name, HIDDEN_MEMBERS_FLAGS).Invoke(obj, parameters);
        }
    }
}
