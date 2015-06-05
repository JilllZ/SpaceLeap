using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class CustomMessage : MessageBase {
    private static Dictionary<short, Type> _idToType = new Dictionary<short, Type>();
    public int senderId;

    public CustomMessage() { }

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

    public static void registerHandler<T>(Action<T> handlerDelegate) where T : CustomMessage, new(){
        NetworkServer.RegisterHandler(getMessageId(typeof(T)), message => {
            T t = new T();
            message.ReadMessage(t);
            handlerDelegate(t);
        });
    }

    public void sendToServer() {
        senderId = CustomLobbyManager.myClient.connection.connectionId;
        CustomLobbyManager.myClient.Send(getMessageId(GetType()), this);
    }

    public void sendToClient(NetworkConnection clientConnection) {
        senderId = -1;
        NetworkServer.SendToClient(clientConnection.connectionId, getMessageId(GetType()), this);
    }

    public void sendToClient(int clientConnectionId) {
        senderId = -1;
        NetworkServer.SendToClient(clientConnectionId, getMessageId(GetType()), this);
    }
}
