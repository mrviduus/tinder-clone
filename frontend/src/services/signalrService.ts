import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private messageCallbacks: ((message: any) => void)[] = [];
  private typingCallbacks: ((data: any) => void)[] = [];

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const accessToken = useAuthStore.getState().accessToken;
    if (!accessToken) {
      throw new Error('No access token available');
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:8080/hubs/chat', {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveMessage', (message) => {
      this.messageCallbacks.forEach(callback => callback(message));
    });

    this.connection.on('UserTyping', (data) => {
      this.typingCallbacks.forEach(callback => callback(data));
    });

    this.connection.on('UserStoppedTyping', (data) => {
      this.typingCallbacks.forEach(callback => callback({ ...data, isTyping: false }));
    });

    await this.connection.start();
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async joinMatch(matchId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinMatch', matchId);
    }
  }

  async leaveMatch(matchId: string): Promise<void> {
    // LeaveMatch not implemented on backend - SignalR handles cleanup on disconnect
    // Keeping method for future implementation
  }

  async sendTyping(matchId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Typing', {
        matchId: matchId,
        isTyping: true
      });
    }
  }

  async stopTyping(matchId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Typing', {
        matchId: matchId,
        isTyping: false
      });
    }
  }

  onMessage(callback: (message: any) => void): () => void {
    this.messageCallbacks.push(callback);
    return () => {
      const index = this.messageCallbacks.indexOf(callback);
      if (index > -1) {
        this.messageCallbacks.splice(index, 1);
      }
    };
  }

  onTyping(callback: (data: any) => void): () => void {
    this.typingCallbacks.push(callback);
    return () => {
      const index = this.typingCallbacks.indexOf(callback);
      if (index > -1) {
        this.typingCallbacks.splice(index, 1);
      }
    };
  }

  getConnectionState(): signalR.HubConnectionState {
    return this.connection?.state || signalR.HubConnectionState.Disconnected;
  }
}

export const signalRService = new SignalRService();