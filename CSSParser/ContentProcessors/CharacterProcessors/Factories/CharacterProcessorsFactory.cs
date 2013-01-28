using System;
using System.Reflection;

namespace CSSParser.ContentProcessors.CharacterProcessors.Factories
{
	public class CharacterProcessorsFactory : IGenerateCharacterProcessors
	{
		/// <summary>
		/// This will never return null, it will throw an exception if unable to satisfy the request
		/// </summary>
		public T Get<T>(params object[] args) where T : IProcessCharacters
		{
			if (args == null)
				throw new ArgumentNullException("args");

			var type = typeof(T);
			foreach (var constructor in type.GetConstructors())
			{
				var constructorParameters = constructor.GetParameters();
				if (constructorParameters.Length != args.Length)
					continue;

				for (var index = 0; index < constructorParameters.Length; index++)
				{
					if (args[index] == null)
					{
						if (!constructorParameters[index].ParameterType.IsValueType)
							throw new ArgumentException("The type of arg[" + index + "] is invalid (" + constructorParameters[index].ParameterType + " may not be null");
					}
					else
					{
						if (!constructorParameters[index].ParameterType.IsAssignableFrom(args[index].GetType()))
							throw new ArgumentException("The type of arg[" + index + "] is invalid (" + args[index].GetType() + " is not assignable to " + constructorParameters[index].ParameterType);
					}
				}
				return (T)constructor.Invoke(args);
			}

			throw new ArgumentException("Unable to instantiate requested type with specified constructor arguments");
		}
	}
}
