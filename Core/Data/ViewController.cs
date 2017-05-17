using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Tutorial.Core.Octree;
using System;
using System.Linq;

namespace Fusee.Tutorial.Core.Data
{
    #region Enums

    public enum ViewMode
    {
        PointCloud, VoxelSpace
    }

    public enum DebugViewMode // works only with pointcloud or wireframe
    {
        Standard,   // Standard behaviour, render only points
        PerLevel,   // watch points / nodes (bounding box wireframes) per level in octree
        PerNode,    // watch single nodes (points/boundingboxes highlighted in orange) together with nodes on the same level
    }

    public enum ViewEvent // could also be used by android
    {
        None,
        SwitchViewMode = KeyCodes.T,
        SwitchDebugViewMode = KeyCodes.L,
        SwitchWireframe = KeyCodes.K,
        LevelUp = KeyCodes.Up,
        LevelDown = KeyCodes.Down,
        PreviousSibling = KeyCodes.Left,
        NextSibling = KeyCodes.Right,
        Snapshot = KeyCodes.S
    }

    #endregion

    /// <summary>
    /// This class acts as an interface between the UI and the Render Context / Canvas.
    /// </summary>
    public class ViewController
    {
        private bool _wireframeVisible = false;
        private bool _snapshotActive = false;

        private ViewEvent _currentViewEvent = ViewEvent.None;

        private ViewMode _currentViewMode = ViewMode.PointCloud;
        private DebugViewMode _currentDebugViewMode = DebugViewMode.Standard;

        private Octree.Octree _octree;
        private OctreeNode _debugNode;
        private bool _hasDebugNodeChanged = false;

        private const int MAX_LEVEL_DEBUG = 8;

        private int _level = 0; // 0 => root node, 1 => second largest node, etc.
        private bool _showAllLevels = true;

        private KeyboardDevice _keyboard;
        
        /// <summary>
        /// This class needs a reference to the octree.
        /// </summary>
        public ViewController(Octree.Octree octree, KeyboardDevice keyboard)
        {
            _octree = octree;
            _keyboard = keyboard;
        }

        #region Setter

        /// <summary>
        /// Switches the view mode from point cloud to voxelspace and vice versa.
        /// </summary>
        public void SwitchViewMode()
        {
            ViewMode nextViewMode = _currentViewMode == ViewMode.PointCloud ? ViewMode.VoxelSpace : ViewMode.PointCloud;
            _currentViewMode = nextViewMode;
        }

        /// <summary>
        /// Switches the visibility of the wireframe between on and off.
        /// </summary>
        public void SwitchWireframe()
        {
            if(_currentViewMode != ViewMode.VoxelSpace)
                _wireframeVisible = !_wireframeVisible;
        }

        /// <summary>
        /// Iterates between the different debug view modes.
        /// </summary>
        public void SwitchDebugViewMode()
        {
            if (_currentViewMode == ViewMode.VoxelSpace)
                return;

            DebugViewMode nextViewMode;

            switch (_currentDebugViewMode)
            {
                case DebugViewMode.Standard:
                    nextViewMode = DebugViewMode.PerLevel;
                    break;
                case DebugViewMode.PerLevel:
                    nextViewMode = DebugViewMode.PerNode;
                    break;
                case DebugViewMode.PerNode:
                default:
                    nextViewMode = DebugViewMode.Standard;
                    break;
            }

            SetDebugViewMode(nextViewMode);
        }

        /// <summary>
        /// Checks whether some keyboard events have taken place.
        /// </summary>
        public void CheckOnKeyboardEvents()
        {
            var keyboardEvents = Enum.GetValues(typeof(ViewEvent));

            ViewEvent currentEvent = ViewEvent.LevelDown;
            bool isKeyDown = false;

            foreach(ViewEvent keyboardEvent in keyboardEvents)
            {
                if (keyboardEvent == ViewEvent.None)
                    continue;

                KeyCodes keyCode = (KeyCodes) keyboardEvent;

                if(_keyboard.IsKeyDown(keyCode))
                {
                    currentEvent = keyboardEvent;
                    isKeyDown = true;
                    break;
                }
            }

            if (!isKeyDown)
            {
                _currentViewEvent = ViewEvent.None;
                return;
            }
            else
                SetViewEvent(currentEvent);            
        }

        /// <summary>
        /// Either should get called by CheckOnKeyboard or from android user interface.
        /// </summary>
        public void SetViewEvent(ViewEvent viewEvent)
        {
            switch (viewEvent)
            {
                case ViewEvent.SwitchViewMode:
                    SwitchViewMode();
                    break;
                case ViewEvent.SwitchDebugViewMode:
                    SwitchDebugViewMode();
                    break;
                case ViewEvent.SwitchWireframe:
                    SwitchWireframe();
                    break;
                case ViewEvent.LevelUp:
                    LevelUp();
                    break;
                case ViewEvent.LevelDown:
                    LevelDown();
                    break;
                case ViewEvent.PreviousSibling:
                    DebugPreviousSibling();
                    break;
                case ViewEvent.NextSibling:
                    DebugNextSibling();
                    break;
                case ViewEvent.Snapshot:
                    OnSnapshotKeyDown();
                    break;
            }
        }

        #endregion

        #region Getter

        /// <summary>
        /// Returns whether point cloud or voxelspace should be rendered.
        /// </summary>
        public ViewMode GetCurrentViewMode()
        {
            return _currentViewMode;
        }

        /// <summary>
        /// Returns which debug mode should be used for rendering.
        /// </summary>
        public DebugViewMode GetCurrentDebugViewMode()
        {
            return _currentDebugViewMode;
        }

        /// <summary>
        /// Returns whether wireframe should be rendered.
        /// </summary>
        public bool IsWireframeVisible()
        {
            return _wireframeVisible;
        }
        
        /// <summary>
        /// Determines whether the debug node has changed since the last time this function was called.
        /// </summary>
        public bool HasDebugNodeChanged()
        {
            bool hasChanged = _hasDebugNodeChanged;
            _hasDebugNodeChanged = false;

            return hasChanged;
        }

        /// <summary>
        /// Returns the node currently set for debugging.
        /// </summary>
        public OctreeNode GetDebugNode()
        {
            return _debugNode;
        }

        /// <summary>
        /// Returns the level at which to debug.
        /// </summary>
        public int GetCurrentLevel()
        {
            return _showAllLevels ? -1 : _level;
        }

        #endregion

        #region Debugging
        
        /// <summary>
        /// Sets the given debug view mode as active.
        /// </summary>
        private void SetDebugViewMode(DebugViewMode viewMode)
        {
            if (_currentViewMode == ViewMode.VoxelSpace)
                return;

            _currentDebugViewMode = viewMode;

            switch (viewMode)
            {
                case DebugViewMode.PerLevel:
                    _showAllLevels = false;
                    break;
                case DebugViewMode.PerNode:

                    // start debugging via traversing octree
                    _debugNode = _octree.GetRootNode();
                    _hasDebugNodeChanged = true;
                    
                    _level = 0;
                    _showAllLevels = false;

                    break;
                case DebugViewMode.Standard:
                default:
                    _showAllLevels = true;
                    break;
            }
        }

        /// <summary>
        /// Demands the wireframe and the point cloud only to render a specific level.
        /// Level up means bigger nodes.
        /// </summary>
        private void LevelUp()
        {
            if (_currentViewMode == ViewMode.VoxelSpace || _currentDebugViewMode == DebugViewMode.Standard)
                return;

            if (_level == 0)
                return;

            if (_currentDebugViewMode == DebugViewMode.PerNode)
            {
                if (_debugNode.Parent != null)
                {
                    _debugNode = _debugNode.Parent;
                    _hasDebugNodeChanged = true;

                    _level--;
                }
            }
            else if (_currentDebugViewMode == DebugViewMode.PerLevel)
            {
                _level--;
            }
        }

        /// <summary>
        /// Demands the wireframe and the point cloud only to render a specific level.
        /// </summary>
        public void LevelDown()
        {
            if (_currentViewMode == ViewMode.VoxelSpace || _currentDebugViewMode == DebugViewMode.Standard)
                return;

            if (_level >= MAX_LEVEL_DEBUG)
                return;

            if (_currentDebugViewMode == DebugViewMode.PerNode)
            {
                if (_debugNode.hasChildren())
                {
                    _debugNode = _debugNode.Children[0];
                    _hasDebugNodeChanged = true;

                    _level++;
                }
            }
            else if (_currentDebugViewMode == DebugViewMode.PerLevel)
            {
                _level++;
            }
        }

        /// <summary>
        /// Sets the debugging node to its previous sibling if existent.
        /// </summary>
        public void DebugPreviousSibling()
        {
            if (_currentViewMode == ViewMode.VoxelSpace || _currentDebugViewMode != DebugViewMode.PerNode)
                return;

            byte index = _debugNode.Path.Last();

            if (index > 0)
            {
                _debugNode = _debugNode.Parent.Children[index - 1];
                _hasDebugNodeChanged = true;
            }
        }

        /// <summary>
        /// Sets the debugging node to its previous sibling if existent.
        /// </summary>
        public void DebugNextSibling()
        {
            if (_currentViewMode == ViewMode.VoxelSpace || _currentDebugViewMode != DebugViewMode.PerNode)
                return;

            byte index = _debugNode.Path.Last();

            if (index < _debugNode.Parent.Children.Length - 1)
            {
                _debugNode = _debugNode.Parent.Children[index + 1];
                _hasDebugNodeChanged = true;
            }
        }

        /// <summary>
        /// Gets called when the user either takes or releases a snapshot.
        /// </summary>
        public void OnSnapshotKeyDown()
        {
            _snapshotActive = !_snapshotActive;
        }

        /// <summary>
        /// Determines whether snapshot mode is currently active.
        /// </summary>
        public bool IsSnapshotActive()
        {
            return _snapshotActive;
        }

        /// <summary>
        /// Determines whether the user has just taken a snapshot.
        /// </summary>
        public bool HasTakenSnapshot()
        {
            return _currentViewEvent == ViewEvent.Snapshot && _snapshotActive;
        }

        /// <summary>
        /// Determines whether the user has just released the current snapshot.
        /// </summary>
        public bool HasReleasedSnapshot()
        {
            return _currentViewEvent == ViewEvent.Snapshot && !_snapshotActive;
        }

        #endregion
    }
}
