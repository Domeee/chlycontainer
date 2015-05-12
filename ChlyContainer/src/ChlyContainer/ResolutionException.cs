using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChlyContainer
{
    /// <summary>
    /// A ResolutionException is thrown if the resolution for a given type failed.
    /// Meaning the type is not registered with the container.
    /// </summary>
    public class ResolutionException : Exception
    {
        /// <summary>
        /// The unresolvable type.
        /// </summary>
        private readonly Type type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionException" /> class.
        /// </summary>
        /// <param name="type">
        /// The unresolvable type.
        /// </param>
        public ResolutionException(Type type)
        {
            this.type = type;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return string.Format("Resoultion for type {0} failed. {0} is not registered.", this.type);
            }
        }
    }
}
