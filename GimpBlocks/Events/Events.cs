using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimpBlocks
{
    public abstract class InputMessage
    {
        public IInputState InputState;
    }

    public class MoveForward : InputMessage
    {
    }

    public class MoveBackward : InputMessage
    {
    }

    public class MoveLeft : InputMessage
    {
    }

    public class MoveRight : InputMessage
    {
    }

    public class MoveUp : InputMessage
    {
    }

    public class MoveDown : InputMessage
    {
    }

    public class MouseLook : InputMessage
    {
    }

    public class MouseMove : InputMessage
    {
    }

    public class ToggleDrawWireframeSetting : InputMessage
    {
    }

    public class ToggleUpdateSetting : InputMessage
    {
    }

    public class ToggleSingleStepSetting : InputMessage
    {
    }

    public class IncreaseCameraSpeed : InputMessage
    {
    }

    public class DecreaseCameraSpeed : InputMessage
    {
    }

    public class ZoomIn : InputMessage
    {
    }

    public class ZoomOut : InputMessage
    {
    }

    public class ResetCamera : InputMessage
    {
    }

    public class SettingsChanged
    {
    }

    public class GarbageCollect : InputMessage
    {
    }

    public class ToggleInputMode : InputMessage
    {
    }

    public class PlaceBlock : InputMessage
    {
    }

    public class DestroyBlock : InputMessage
    {
    }

    public class ChunkRebuilt
    {
        public Chunk Chunk;
    }

    public class CameraMoved
    {
    }

    public class BlockSelectionChanged
    {
        public Block SelectedBlock;
        public BlockPosition SelectedPlacePosition;
    }

    public class ProfileWorldGeneration : InputMessage
    {
    }

    public class EnabledMouseLookMode
    {
    }

    public class DisabledMouseLookMode
    {
    }
}
