using Godot;
using Godot.Collections;

[System.Serializable]
public class InputState
{
    public float timestamp;
    public Vector2 moveDirection;
    public bool jumpPressed;
    public Vector2 mouseDelta;
    public Vector3 rotation;
    public Vector3 cameraRotation;
    
    public InputState()
    {
        timestamp = 0f;
        moveDirection = Vector2.Zero;
        jumpPressed = false;
        mouseDelta = Vector2.Zero;
        rotation = Vector3.Zero;
        cameraRotation = Vector3.Zero;
    }
    
    public InputState(float timestamp, Vector2 moveDirection, bool jumpPressed, Vector2 mouseDelta, Vector3 rotation, Vector3 cameraRotation)
    {
        this.timestamp = timestamp;
        this.moveDirection = moveDirection;
        this.jumpPressed = jumpPressed;
        this.mouseDelta = mouseDelta;
        this.rotation = rotation;
        this.cameraRotation = cameraRotation;
    }
    
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            { "timestamp", timestamp },
            { "moveDirection", moveDirection },
            { "jumpPressed", jumpPressed },
            { "mouseDelta", mouseDelta },
            { "rotation", rotation },
            { "cameraRotation", cameraRotation }
        };
    }
    
    public static InputState Deserialize(Dictionary dict)
    {
        return new InputState
        {
            timestamp = dict["timestamp"].AsSingle(),
            moveDirection = dict["moveDirection"].AsVector2(),
            jumpPressed = dict["jumpPressed"].AsBool(),
            mouseDelta = dict["mouseDelta"].AsVector2(),
            rotation = dict["rotation"].AsVector3(),
            cameraRotation = dict["cameraRotation"].AsVector3()
        };
    }
    
    public override string ToString()
    {
        return $"InputState(t: {timestamp:F2}, move: {moveDirection}, jump: {jumpPressed})";
    }
}
