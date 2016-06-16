using System;

namespace ChlyContainer
{
  /// <summary>
  ///   A ResolutionException is thrown if the resolution for a given type failed.
  ///   Meaning the type is not registered with the container.
  /// </summary>
  public class ResolutionException : Exception
  {
    /// <summary>
    ///   The unresolvable type.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ResolutionException" /> class.
    /// </summary>
    /// <param name="type">
    ///   The unresolvable type.
    /// </param>
    public ResolutionException(Type type)
    {
      _type = type;
    }

    /// <summary>
    ///   Gets the message.
    /// </summary>
    public override string Message => $"Resoultion for type {_type} failed. {_type} is not registered.";
  }
}