using System;

namespace BestHTTP.SocketIO
{
	public enum SocketIOEventTypes
	{
		Unknown = -1,
		Connect,
		Disconnect,
		Event,
		Ack,
		Error,
		BinaryEvent,
		BinaryAck
	}
}
