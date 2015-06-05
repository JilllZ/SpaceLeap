using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class CustomMessage : MessageBase {
    private static Dictionary<short, Type> _idToType = new Dictionary<short, Type>();

    public static short getMessageId(Type messageType) {
        int hashCode = Mathf.Abs(messageType.ToString().GetHashCode());
        int range = short.MaxValue - MsgType.Highest - 3;
        short id = (short)(MsgType.Highest + 1 + (hashCode % range));

        //Id validation
        if (id <= MsgType.Highest) {
            throw new System.Exception("ID should always be less than MstType.Highest");
        }

        Type existingType;
        if (_idToType.TryGetValue(id, out existingType)) {
            if (existingType != messageType) {
                throw new System.Exception("Collision occured with IDs, must be smater!");
            }
        }

        return id;
    }

    public static void registerHandler<T>(Action<T> handlerDelegate) where T : CustomMessage{
        NetworkServer.RegisterHandler(getMessageId(typeof(T)), message => handlerDelegate(message as T));
    }

    public void sendToServer() {
        CustomLobbyManager.myClient.Send(getMessageId(GetType()), this);
    }

    public void sendToClient(NetworkConnection clientConnection) {
        NetworkServer.SendToClient(clientConnection.connectionId, getMessageId(GetType()), this);
    }
}
