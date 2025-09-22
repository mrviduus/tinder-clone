import * as signalR from '@microsoft/signalr';
import { Platform } from 'react-native';
import { useAuthStore } from '../store/authStore';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private messageCallbacks: ((message: any) => void)[] = [];
  private typingCallbacks: ((data: any) => void)[] = [];
  private isConnecting = false;

  private getHubUrl(): string {
    if (Platform.OS === 'ios') {
      return 'http://localhost:8080/hubs/chat';
    } else if (Platform.OS === 'android') {
      return 'http://10.0.2.2:8080/hubs/chat';
    } else {
      return 'http://localhost:8080/hubs/chat';
    }
  }

  async connect(): Promise<void> {
    // Check if already connected
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return;
    }

    // Prevent multiple simultaneous connection attempts
    if (this.isConnecting) {
      console.log('SignalR connection already in progress');
      return;
    }

    const accessToken = useAuthStore.getState().accessToken;
    if (!accessToken) {
      throw new Error('No access token available');
    }

    this.isConnecting = true;

    try {
      const hubUrl = this.getHubUrl();
      console.log('Connecting to SignalR hub:', hubUrl);

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => accessToken,
          withCredentials: true,
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up event handlers before starting connection
      this.connection.on('MessageReceived', (message) => {
        console.log('Message received via SignalR:', message);
        this.messageCallbacks.forEach(callback => callback(message));
      });

      this.connection.on('Typing', (data) => {
        console.log('Typing indicator received:', data);
        this.typingCallbacks.forEach(callback => callback(data));
      });

      this.connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
      });

      this.connection.onreconnected(() => {
        console.log('SignalR reconnected');
      });

      this.connection.onclose((error) => {
        console.log('SignalR connection closed', error);
        this.isConnecting = false;
      });

      await this.connection.start();
      console.log('SignalR connected successfully');
    } catch (error) {
      console.error('Failed to connect to SignalR:', error);
      throw error;
    } finally {
      this.isConnecting = false;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async joinMatch(matchId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        console.log('Joining match:', matchId);
        await this.connection.invoke('JoinMatch', matchId);
        console.log('Successfully joined match:', matchId);
      } catch (error) {
        console.error('Failed to join match:', error);
        throw error;
      }
    } else {
      console.warn('Cannot join match - not connected to SignalR');
    }
  }

  async leaveMatch(matchId: string): Promise<void> {
    // LeaveMatch not implemented on backend - SignalR handles cleanup on disconnect
    // Keeping method for future implementation
  }

  async sendTyping(matchId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        console.log('Sending typing indicator for match:', matchId);
        await this.connection.invoke('Typing', {
          matchId: matchId,
          isTyping: true
        });
      } catch (error) {
        console.error('Failed to send typing indicator:', error);
      }
    } else {
      console.warn('Cannot send typing - not connected to SignalR');
    }
  }

  async stopTyping(matchId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        console.log('Stopping typing indicator for match:', matchId);
        await this.connection.invoke('Typing', {
          matchId: matchId,
          isTyping: false
        });
      } catch (error) {
        console.error('Failed to stop typing indicator:', error);
      }
    } else {
      console.warn('Cannot stop typing - not connected to SignalR');
    }
  }

  async sendMessage(matchId: string, text: string, photoId?: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      try {
        console.log('Sending message via SignalR:', { matchId, text });
        await this.connection.invoke('SendMessage', {
          matchId: matchId,
          text: text,
          photoId: photoId
        });
        console.log('Message sent successfully');
      } catch (error) {
        console.error('Failed to send message via SignalR:', error);
        throw error;
      }
    } else {
      throw new Error('Not connected to SignalR');
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

  async ensureConnected(): Promise<void> {
    const maxRetries = 3;
    let retries = 0;

    while (retries < maxRetries) {
      if (this.connection?.state === signalR.HubConnectionState.Connected) {
        return;
      }

      try {
        await this.connect();
        return;
      } catch (error) {
        retries++;
        console.error(`Connection attempt ${retries} failed:`, error);
        if (retries >= maxRetries) {
          throw new Error('Failed to establish SignalR connection after multiple attempts');
        }
        // Wait before retrying
        await new Promise(resolve => setTimeout(resolve, 1000 * retries));
      }
    }
  }
}

export const signalRService = new SignalRService();