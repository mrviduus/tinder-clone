import apiClient from '../config/api';
import { Message, SendMessageRequest } from '../types/api';

export class MessageService {
  static async getMessages(
    matchId: string,
    before?: Date,
    take: number = 30
  ): Promise<Message[]> {
    const params: any = { take };
    if (before) {
      params.before = before.toISOString();
    }

    const response = await apiClient.get<Message[]>(`/matches/${matchId}/messages`, {
      params,
    });
    return response.data;
  }

  static async sendMessage(request: SendMessageRequest): Promise<Message> {
    const response = await apiClient.post<Message>(
      `/matches/${request.matchId}/messages`,
      request
    );
    return response.data;
  }
}