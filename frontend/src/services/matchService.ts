import apiClient from '../config/api';
import { Match } from '../types/api';

export class MatchService {
  static async getMatches(page: number = 1, pageSize: number = 50): Promise<Match[]> {
    const response = await apiClient.get<Match[]>('/matches', {
      params: {
        page,
        pageSize,
      },
    });
    return response.data;
  }

  static async getMatchDetails(matchId: string): Promise<Match | null> {
    try {
      const response = await apiClient.get<Match>(`/matches/${matchId}`);
      return response.data;
    } catch (error) {
      return null;
    }
  }

  static async unmatch(matchId: string): Promise<boolean> {
    try {
      await apiClient.delete(`/matches/${matchId}`);
      return true;
    } catch (error) {
      return false;
    }
  }
}