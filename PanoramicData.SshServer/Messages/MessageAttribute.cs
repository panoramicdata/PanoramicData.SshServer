using System;

namespace PanoramicData.SshServer.Messages;

/// <summary>
/// Attribute that marks a class as an SSH message with a name and number.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class MessageAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MessageAttribute"/> class.
	/// </summary>
	/// <param name="name">The message name.</param>
	/// <param name="number">The message number.</param>
	public MessageAttribute(string name, byte number)
	{
		ArgumentNullException.ThrowIfNull(name);

		Name = name;
		Number = number;
	}

	/// <summary>
	/// Gets the message name.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the message number.
	/// </summary>
	public byte Number { get; private set; }
}
