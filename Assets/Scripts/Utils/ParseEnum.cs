using System;
using UnityEngine;

public static class ParseEnum
{
	public static T Parse<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, true);
	}

	public static T ToEnum<T>(this string value)
	{
		Debug.Assert(Enum.IsDefined(typeof(T), value));
		return (T)Enum.Parse(typeof(T), value, true);
	}
}