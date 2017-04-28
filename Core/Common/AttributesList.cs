using Fusee.Engine.Core;
using Fusee.Math.Core;
using System.Collections.Generic;

namespace Fusee.Tutorial.Core.Common
{
    /// <summary>
    /// Holds and manages a list of <see cref="DynamicAttributes"/>. Because one giant array buffer isn't appropriate or fast enough to
    /// read from and write to, we need to separate them into several buffers.
    /// </summary>
    public class AttributesList
    {
        private List<DynamicAttributes> _buffers; // where all full meshes are stored
        private DynamicAttributes _currentBuffer; // to this mesh everything gets added

        private int _limit; // defines how many vertices can be stored by one buffer object

        /// <summary>
        /// Initializes the list.
        /// </summary>
        /// <param name="limit">Defines how many vertices can be stored by one buffer object.</param>
        public AttributesList(int limit)
        {
            _limit = limit;

            _buffers = new List<DynamicAttributes>();
            _currentBuffer = new DynamicAttributes(_limit);
        }

        /// <summary>
        /// Adds another attribute to the whatever this is.
        /// </summary>
        /// <param name="attribute">offset respectively position</param>
        public void AddAttribute(float3 attribute)
        {
            if(!_currentBuffer.AddAttribute(attribute))
            {
                _buffers.Add(_currentBuffer);
                _currentBuffer = new DynamicAttributes(_limit);
                _currentBuffer.AddAttribute(attribute);
            }
        }

        /// <summary>
        /// Adds multiple attributes to the whatever this is.
        /// </summary>
        /// <param name="attributes">List of attributes respectively positions.</param>
        public void AddAttributes(List<float3> attributes)
        {
            if (!_currentBuffer.AddAttributes(attributes))
            {
                _buffers.Add(_currentBuffer);
                _currentBuffer = new DynamicAttributes(_limit);
                _currentBuffer.AddAttributes(attributes);
            }
        }

        /// <summary>
        /// Returns all meshes contained by this class.
        /// </summary>
        public List<DynamicAttributes> GetAttributesList()
        {
            List<DynamicAttributes> bufferList = new List<DynamicAttributes>();

            bufferList.AddRange(_buffers);
            bufferList.Add(_currentBuffer);

            return bufferList;
        }
    }
}
